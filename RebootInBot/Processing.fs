module RebootInBot.Processing

open System
open System.Threading
open RebootInBot.Types

type LongRunningProcessor<'a> private (longRunningWorker: MailboxProcessor<WorkQueueItem<'a>>) =
    member this.StartLongRunningTask(workData) =
        longRunningWorker.PostAndReply(fun rc -> WorkQueueItem.Work {
            WorkData = workData
            ReplyChannel = rc
        })

    static member Start<'a>(processWork, longRunningLimit, onWorkFail, ?cancellationToken:CancellationToken) =
        let agent = MailboxProcessor.Start((fun inbox -> async {
            let mutable longRunningJobs = 0
            let handleLongRunningWork work =
                match longRunningJobs with
                | n when n < longRunningLimit ->
                    longRunningJobs <- longRunningJobs + 1
                    Async.Start(async {
                        try 
                            do! processWork work.WorkData
                            inbox.Post(WorkDone)
                        with
                        | _ -> onWorkFail work.WorkData
                    }, ?cancellationToken = cancellationToken)
                    work.ReplyChannel.Reply(Started)
                | _ -> work.ReplyChannel.Reply(Throttled)
            
            while true do
                let! workItem = inbox.Receive()
                match workItem with
                | Work work -> handleLongRunningWork work
                | WorkDone -> longRunningJobs <- longRunningJobs - 1

            }), ?cancellationToken = cancellationToken)
        
        new LongRunningProcessor<'a>(agent)
        
    static member Start<'a>(processWork, longRunningLimit, ?cancellationToken:CancellationToken) =
        let onWorkFail (_:'a) = ()
        LongRunningProcessor<'a>.Start(processWork, longRunningLimit, onWorkFail, ?cancellationToken = cancellationToken)
        
        
    interface IDisposable with
        member x.Dispose() = 
            (longRunningWorker :> IDisposable).Dispose()
    
        