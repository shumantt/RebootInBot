module ProcessStorageTests

open FsUnit
open Xunit
open RebootInBot.Processing.Types
open RebootInBot.Processing.ProcessStorage

let tearDown createdIds =
    use client = defaultRedisClientFactory()
    client.RemoveEntry(createdIds)

[<Fact>]
let ``Create and get new process`` () =
    let mutable createdIds = [||]
    try
        let processObj: Process = { Id = 1L
                                    State = { CurrentState = Counting
                                              Count = None
                                              UserStarted = None }
                                    Config = { ExcludeMembers = None } }
        
        createdIds <- [| processObj.Id.ToString() |]
        createOrUpdateProcess defaultRedisClientFactory processObj |> ignore
        let fromDb = getProcess defaultRedisClientFactory processObj.Id
    
        fromDb |> should equal <| Some processObj
    finally
        tearDown createdIds |> ignore
    

[<Fact>]
let ``Update and get process`` () =
    let mutable createdIds = [||]
    try
        let processObj: Process = { Id = 2L
                                    State = { CurrentState = Idle
                                              Count = None
                                              UserStarted = None }
                                    Config = { ExcludeMembers = None } }
        let updatedProcess = { processObj with State = { CurrentState = Counting
                                                         Count = Some 10
                                                         UserStarted = Some "@testUser" };
                                                Config = { ExcludeMembers = Some ([|"excluded"|]) } }
        
        createdIds <- [| processObj.Id.ToString() |]
        createOrUpdateProcess defaultRedisClientFactory processObj |> ignore //create
        let fromDb1 = getProcess defaultRedisClientFactory processObj.Id 
        createOrUpdateProcess defaultRedisClientFactory updatedProcess |> ignore //update   
        let fromDb = getProcess defaultRedisClientFactory processObj.Id
    
       
    
        fromDb |> should equal <| Some updatedProcess
    finally
        tearDown createdIds |> ignore
