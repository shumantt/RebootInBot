module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Mentions
open RebootInBot.TimerProcess

let private createTimerProcess updateMessage =
    let mutable numberOfUpdates = 10
    let (task, timerTicks) = createTimer 1 numberOfUpdates
    timerTicks
        |> Observable.subscribe (fun _ ->
            updateMessage (sprintf "Перезапуск через %i" numberOfUpdates)
            numberOfUpdates <- numberOfUpdates - 1)
        |> ignore
    task

let buildStartTimer getParticipants (message:IncomingMessage) =
        { Chat = message.Chat
          Starter = message.Author
          ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage updateMessage startTimerCommand =
        buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
            |> sendMessage startTimerCommand.Chat
            |> fun messageId ->
                let updateMessage = updateMessage startTimerCommand.Chat messageId
                createTimerProcess updateMessage