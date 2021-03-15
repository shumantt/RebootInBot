module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Helpers
open RebootInBot.Mentions

let countDown (sendMessage: SendMessage)
              (updateMessage: UpdateMessage)
              (getParticipants: GetParticipants)
              (getTimer: GetTimer)
              (stopTimer: StopTimer)
              (config: TimerConfig)
              (runningTimer: RunningTimer)
              =
    let rec doUpdates count messageId =
        async {
            let! timer = getTimer runningTimer.Id

            match timer with
            | RunningTimer _ ->
                match count with
                | n when n >= 0 ->
                    do! Async.Sleep config.Delay
                    do! updateMessage runningTimer.Chat messageId (sprintf "Перезапуск через %i" count)
                    do! doUpdates (count - 1) messageId
                | _ -> ()
            | _ -> ()
        }

    async {
        let! messageId =
            getParticipants runningTimer.Chat
            |> mapAsync (buildMentionList runningTimer.Starter)
            |> bindAsync (fun mentions ->
                async {
                    let! _ = sendMessage runningTimer.Chat mentions "Буду перезапускать, никто не против?"
                    return! sendMessage runningTimer.Chat mentions "Начинаю обратный отсчет"
                })

        do! (doUpdates config.CountsNumber messageId)
        let! _ = sendMessage runningTimer.Chat (Seq.singleton runningTimer.Starter) "Поехали!"
        let! stopResult = stopTimer runningTimer

        match stopResult with
        | Ok _ -> ()
        | Error _ -> failwith "Can't stop timer"
    }


let startTimer (saveTimer: RunningTimer -> Async<ActionResult>)
               (startTimerCommand: StartTimerCommand)
               (inactiveTimer: InactiveTimer)
               : Async<Result<RunningTimer, StartError>> =
    async {
        let runningTimer =
            { Id = inactiveTimer.Id
              Chat = startTimerCommand.Chat
              Starter = startTimerCommand.Starter }

        let! saveTimerResult = saveTimer runningTimer

        match saveTimerResult with
        | Success -> return Ok runningTimer
        | Fail -> return Error StartError.SaveError
    }

let startTimerCountDown startLongRunningTask runningTimer =
    match startLongRunningTask runningTimer with
    | Started -> Ok runningTimer
    | Throttled -> Error StartError.Throttled

let startTimerProcess: StartTimerProcess =
    let toInactive timer =
        match timer with
        | InactiveTimer inactive -> Ok inactive
        | RunningTimer _ -> Error StartError.AlreadyRunning

    fun getTimer startTimer startTimerCountDown startTimerCommand ->
        async {
            return!
                getTimer (startTimerCommand.Chat |> toTimerId)
                |> mapAsync toInactive
                |> bindAsyncResultAsync (startTimer startTimerCommand)
                |> bindAsyncResult startTimerCountDown
                |> mapAsyncResult (fun runningTimer -> { Timer = runningTimer })
                |> mapAsyncError (fun error ->
                    { Chat = startTimerCommand.Chat
                      Starter = startTimerCommand.Starter
                      Error = error })
        }

let processStartError (sendMessage: SendMessage) (error: TimerStartFailure) =
    async {
        match error.Error with
        | StartError.Throttled ->
            let! _ =
                sendMessage error.Chat (Seq.singleton error.Starter) "Пока не можем запустить отсчет, попробуйте позже"

            ()
        | _ -> ()

    }
