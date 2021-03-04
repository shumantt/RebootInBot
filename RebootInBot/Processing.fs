module RebootInBot.Processing

open RebootInBot.Types
open RebootInBot.Commands

let processMessage checkIsCancelled getParticipants sendMessage updateMessage (longRunningWorker: MailboxProcessor<WorkQueueItem>) incomingMessage =
   let work = processMessage getParticipants sendMessage updateMessage checkIsCancelled incomingMessage
   match work with
   | None -> ()
   | Some someWork ->
       match someWork with
       | TimerWork timerWork -> longRunningWorker.Post (WorkQueueItem.Work (Work.TimerWork timerWork))

type StartedProcessor(checkIsCancelled, getParticipants, sendMessage, updateMessage, longRunningWorker) =
    member this.Process(message:IncomingMessage) =
        processMessage checkIsCancelled getParticipants sendMessage updateMessage longRunningWorker message

type Processor(getParticipants, sendMessage, updateMessage, onThrottled, longRunningLimit) =
    let startLongRunningWorker =
        let agent cancellationToken = MailboxProcessor.Start((fun inbox -> async {
            let mutable longRunningJobs = 0
            
            let handleTimerWork timerWork =
                if longRunningJobs < longRunningLimit then
                    longRunningJobs <- longRunningJobs + 1
                    Async.Start(async {
                        do! timerWork
                        inbox.Post(WorkQueueItem.WorkResult Done)
                    })
                else
                    inbox.Post(WorkQueueItem.WorkResult Throttled)
            
            while true do
                let! workItem = inbox.Receive()
                match workItem with
                | Work work ->
                    match work with
                    | TimerWork timerWork -> handleTimerWork timerWork
                | WorkResult result ->
                    match result with
                    | Done -> longRunningJobs <- longRunningJobs - 1
                    | Throttled -> onThrottled

            }), cancellationToken)
        agent
    
    member this.StartProcessor(cancellationToken) =
        //todo configure storage
        let checkIsCancelled chat () = false
        let longRunningWorker = startLongRunningWorker cancellationToken
        StartedProcessor(checkIsCancelled, getParticipants, sendMessage, updateMessage, longRunningWorker)
    
        