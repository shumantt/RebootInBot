module RebootInBot.Processing

open RebootInBot.Types
open RebootInBot.Commands
open RebootInBot.StartTimer

type StartedProcessor(longRunningWorker: MailboxProcessor<WorkQueueItem>) =
    let processCommand command =
       match command with
       | None -> ()
       | Some someCommand ->
           match someCommand with
           | StartTimer startTimer -> longRunningWorker.Post (WorkQueueItem.Work (Work.TimerWork startTimer))

    member this.Process(message:IncomingMessage) =
        parseCommand message
        |> processCommand 

type Processor(getParticipants, sendMessage, updateMessage, onThrottled, longRunningLimit, timerConfig) =
    let startLongRunningWorker checkIsCancelled =
        let agent cancellationToken = MailboxProcessor.Start((fun inbox -> async {
            let mutable longRunningJobs = 0
            
            let handleTimerWork timerWork =
                if longRunningJobs < longRunningLimit then
                    longRunningJobs <- longRunningJobs + 1
                    Async.Start(async {
                        do! processStartTimer getParticipants sendMessage updateMessage checkIsCancelled timerConfig timerWork
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
        let longRunningWorker = startLongRunningWorker checkIsCancelled cancellationToken
        StartedProcessor(longRunningWorker)
    
        