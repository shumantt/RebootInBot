module RebootInBot.Commands

open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand incomingMessage (command: BotCommand) =
    match command with
    | "/reboot" -> Some (Command.StartTimer (buildStartTimerCommand incomingMessage))
    | "/cancel" -> Some (Command.CancelTimer ({ Chat = incomingMessage.Chat }))
    | _ -> None

let parseCommand (incomingMessage: IncomingMessage) =
    incomingMessage.Commands
    |> Seq.tryHead
    |> Option.bind (buildCommand incomingMessage)
    
