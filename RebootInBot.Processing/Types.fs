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