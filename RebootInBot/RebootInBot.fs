namespace RebootInBot.Bot

open System
open System.Threading
open RebootInBot.Storage.InMemoryStorage
open RebootInBot.Types
open RebootInBot.Processing
open RebootInBot.Commands
open RebootInBot.StartTimer
open RebootInBot.CancelTimer


type Bot private (messenger: IBotMessenger,
                  storage: ITimerStorage,
                  processor: LongRunningProcessor<StartTimer>,
                  cts: CancellationTokenSource) =

    let stopTimer : StopTimer =
        stopTimer storage.Delete
    
    let cancelProcess =
        cancelProcess storage.Get stopTimer   
    
    let processMessage (message: IncomingMessage) =
        message
        |> parseCommand
        |> Option.iter (fun command ->
            match command with
            | CancelTimerCommand cancelTimerCommand -> ()
            | StartTimerCommand startTimerCommand -> ()
           )


    let processCommand command =

        let processStartTimer (startTimer: StartTimer) =
            let chatProcess =
                storage.Get startTimer.Chat.ChatId

            match chatProcess with
            | Some _ ->
                processRunning messenger.SendMessage startTimer
                |> ignore
            | None ->
                processor.Process(startTimer)
                |> fun result ->
                    match result with
                    | Started -> ()
                    | Throttled ->
                        processThrottled messenger.SendMessage startTimer
                        |> ignore


        match command with
        | StartTimer startTimer -> processStartTimer startTimer
        | CancelTimer cancelTimer ->
            processCancelTimer storage.Get storage.Delete messenger.SendMessage cancelTimer

    member this.ProcessMessage(message: IncomingMessage) =
        message
        |> parseCommand
        |> Option.iter processCommand


    static member Start(messenger: IBotMessenger) =
        let cancellationTokenSource = new CancellationTokenSource()
        let inMemoryStorage = InMemoryTimerStorage() :> ITimerStorage
        let defaultTimerConfig = { Delay = 1000; CountsNumber = 10 }

        let timerProcessor =
            processStartTimer
                messenger.GetParticipants
                messenger.SendMessage
                messenger.UpdateMessage
                inMemoryStorage.Save
                inMemoryStorage.Get
                inMemoryStorage.Delete
                defaultTimerConfig

        let longRunningProcessor =
            LongRunningProcessor<StartTimer>
                .Start(timerProcessor, 10, cancellationTokenSource.Token)

        new Bot(messenger, inMemoryStorage, longRunningProcessor, cancellationTokenSource)


    interface IDisposable with
        member x.Dispose() =
            (processor :> IDisposable).Dispose()
            cts.Cancel()
