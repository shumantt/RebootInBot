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

let private sendToChat sendMessage (startTimer:StartTimer) =
    sendMessage startTimer.Chat
    
let private sendToChatWithStarter sendMessage (startTimer:StartTimer) =
    sendMessage startTimer.Chat (startTimer.Starter |> List.singleton)

let processStartTimer getParticipants sendMessage updateMessage saveProcess getProcess config startTimer =
    let sendToChat = sendToChat sendMessage startTimer
    let sendToChatWithStarter = sendToChatWithStarter sendMessage startTimer
    
    let saveProcess () =
        let chatProcess = {
            ChatId = startTimer.Chat.ChatId
            Starter = startTimer.Starter
        }
        saveProcess chatProcess
    
    let checkIsCancelled () =
        let chatProcess = getProcess startTimer.Chat.ChatId
        match chatProcess with
        | Some _ -> false
        | None -> true
    
    getParticipants startTimer.Chat
    |> buildMentionList startTimer.Starter 
    |> fun mentions -> sendToChat mentions "Буду перезапускать, никто не против?"
    |> fun messageId ->
        let updateMessage = updateMessage startTimer.Chat messageId
        saveProcess()
        createTimerProcess sendToChatWithStarter updateMessage checkIsCancelled config
        
let processThrottled sendMessage (startTimer:StartTimer) =
    sendToChatWithStarter sendMessage startTimer "Не можем обработаь ваш запрос"