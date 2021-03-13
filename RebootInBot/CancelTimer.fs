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

type CancelledTimer = { Timer: RunningTimer }

type CancelError = { Message: string }


type FailedTimerCancellation = { Timer: Timer }

type CancelTimer = RunningTimer -> Async<Result<RunningTimer, CancelError>>

type CancelProcess = Timer -> CancelTimer -> Async<Result<CancelledTimer, FailedTimerCancellation>>

let cancelProcess: CancelProcess =
    fun timer cancelTimer ->
        async {
            match timer with
            | RunningTimer runningTimer ->
                let! cancelResult = cancelTimer runningTimer

                match cancelResult with
                | Ok runningTimer -> return Result.Ok { Timer = runningTimer }
                | Error _ -> return Result.Error { Timer = timer }
            | InactiveTimer _ -> return Result.Error { Timer = timer }
        }
