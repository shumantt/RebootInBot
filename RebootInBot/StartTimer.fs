module RebootInBot.StartTimer

open System
open RebootInBot.Types
open RebootInBot.Helpers
open RebootInBot.Mentions

let private createTimerProcess sendFinished updateMessage checkIsCancelled deleteProcess config =
    let rec doUpdates count =
        async {
            let cancelled = checkIsCancelled ()

            if not cancelled then
                if count > 0 then
                    do! Async.Sleep config.Delay
                    updateMessage (sprintf "Перезапуск через %i" count)
                    do! doUpdates (count - 1)
                else
                    sendFinished "Поехали!" |> ignore
                    deleteProcess ()
        }

    doUpdates config.CountsNumber

let buildStartTimerCommand (message: IncomingMessage): StartTimerCommand =
    { Chat = message.Chat
      Starter = message.Author }

let private sendToChat sendMessage (startTimer: StartTimerCommand) = sendMessage startTimer.Chat

let private sendToChatWithStarter sendMessage (startTimer: StartTimerCommand) =
    sendMessage startTimer.Chat (startTimer.Starter |> Seq.singleton)

let processStartTimer getParticipants sendMessage updateMessage saveProcess getProcess deleteProcess config startTimer =
    let sendToChat = sendToChat sendMessage startTimer

    let sendToChatWithStarter =
        sendToChatWithStarter sendMessage startTimer

    let saveProcess () =
        let chatProcess =
            { ChatId = startTimer.Chat.ChatId
              Starter = startTimer.Starter }

        saveProcess chatProcess

    let deleteProcess () = deleteProcess startTimer.Chat.ChatId

    let checkIsCancelled () =
        let chatProcess = getProcess startTimer.Chat.ChatId

        match chatProcess with
        | Some _ -> false
        | None -> true

    getParticipants startTimer.Chat
    |> buildMentionList startTimer.Starter
    |> fun mentions ->
        sendToChat mentions "Буду перезапускать, никто не против?"
        |> ignore

        sendToChat mentions "Начинаю обратный отсчет"
    |> fun messageId ->
        let updateMessage = updateMessage startTimer.Chat messageId
        saveProcess ()
        createTimerProcess sendToChatWithStarter updateMessage checkIsCancelled deleteProcess config

let processThrottled sendMessage (startTimer: StartTimerCommand) =
    sendToChatWithStarter sendMessage startTimer "Не можем обработаь ваш запрос"

let processRunning sendMessage (startTimer: StartTimerCommand) =
    sendToChatWithStarter sendMessage startTimer "Процесс уже запущен"

let countDown (sendMessage: SendMessage)
              (updateMessage: UpdateMessage)
              (getParticipants: GetParticipants)
              (getTimer: GetTimer)
              (stopTimer: StopTimer)
              (config: TimerConfig)
              (runningTimer: RunningTimer, startTimerCommand: StartTimerCommand)
              =
    let rec doUpdates count messageId =
        async {
            let! timer = getTimer runningTimer.Id

            match timer with
            | RunningTimer _ ->
                match count with
                | n when n >= 0 ->
                    do! Async.Sleep config.Delay
                    do! updateMessage startTimerCommand.Chat messageId (sprintf "Перезапуск через %i" count)
                    do! doUpdates (count - 1) messageId
                | _ -> ()
            | _ -> ()
        }

    async {
        let! messageId =
            getParticipants startTimerCommand.Chat
            |> mapAsync (buildMentionList startTimerCommand.Starter)
            |> bindAsync (fun mentions ->
                async {
                    let! _ = sendMessage startTimerCommand.Chat mentions "Буду перезапускать, никто не против?"
                    return! sendMessage startTimerCommand.Chat mentions "Начинаю обратный отсчет"
                })

        do! (doUpdates config.CountsNumber messageId)
        let! _ = sendMessage startTimerCommand.Chat (Seq.singleton startTimerCommand.Starter) "Поехали!"
        let! stopResult = stopTimer runningTimer
        match stopResult with
        | Ok _ -> ()
        | Error _ -> failwith "Can't stop timer"
    }


let startTimer (saveTimer: InactiveTimer -> Async<ActionResult>)
               (inactiveTimer: InactiveTimer)
               : Async<Result<RunningTimer, TimerStartFailure>> =
    async {
        let! saveTimerResult = saveTimer inactiveTimer

        match saveTimerResult with
        | Success -> return Ok { Id = inactiveTimer.Id }
        | Fail -> return Error { Timer = inactiveTimer }
    }

let startTimerCountDown (startTimer: StartTimer)
                        (startLongRunningProcess: StartLongRunningProcess<RunningTimer * StartTimerCommand>)
                        (startTimerCommand: StartTimerCommand)
                        (inactiveTimer: InactiveTimer)
                        : Async<Result<RunningTimer, TimerStartFailure>> =
    async {
        return!
            startTimer inactiveTimer
            |> bindAsyncResult (fun runningTimer ->
                match startLongRunningProcess (runningTimer, startTimerCommand) with
                | Started -> Ok runningTimer
                | Throttled -> Error({ Timer = runningTimer }))
    }

let startTimerProcess: StartTimerProcess =
    let toInactive timer =
        match timer with
        | InactiveTimer inactive -> Ok inactive
        | RunningTimer runningTimer -> Error { Timer = runningTimer }
    
    fun getTimer startTimerCountDown startTimerCommand ->
        async {
            return!
                getTimer (startTimerCommand.Chat |> toTimerId)
                |> mapAsync toInactive
                |> bindAsyncResultAsync (startTimerCountDown startTimerCommand)
                |> mapAsyncResult (fun runningTimer -> { Timer = runningTimer })
        }
