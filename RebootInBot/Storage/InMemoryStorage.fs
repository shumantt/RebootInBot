module RebootInBot.Storage.InMemoryStorage

open System.Collections.Generic
open RebootInBot.Types

type InMemoryStorage() =
    let processesStorage = Dictionary<ChatId, Process>()
    
    interface IStorage with
        member this.SaveProcess(chatProcess) =
            processesStorage.Add(chatProcess.ChatId, chatProcess)
    
        member this.GetProcess(chatId: ChatId) =
            let found, chatProcess = processesStorage.TryGetValue chatId
            if(found) then
                Some chatProcess
            else
                None
            
        member this.DeleteProcess(chatId: ChatId) =
            processesStorage.Remove chatId |> ignore