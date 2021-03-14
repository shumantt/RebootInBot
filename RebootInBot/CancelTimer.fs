module RebootInBot.CancelTimer

open RebootInBot.Types
open RebootInBot.Helpers

let processCancelTimer getProcess deleteProcess sendMessage (cancelTimer: CancelTimerCommand) =
    let chatProcess = getProcess cancelTimer.Chat.ChatId

    match chatProcess with
    | None -> ()
    | Some chatProcess ->
        deleteProcess chatProcess.ChatId

        sendMessage cancelTimer.Chat (List.singleton chatProcess.Starter) "С перезапуском нужно подождать"
        |> ignore

let stopTimer (deleteTimer:RunningTimer -> Async<ActionResult>) (runningTimer:RunningTimer) : Async<Result<InactiveTimer, TimerCancellationFailure>> =
    async {
        let! deleteResult = deleteTimer runningTimer
        match deleteResult with
        | Success -> return Ok runningTimer
        | Fail -> return Error({ Timer = runningTimer }: TimerCancellationFailure)
    }

let cancelProcess: CancelTimerProcess =
    let toRunning timer =
        match timer with
        | RunningTimer running -> Ok running
        | InactiveTimer inactiveTimer -> Error({ Timer = inactiveTimer }: TimerCancellationFailure)

    fun getTimer stopTimer cancelTimerCommand ->
        async {
            return!
                getTimer (cancelTimerCommand.Chat |> toTimerId)
                |> mapAsync toRunning
                |> bindAsyncResultAsync stopTimer 
                |> mapAsyncResult (fun inactiveTimer -> { Timer = inactiveTimer })
        }
