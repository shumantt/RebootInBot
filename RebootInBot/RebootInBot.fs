namespace RebootInBot.Bot

open System
open System.Threading
open RebootInBot
open RebootInBot.Storage.InMemoryStorage
open RebootInBot.Types
open RebootInBot.Processing
open RebootInBot.Extensions

type Bot(messenger: IBotMessenger, storage: ITimerStorage, timerConfig: TimerConfig) =
    let cts = new CancellationTokenSource()
    let stopTimer: StopTimer = CancelTimer.stopTimer storage.Delete
    let startTimer: StartTimer = StartTimer.startTimer storage.Save
    let countDown =
        StartTimer.countDown
            messenger.SendMessageAsync
            messenger.UpdateMessageAsync
            messenger.GetParticipantsAsync
            storage.Get
            stopTimer
            timerConfig
    let longRunningProcessor = LongRunningProcessor<RunningTimer>.Start(countDown, 10, cts.Token)
    let cancelTimerProcess = CancelTimer.cancelTimerProcess storage.Get stopTimer
    let startTimerCountDown: StartTimerCountDown = StartTimer.startTimerCountDown longRunningProcessor.StartLongRunningTask
    let startTimerProcess = StartTimer.startTimerProcess storage.Get startTimer startTimerCountDown

    let processCommand command =
        async {
            match command with
            | StartTimerCommand startTimerCommand ->
                let! startResult = startTimerProcess startTimerCommand
                match startResult with
                | Ok _ -> ()
                | Error error -> do! StartTimer.processStartError messenger.SendMessageAsync error
            | CancelTimerCommand cancelTimerCommand ->
                let! cancelResult = cancelTimerProcess cancelTimerCommand
                match cancelResult  with
                | Ok timerCancelled -> do! CancelTimer.processTimerCancelled messenger.SendMessageAsync timerCancelled
                | Error _ -> ()
        }

    let processMessage (message: IncomingMessage) =
        async {
            match Commands.parseCommand message with
            | Some command -> do! processCommand command
            | None -> ()
        }

    member this.ProcessMessage(message: IncomingMessage) = processMessage message |> Async.StartAsTask

    static member Start(messenger: IBotMessenger) =
        let inMemoryStorage = InMemoryTimerStorage() :> ITimerStorage
        let defaultTimerConfig = { Delay = 1000; CountsNumber = 10 }
        new Bot(messenger, inMemoryStorage, defaultTimerConfig)

    interface IDisposable with
        member x.Dispose() =
            (longRunningProcessor :> IDisposable).Dispose()
            cts.Cancel()
