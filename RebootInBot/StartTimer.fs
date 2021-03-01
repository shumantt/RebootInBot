module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Mentions

let private createTimerProcess updateMessage checkIsCancelled (countTimes, delay) =
    async {
        for i in countTimes .. -1 .. 1 do
          updateMessage (sprintf "Перезапуск через %i" i)
          let cancelled = checkIsCancelled ()
          if cancelled then
              Async.CancelDefaultToken()
          do! Async.Sleep delay
    }

let buildStartTimer getParticipants (message: IncomingMessage) =
    { Chat = message.Chat
      Starter = message.Author
      ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage updateMessage checkIsCancelled config startTimerCommand =
    buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
    |> sendMessage startTimerCommand.Chat
    |> fun messageId ->
        let updateMessage = updateMessage startTimerCommand.Chat messageId
        let checkIsCancelled = checkIsCancelled startTimerCommand.Chat
        createTimerProcess updateMessage checkIsCancelled config
