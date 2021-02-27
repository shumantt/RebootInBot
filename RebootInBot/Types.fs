module RebootInBot.Types

// BotProcessing
type Chat = unit
type MessageAuthor = unit
type MessageText = string
type MessageId = unit
type Mention = unit
type BotCommand = string
type ChatParticipant = unit

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
    ChatParticipants: ChatParticipant list
}

type Command =
    | StartTimer of StartTimer

        

