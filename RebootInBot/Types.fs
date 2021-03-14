namespace RebootInBot.Types

open System
open System.Collections.Generic

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
    abstract member SendMessage: Chat -> IEnumerable<ChatParticipant> -> string -> MessageId
    abstract member GetParticipants: Chat -> IEnumerable<ChatParticipant>
    abstract member UpdateMessage: Chat -> MessageId -> string -> unit

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

type TimerInfo = { Id: TimerId }

type RunningTimer = TimerInfo

type InactiveTimer = TimerInfo

type Timer =
    | RunningTimer of RunningTimer
    | InactiveTimer of InactiveTimer

type GetTimer = TimerId -> Async<Timer>

type TimerCancelled = { Timer: InactiveTimer }

type GeneralError = { Message: string }


type TimerCancellationFailure = { Timer: TimerInfo }

type ActionResult =
    | Success
    | Fail

type StopTimerError = string

type StopTimer = RunningTimer -> Async<Result<InactiveTimer, TimerCancellationFailure>> 

type CancelTimerProcess = GetTimer -> StopTimer -> CancelTimerCommand -> Async<Result<TimerCancelled, TimerCancellationFailure>>

type TimerStarted = {
    Timer: RunningTimer
}

type TimerStartFailure = {
    Timer: TimerInfo
}

type StartTimer = InactiveTimer -> Async<Result<RunningTimer, TimerStartFailure>>

type StartTimerCountDown = StartTimerCommand -> InactiveTimer -> Async<Result<RunningTimer, TimerStartFailure>>

type CountDown = GetTimer -> RunningTimer -> Async<unit>

type StartTimerProcess = GetTimer -> StartTimerCountDown -> StartTimerCommand -> Async<Result<TimerStarted, TimerStartFailure>>

// workflows

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
    abstract member Save: InactiveTimer -> Async<ActionResult>
    abstract member Get: TimerId -> Async<Timer>
    abstract member Delete: RunningTimer -> Async<ActionResult>