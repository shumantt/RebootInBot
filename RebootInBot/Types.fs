namespace RebootInBot.Types

open System
open System.Collections.Generic
open System.Threading.Tasks

// BotProcessing
type ChatId = Guid

type Chat = {
    ChatId: ChatId
}

type MessageAuthor = unit
type MessageText = string
type MessageId = Guid
type Mention = unit
type BotCommand = string
type ChatParticipant = string

type Message = {
    Author: ChatParticipant
    Text: MessageText
    Chat: Chat
    MessageId: MessageId
    Commands: IEnumerable<BotCommand>
}

type IncomingMessage = Message


type GetParticipants = Chat -> Async<seq<ChatParticipant>>
type SendMessage = Chat -> seq<ChatParticipant> -> string -> Async<MessageId>
type UpdateMessage = Chat -> MessageId -> string -> Async<unit>


type IBotMessenger =
    abstract member SendMessage: Chat -> IEnumerable<ChatParticipant> -> string -> Task<MessageId>
    abstract member GetParticipants: Chat -> Task<IEnumerable<ChatParticipant>>
    abstract member UpdateMessage: Chat -> MessageId -> string -> Task

// Process
type Process = {
    ChatId: ChatId
    Starter: ChatParticipant
}



// Commands

type ChatCommand = {
    Chat: Chat
}

type CancelTimerCommand = ChatCommand
type StartTimerCommand = {
    Chat: Chat
    Starter: ChatParticipant
}

type Command =
    | StartTimerCommand of StartTimerCommand
    | CancelTimerCommand of CancelTimerCommand

type ParseCommand = IncomingMessage -> Option<Command>   
        
type MessageProcess = IncomingMessage -> Async<unit>

type TimerId = Guid

type TimerInfo = {
    Id: TimerId
}

type RunningTimer = {
    Id: TimerId
    Chat: Chat
    Starter: ChatParticipant
}

type InactiveTimer = TimerInfo

type Timer =
    | RunningTimer of RunningTimer
    | InactiveTimer of InactiveTimer

type GetTimer = TimerId -> Async<Timer>

type TimerCancelled = {
    Timer: InactiveTimer
    Chat: Chat
    Starter: ChatParticipant
}

type GeneralError = { Message: string }


type TimerCancellationFailure = { Timer: TimerInfo }

type ActionResult =
    | Success
    | Fail

type StoppedTimer = {
    Timer: InactiveTimer
    Chat: Chat
    Starter: ChatParticipant
}

type StopTimer = RunningTimer -> Async<Result<StoppedTimer, TimerCancellationFailure>> 

type CancelTimerProcessResult = Result<TimerCancelled, TimerCancellationFailure>

type CancelTimerProcess = GetTimer -> StopTimer -> CancelTimerCommand -> Async<CancelTimerProcessResult>

type TimerStarted = {
    Timer: RunningTimer
}

type StartError =
    | Throttled
    | SaveError
    | AlreadyRunning

type TimerStartFailure = {
    Chat: Chat
    Starter: ChatParticipant
    Error: StartError
}

type StartTimer = StartTimerCommand -> InactiveTimer -> Async<Result<RunningTimer, StartError>>

type StartTimerCountDown = RunningTimer -> Result<RunningTimer, StartError>

type CountDown = GetTimer -> RunningTimer -> Async<unit>

type StartTimerProcessResult = Result<TimerStarted, TimerStartFailure>

type StartTimerProcess = GetTimer -> StartTimer -> StartTimerCountDown -> StartTimerCommand -> Async<StartTimerProcessResult>

// workflows

type CommandProcessResult =
    | StartTimerProcessResult of StartTimerProcessResult
    | CancelTimerProcessResult of CancelTimerProcessResult

type MessageProcessError =
    | TimerStartFailure of TimerStartFailure
    | TimerCancellationFailure of TimerCancellationFailure
        
type MessageProcessed =
    | TimerStarted of TimerStarted
    | TimerCancelled of TimerCancelled
   


//long running process

type LongRunningResult =
    | Started
    | Throttled

type StartLongRunningProcess<'a> = 'a -> LongRunningResult

type LongRunningWork<'a> = {
    WorkData: 'a
    ReplyChannel: AsyncReplyChannel<LongRunningResult>
}

type WorkQueueItem<'a> =
    | Work of LongRunningWork<'a>
    | WorkDone


// config

type TimerConfig = {
    Delay: int
    CountsNumber: int
}

type ITimerStorage =
    abstract member Save: RunningTimer -> Async<ActionResult>
    abstract member Get: TimerId -> Async<Timer>
    abstract member Delete: RunningTimer -> Async<ActionResult>