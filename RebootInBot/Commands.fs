module RebootInBot.Commands
open System
open RebootInBot.Types
open RebootInBot.Mentions

let private buildCommand getParticipants (message:IncomingMessage) (command: BotCommand) =
    let buildStartTimer (message:IncomingMessage) =
        { Chat = message.Chat
          Starter = message.Author
          ChatParticipants = getParticipants message.Chat }
    
    match command with
    | "/reboot" -> Some (Command.StartTimer (buildStartTimer message))
    | _ -> None

let private startTimerProcess chat messageId =
    Console.WriteLine("Started Timer")

let private processCommand sendMessage command =
    
    let processStartTimer startTimerCommand =
        buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
            |> sendMessage startTimerCommand.Chat
            |> startTimerProcess startTimerCommand.Chat
        
    match command with
    | StartTimer startTimer -> processStartTimer startTimer

let processMessage getParticipants sendMessage (incomingMessage: IncomingMessage) =
    let getCommand incomingMessage =
        incomingMessage.Commands
            |> Seq.tryHead
            |> Option.bind (buildCommand getParticipants incomingMessage)
    
    let command = getCommand incomingMessage
    match command with
    | Some command -> processCommand sendMessage command
    | _ -> Console.WriteLine("No Action")