module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Mentions

let private createTimerProcess updateMessage checkIsCancelled =
    async {
        for i in 10.. -1 .. 1 do
          updateMessage (sprintf "Перезапуск через %i" i)
          let cancelled = checkIsCancelled ()
          if cancelled then
              Async.CancelDefaultToken()
          do! Async.Sleep 10_000
    }

let buildStartTimer getParticipants (message: IncomingMessage) =
    { Chat = message.Chat
      Starter = message.Author
      ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage updateMessage checkIsCancelled startTimerCommand =
    buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
    |> sendMessage startTimerCommand.Chat
    |> fun messageId ->
        let updateMessage = updateMessage startTimerCommand.Chat messageId
        let checkIsCancelled = checkIsCancelled startTimerCommand.Chat
        createTimerProcess updateMessage checkIsCancelled
