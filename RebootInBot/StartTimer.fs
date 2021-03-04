module RebootInBot.StartTimer

open RebootInBot.Types
open RebootInBot.Mentions

let private createTimerProcess sendFinished updateMessage checkIsCancelled config =
    let rec doUpdates count =
        async {
             let cancelled = checkIsCancelled ()
             if not cancelled then
                 if count > 0 then
                    updateMessage (sprintf "Перезапуск через %i" count)
                    do! Async.Sleep config.Delay
                    do! doUpdates (count - 1)
                 else
                     sendFinished "Поехали!" |> ignore      
        }
        
    doUpdates config.CountsNumber

let buildStartTimerCommand (message:IncomingMessage) =
    {
       Chat = message.Chat
       Starter = message.Author
    }

let processStartTimer getParticipants sendMessage updateMessage checkIsCancelled config startTimer =
    let sendToChat = sendMessage startTimer.Chat
    let sendToChatWithStarter = sendToChat (startTimer.Starter |> List.singleton)
    let checkIsCancelled = checkIsCancelled startTimer.Chat
    
    getParticipants startTimer.Chat
    |> buildMentionList startTimer.Starter 
    |> fun mentions -> sendToChat mentions "Буду перезапускать, никто не против?"
    |> fun messageId ->
        let updateMessage = updateMessage startTimer.Chat messageId
        createTimerProcess sendToChatWithStarter updateMessage checkIsCancelled config
