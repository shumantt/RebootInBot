module RebootInBot.Commands
open System
open RebootInBot.Types
open RebootInBot.Mentions
open RebootInBot.StartTimer

let private buildCommand getParticipants (message:IncomingMessage) (command: BotCommand) =
    match command with
    | "/reboot" -> Some (Command.StartTimer (buildStartTimer getParticipants message))
    | _ -> None

let private processCommand sendMessage command =        
    match command with
    | StartTimer startTimer -> processStartTimer sendMessage startTimer

let processMessage getParticipants sendMessage (incomingMessage: IncomingMessage) =
    let getCommand incomingMessage =
        incomingMessage.Commands
            |> Seq.tryHead
            |> Option.bind (buildCommand getParticipants incomingMessage)
    
    let command = getCommand incomingMessage
    match command with
    | Some command -> processCommand sendMessage command
    | _ -> Console.WriteLine("No Action")