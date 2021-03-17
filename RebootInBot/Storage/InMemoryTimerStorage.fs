module RebootInBot.Storage.InMemoryStorage

open System.Collections.Generic
open RebootInBot.Types
open RebootInBot.Helpers

type InMemoryTimerStorage() =
    let processesStorage = Dictionary<TimerId, RunningTimer>()
    
    let toId (chat:Chat) =
        chat.ChatId
    
    interface ITimerStorage with
        member this.Save(timer) =
            processesStorage.Add(timer.Id, timer)
            liftAsync ActionResult.Success
        
        member this.Get(timerId) =
            let found, timer = processesStorage.TryGetValue (timerId)
            if(found) then
                liftAsync (Timer.RunningTimer timer)
            else
                liftAsync (Timer.InactiveTimer { Id = timerId })
            
        member this.Delete(timer) =
            match (processesStorage.Remove timer.Id) with
            | true -> liftAsync ActionResult.Success
            | false -> liftAsync ActionResult.Fail
            
    