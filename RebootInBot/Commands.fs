module RebootInBot.Commands

open RebootInBot.Types
open RebootInBot.StartTimer

let private buildCommand getParticipants (message: IncomingMessage) (command: BotCommand) =
    match command with
    | "/reboot" -> Some(Command.StartTimer(buildStartTimer getParticipants message))
    | _ -> None

let private processCommand sendMessage updateMessage checkIsCancelled config command =
    let processStartTimerWithConfig =
        processStartTimer sendMessage updateMessage checkIsCancelled config

    match command with
    | StartTimer startTimer -> TimerWork(processStartTimerWithConfig startTimer)

let processMessage getParticipants sendMessage updateMessage checkIsCancelled config (incomingMessage: IncomingMessage) =
    incomingMessage.Commands
    |> Seq.tryHead
    |> Option.bind (buildCommand getParticipants incomingMessage)
    |> Option.map (processCommand sendMessage updateMessage checkIsCancelled config)
    
