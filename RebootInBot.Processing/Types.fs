module RebootInBot.Processing.Types

type IncomingCommandType =
| StartTimer
| CancelTimer
| ExcludeMember

type SendCommandType =
| Started
| Count
| Cancelled
| Excluded

type CommandSource =
| Telegram


type ProcessStateType =
| Idle
| Counting

type CommandData = {
    MachineName: string option
    FromUser: string option
}

type IncomingCommand = {
    Type: IncomingCommandType
    Source: CommandSource
    ProcessId: int64
    Data: CommandData
}

[<CLIMutable>]
type ProcessState = {
    CurrentState: ProcessStateType
    Count: int option
    UserStarted: string option
}

[<CLIMutable>]
type ProcessConfig = {
    ExcludeMembers: array<string> option
}

[<CLIMutable>]
type Process = {
    Id: int64
    State: ProcessState
    Config: ProcessConfig
}