namespace RebootInBot.Bot

open System
open System.Threading
open RebootInBot
open RebootInBot.Storage.InMemoryStorage
open RebootInBot.Types
open RebootInBot.Processing


type Bot private (messenger: IBotMessenger,
                  storage: ITimerStorage,
                  processor: LongRunningProcessor<RunningTimer>,
                  cts: CancellationTokenSource) =

    let sendMessage: SendMessage =
        fun chat participants text ->
            messenger.SendMessage chat participants text
            |> Async.AwaitTask
    
    let stopTimer : StopTimer =
        CancelTimer.stopTimer storage.Delete
    
    let cancelTimerProcess =
        CancelTimer.cancelTimerProcess storage.Get stopTimer

    let startTimer: StartTimer =
        StartTimer.startTimer storage.Save
    
    let startTimerCountDown: StartTimerCountDown =
        StartTimer.startTimerCountDown processor.StartLongRunningTask
    
    let startTimerProcess =
        StartTimer.startTimerProcess storage.Get startTimer startTimerCountDown
    
    let processCommand command =
        let getResult command =
            async {
                match command with
                | StartTimerCommand startTimerCommand ->
                    let! startResult = startTimerProcess startTimerCommand
                    return CommandProcessResult.StartTimerProcessResult startResult
                | CancelTimerCommand cancelTimerCommand ->
                    let! cancelResult = cancelTimerProcess cancelTimerCommand
                    return CommandProcessResult.CancelTimerProcessResult cancelResult
            }
        
        async {
            let! result = getResult command
            match result with
            | StartTimerProcessResult startTimerProcessResult ->
                match startTimerProcessResult with
                | Ok _ -> ()
                | Error error -> do! StartTimer.processStartError sendMessage error
            | CancelTimerProcessResult cancelTimerProcessResult ->
                match cancelTimerProcessResult with
                | Ok timerCancelled -> do! CancelTimer.processTimerCancelled sendMessage timerCancelled
                | Error _ -> ()
        }
       
    
    let processMessage (message: IncomingMessage) =
        async {
            match Commands.parseCommand message with
            | None -> ()
            | Some command -> do! processCommand command
        }

    member this.ProcessMessage(message: IncomingMessage) =
        processMessage message
        |> Async.StartAsTask


    static member Start(messenger: IBotMessenger) =
        let cancellationTokenSource = new CancellationTokenSource()
        let inMemoryStorage = InMemoryTimerStorage() :> ITimerStorage
        let defaultTimerConfig = { Delay = 1000; CountsNumber = 10 }

        let sendMessage: SendMessage =
            fun chat participants text ->
                messenger.SendMessage chat participants text
                |> Async.AwaitTask
                
        let updateMessage: UpdateMessage =
            fun chat messageId text ->
                messenger.UpdateMessage chat messageId text
                |> Async.AwaitTask
        
        let getParticipants: GetParticipants =
            fun chat ->
                messenger.GetParticipants chat
                |> Async.AwaitTask
        let stopTimer : StopTimer =
            CancelTimer.stopTimer inMemoryStorage.Delete
        
        let countDown =
            StartTimer.countDown
                sendMessage
                updateMessage
                getParticipants
                inMemoryStorage.Get
                stopTimer
                defaultTimerConfig

        let longRunningProcessor = LongRunningProcessor<RunningTimer>.Start(countDown, 10, cancellationTokenSource.Token)

        new Bot(messenger, inMemoryStorage, longRunningProcessor, cancellationTokenSource)


    interface IDisposable with
        member x.Dispose() =
            (processor :> IDisposable).Dispose()
            cts.Cancel()
