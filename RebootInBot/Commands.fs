module RebootInBot.Commands
open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand getParticipants (message:IncomingMessage) (command: BotCommand) =
    match command with
    | "/reboot" -> Some (Command.StartTimer (buildStartTimer getParticipants message))
    | _ -> None

let private processCommand sendMessage updateMessage command =
    let checkIsCancelled chat ()  = false
    let processStartTimerWithDefaultConfig = processStartTimer sendMessage updateMessage checkIsCancelled (10, 1000)
    match command with
    | StartTimer startTimer -> Work.TimerWork (processStartTimerWithDefaultConfig startTimer)

let processMessage getParticipants sendMessage updateMessage (incomingMessage: IncomingMessage) =
    let getCommand incomingMessage =
        incomingMessage.Commands
            |> Seq.tryHead
            |> Option.bind (buildCommand getParticipants incomingMessage)
    
    let command = getCommand incomingMessage
    match command with
    | Some command -> processCommand sendMessage updateMessage command
    | _ -> NoWork