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
                                    State = IdleState
                                    Config = { ExcludeMembers = [||] } }
        
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
                                    State = IdleState
                                    Config = { ExcludeMembers = [|  |] } }
        let updatedProcess = { processObj with State = CountingState { Count = 10; UserStarted = "@testUser" };
                                               Config = { ExcludeMembers = [|"excluded"|] } }
        
        createdIds <- [| processObj.Id.ToString() |]
        createOrUpdateProcess defaultRedisClientFactory processObj |> ignore //create
        createOrUpdateProcess defaultRedisClientFactory updatedProcess |> ignore //update   
        let fromDb = getProcess defaultRedisClientFactory processObj.Id
         
        fromDb |> should equal <| Some updatedProcess
    finally
        tearDown createdIds |> ignore
