module RebootInBot.CancelTimer

open System
open RebootInBot.Types

let processCancelTimer getProcess deleteProcess sendMessage (cancelTimer: CancelTimer) =
    let chatProcess = getProcess cancelTimer.Chat.ChatId

    match chatProcess with
    | None -> ()
    | Some chatProcess ->
        deleteProcess chatProcess.ChatId

        sendMessage cancelTimer.Chat (List.singleton chatProcess.Starter) "С перезапуском нужно подождать"
        |> ignore




type CancelTimerCommand = unit

type TimerInfo = { Id: Guid }

type RunningTimer = TimerInfo
type InactiveTimer = TimerInfo

type Timer =
    | RunningTimer of RunningTimer
    | InactiveTimer of InactiveTimer

type TimerCancelled = { Timer: InactiveTimer }

type GeneralError = { Message: string }


type TimerCancellationFailure = { Timer: Timer }

type CancelTimer = RunningTimer -> Async<Result<InactiveTimer, GeneralError>>

type CancelProcess = Timer -> CancelTimer -> Async<Result<TimerCancelled, TimerCancellationFailure>>

let cancelProcess: CancelProcess =
    fun timer cancelTimer ->
        async {
            match timer with
            | RunningTimer runningTimer ->
                let! cancelResult = cancelTimer runningTimer

                match cancelResult with
                | Ok inactiveTimer -> return Result.Ok { Timer = inactiveTimer }
                | Error _ -> return Result.Error { Timer = timer }
            | InactiveTimer _ -> return Result.Error { Timer = timer }
        }


type TimerStarted = {
    Timer: RunningTimer
}

type TimerStartFailure = {
    Timer: Timer
}

type StartTimerCountDownFailure =
    | NewThrottled
    | StartError of GeneralError

type StartTimerCountDown = InactiveTimer -> Async<Result<RunningTimer, StartTimerCountDownFailure>>

type StartTimerProcess = Timer -> StartTimerCountDown -> Async<Result<TimerStarted, TimerStartFailure>>


let startTimerProcess: StartTimerProcess =
    fun timer startTimerCountDown ->
        async {
            match timer with
            | InactiveTimer inactiveTimer ->
                let! startResult = startTimerCountDown inactiveTimer
                match startResult with
                | Ok runningTimer -> return Result.Ok { Timer = runningTimer }
                | Error startCountDownFailure -> return Result.Error { Timer = timer }
            | RunningTimer runningTimer -> return Result.Error { Timer = timer }
        }