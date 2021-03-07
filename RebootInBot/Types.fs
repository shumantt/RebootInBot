module RebootInBot.Types

open System

// BotProcessing
type Chat = {
    ChatId: Guid
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
    Commands: BotCommand list
}

type IncomingMessage = Message

// Commands processing

type InformationText = unit

// Start
type Process = unit

type StartTimer = {
    Chat: Chat
    Starter: ChatParticipant
}

type Command =
    | StartTimer of StartTimer     

type CancelWork = Async<unit>

type LongRunningResult =
    | Started
    | Throttled

type LongRunningWork<'a> = {
    WorkData: 'a
    ReplyChannel: AsyncReplyChannel<LongRunningResult>
}

type WorkQueueItem<'a> =
    | Work of LongRunningWork<'a>
    | WorkDone

type TimerConfig = {
    Delay: int
    CountsNumber: int
}