module RebootInBot.Commands

open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand incomingMessage (command: BotCommand) =
    match command with
    | "/reboot" -> Some(Command.StartTimerCommand(buildStartTimerCommand incomingMessage))
    | "/cancel" -> Some(Command.CancelTimerCommand({ Chat = incomingMessage.Chat }))
    | _ -> None

let parseCommand: ParseCommand =
    fun incomingMessage ->
        incomingMessage.Commands
        |> Seq.tryHead
        |> Option.bind (buildCommand incomingMessage)
