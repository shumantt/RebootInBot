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

type Work =
    | TimerWork of StartTimer
    
type WorkResult =
    | Throttled
    | Done

type WorkQueueItem =
    | Work of Work
    | WorkResult of WorkResult

type TimerConfig = {
    Delay: int
    CountsNumber: int
}