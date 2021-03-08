module RebootInBot.CancelTimer

open RebootInBot.Types

let processCancelTimer getProcess deleteProcess sendMessage (cancelTimer: CancelTimer) =
    let chatProcess = getProcess cancelTimer.Chat.ChatId
    match chatProcess with
    | None -> ()
    | Some chatProcess ->
        deleteProcess chatProcess.ChatId |> ignore
        sendMessage cancelTimer.Chat (List.singleton chatProcess.Starter) "С перезапуском нужно подождать" |> ignore