module RebootInBot.StartTimer

open System
open RebootInBot.Types
open RebootInBot.Mentions

let private startTimerProcess chat messageId =
    Console.WriteLine("Started Timer")

let buildStartTimer getParticipants (message:IncomingMessage) =
        { Chat = message.Chat
          Starter = message.Author
          ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage startTimerCommand =
        buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
            |> sendMessage startTimerCommand.Chat
            |> startTimerProcess startTimerCommand.Chat