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

type CancelTimer = RunningTimer -> Async<Result<InactiveTimer, TimerCancellationFailure>>

type CancelProcess = Timer -> CancelTimer -> Async<Result<TimerCancelled, TimerCancellationFailure>>

type TimerStarted = {
    Timer: RunningTimer
}

type TimerStartFailure = {
    Timer: Timer
}

type StartTimerCountDown = InactiveTimer -> Async<Result<RunningTimer, TimerStartFailure>>

type StartTimerProcess = Timer -> StartTimerCountDown -> Async<Result<TimerStarted, TimerStartFailure>>
    
let bindResultAsync binder value =
    async {
        match value with
        | Ok result -> return! binder result
        | Error error -> return Error error
    }
    
let mapResultAsync map value =
    async {
        let! result = value
        match result with
        | Ok result -> return (map result)
        | Error error -> return Error error
    }

let cancelProcess: CancelProcess =
    let toRunning timer =
        match timer with
        | RunningTimer running -> Ok running
        | InactiveTimer _ -> Error ({ Timer = timer } : TimerCancellationFailure)
    
    fun timer cancelTimer ->
        async {
            return! timer
            |> toRunning
            |> bindResultAsync cancelTimer
            |> mapResultAsync (fun inactiveTimer -> Ok { Timer = inactiveTimer })
        }


let startTimerProcess: StartTimerProcess =
    let toInactive timer = 
        match timer with
        | InactiveTimer inactive -> Ok inactive
        | RunningTimer _ -> Error { Timer = timer }
    
    fun timer startTimerCountDown ->
        async {
           return! timer
            |> toInactive
            |> bindResultAsync startTimerCountDown
            |> mapResultAsync (fun runningTimer -> Ok { Timer = runningTimer })
        }
type MessageProcessError =
    | TimerStartFailure of TimerStartFailure
    | TimerCancellationFailure of TimerCancellationFailure
        
type MessageProcessed =
    | TimerStarted of TimerStarted
    | TimerCancelled of TimerCancelled
        
type MessageProcess = IncomingMessage -> Async<Result<MessageProcessed, MessageProcessError>>