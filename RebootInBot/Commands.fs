module RebootInBot.Commands

open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand getParticipants (message: IncomingMessage) (command: BotCommand) =
    match command with
    | "/reboot" -> Some(Command.StartTimer(buildStartTimer getParticipants message))
    | _ -> None

let private processCommand sendMessage updateMessage checkIsCancelled command =
    let processStartTimerWithDefaultConfig =
        processStartTimer sendMessage updateMessage checkIsCancelled (10, 1000)

    match command with
    | StartTimer startTimer -> TimerWork(processStartTimerWithDefaultConfig startTimer)

let processMessage getParticipants sendMessage updateMessage checkIsCancelled (incomingMessage: IncomingMessage) =
    incomingMessage.Commands
    |> Seq.tryHead
    |> Option.bind (buildCommand getParticipants incomingMessage)
    |> Option.map (processCommand sendMessage updateMessage checkIsCancelled)
    
