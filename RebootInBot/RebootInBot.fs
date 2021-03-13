namespace RebootInBot.Bot

open System
open System.Threading
open RebootInBot.Storage.InMemoryStorage
open RebootInBot.Types
open RebootInBot.Processing
open RebootInBot.Commands
open RebootInBot.StartTimer
open RebootInBot.CancelTimer
    
    
type Bot private(messenger: IBotMessenger, storage: IStorage, processor:LongRunningProcessor<StartTimer>, cts: CancellationTokenSource) =
    
    let processCommand command =
        
        let processStartTimer (startTimer:StartTimer) =
            let chatProcess =
                storage.GetProcess startTimer.Chat.ChatId
            match chatProcess with
            | Some _ -> processRunning messenger.SendMessage startTimer |> ignore
            | None ->
                processor.Process(startTimer)
                |> fun result ->
                    match result with
                    |Started -> ()
                    |Throttled -> processThrottled messenger.SendMessage startTimer |> ignore
    
        
        match command with
        | StartTimer startTimer -> processStartTimer startTimer
        | CancelTimer cancelTimer -> processCancelTimer storage.GetProcess storage.DeleteProcess messenger.SendMessage cancelTimer
    
    member this.ProcessMessage(message:IncomingMessage) =
        message
        |> parseCommand
        |> Option.iter processCommand
        
    
    static member Start(messenger: IBotMessenger) =
        let cancellationTokenSource = new CancellationTokenSource()
        let inMemoryStorage = InMemoryStorage() :> IStorage
        let defaultTimerConfig = {
            Delay = 1000
            CountsNumber = 10
        }
        let timerProcessor = processStartTimer
                                 messenger.GetParticipants
                                 messenger.SendMessage
                                 messenger.UpdateMessage
                                 inMemoryStorage.SaveProcess
                                 inMemoryStorage.GetProcess
                                 inMemoryStorage.DeleteProcess
                                 defaultTimerConfig
        let longRunningProcessor = LongRunningProcessor<StartTimer>.Start(timerProcessor, 10, cancellationTokenSource.Token)
        new Bot(messenger, inMemoryStorage, longRunningProcessor, cancellationTokenSource)
    
    
    interface IDisposable with
        member x.Dispose() =
            (processor :> IDisposable).Dispose()
            cts.Cancel()



