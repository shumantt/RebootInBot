module RebootInBot.Processing.Types

type IncomingCommandType =
| StartTimer
| CancelTimer
| ExcludeMember
| IncludeMember

type SendCommandType =
| Started
| Count
| Cancelled
| Excluded

type CommandSource =
| Telegram

type CommandData = {
    MachineName: string option
    FromUser: string
}

type IncomingCommand = {
    Type: IncomingCommandType
    Source: CommandSource
    ProcessId: int64
    Data: CommandData
}

type Mentions =
| Specific of array<string>
| AllWithExcluded of array<string>
| All
| NoOne

type BaseOutgoingCommand = {
    Mentions: Mentions
    MessageText: string
    ProcessId: int64
}

type EditMessage = {
    Base: BaseOutgoingCommand
    MessageId: int64
}

type NewMessage = BaseOutgoingCommand

type OutgoingCommand =
| NewMessage of NewMessage
| EditMessage of EditMessage
| NoAction

[<CLIMutable>]
type CountingState = {
    Count: int
    UserStarted: string
}

type ProcessState =
| CountingState of CountingState
| IdleState

[<CLIMutable>]
type ProcessConfig = {
    ExcludeMembers: array<string>
}

[<CLIMutable>]
type Process = {
    Id: int64
    State: ProcessState
    Config: ProcessConfig
}