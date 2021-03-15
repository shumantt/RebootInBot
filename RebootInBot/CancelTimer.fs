module RebootInBot.CancelTimer

open RebootInBot.Types
open RebootInBot.Helpers

let stopTimer (deleteTimer:RunningTimer -> Async<ActionResult>) (runningTimer:RunningTimer) : Async<Result<StoppedTimer, TimerCancellationFailure>> =
    async {
        let! deleteResult = deleteTimer runningTimer
        let inactiveTimer = { Id = runningTimer.Id }
        match deleteResult with
        | Success -> return Ok {
            Timer = inactiveTimer
            Chat = runningTimer.Chat
            Starter = runningTimer.Starter }
        | Fail -> return Error({ Timer = inactiveTimer }: TimerCancellationFailure)
    }

let cancelTimerProcess: CancelTimerProcess =
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
                |> mapAsyncResult (fun stoppedTimer -> { Timer = stoppedTimer.Timer
                                                         Chat = stoppedTimer.Chat
                                                         Starter = stoppedTimer.Starter })
        }
        
let processTimerCancelled (sendMessage:SendMessage) (timerCancelled: TimerCancelled) =
    async {
        let! _ = sendMessage timerCancelled.Chat (List.singleton timerCancelled.Starter) "С перезапуском нужно подождать" 
        ()
    }
