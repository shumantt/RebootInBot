module RebootInBot.Commands
open System
open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand getParticipants (message:IncomingMessage) (command: BotCommand) =
    match command with
    | "/reboot" -> Some (Command.StartTimer (buildStartTimer getParticipants message))
    | _ -> None

let private processCommand sendMessage updateMessage command =        
    match command with
    | StartTimer startTimer -> processStartTimer sendMessage updateMessage startTimer

let processMessage getParticipants sendMessage updateMessage (incomingMessage: IncomingMessage) =
    let getCommand incomingMessage =
        incomingMessage.Commands
            |> Seq.tryHead
            |> Option.bind (buildCommand getParticipants incomingMessage)
    
    let command = getCommand incomingMessage
    match command with
    | Some command -> processCommand sendMessage updateMessage command
    | _ -> async { Console.WriteLine("No action") }