module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Mentions

let private createTimerProcess sendFinished updateMessage checkIsCancelled (countTimes, delay) =
    let rec doUpdates count =
        async {
             let cancelled = checkIsCancelled ()
             if not cancelled then
                 if count > 0 then
                    updateMessage (sprintf "Перезапуск через %i" count)
                    do! Async.Sleep delay
                    do! doUpdates (count - 1)
                 else
                     sendFinished "Поехали!"           
        }
        
    doUpdates countTimes

let buildStartTimer getParticipants (message: IncomingMessage) =
    { Chat = message.Chat
      Starter = message.Author
      ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage updateMessage checkIsCancelled config startTimerCommand =
    let sendToChat = sendMessage startTimerCommand.Chat
    let sendToChatWithAuthor = sendToChat (startTimerCommand.Starter |> List.singleton)
    buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
    |> fun mentions -> sendToChat mentions "Буду перезапускать, никто не против?"
    |> fun messageId ->
        let updateMessage = updateMessage startTimerCommand.Chat messageId
        let checkIsCancelled = checkIsCancelled startTimerCommand.Chat
        createTimerProcess sendToChatWithAuthor updateMessage checkIsCancelled config
