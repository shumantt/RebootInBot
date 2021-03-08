module RebootInBot.Bot

open System
open System.Threading
open RebootInBot.Storage
open RebootInBot.Storage.InMemoryStorage
open RebootInBot.Types
open RebootInBot.Processing
open RebootInBot.Commands
open RebootInBot.StartTimer
    
    
type Bot private(messenger: IBotMessenger, processor:LongRunningProcessor<StartTimer>, cts: CancellationTokenSource) =
    
    let processCommand command =
        
        let processStartTimer startTimer =
            processor.Process(startTimer)
            |> fun result ->
                match result with
                |Started -> ()
                |Throttled -> processThrottled messenger.SendMessage startTimer |> ignore
    
        
        match command with
        | StartTimer startTimer -> processStartTimer startTimer
    
    member this.ProcessMessage(message:IncomingMessage) =
        message
        |> parseCommand
        |> Option.iter processCommand
        
    
    static member Start(messenger: IBotMessenger) =
        let cancellationTokenSource = new CancellationTokenSource()
        let inMemoryStorage = InMemoryStorage()
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
                                 defaultTimerConfig
        let longRunningProcessor = LongRunningProcessor<StartTimer>.Start(timerProcessor, 10, cancellationTokenSource.Token)
        new Bot(messenger, longRunningProcessor, cancellationTokenSource)
    
    
    interface IDisposable with
        member x.Dispose() =
            (processor :> IDisposable).Dispose()
            cts.Cancel()



