module RebootInBot.Commands

open RebootInBot.Types

let private buildCommand incomingMessage (command: BotCommand) =
    match command with
    |BotCommand "/reboot" -> Some(Command.StartTimerCommand(StartTimer.buildStartTimerCommand incomingMessage))
    | BotCommand "/cancel" -> Some(Command.CancelTimerCommand({ Chat = incomingMessage.Chat }))
    | _ -> None

let parseCommand: ParseCommand =
    fun incomingMessage ->
        incomingMessage.Commands
        |> Seq.tryHead
        |> Option.bind (buildCommand incomingMessage)
