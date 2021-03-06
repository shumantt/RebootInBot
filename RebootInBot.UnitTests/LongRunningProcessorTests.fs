module RebootInBot.Tests.LongRunningProcessorTests

open RebootInBot.Processing
open RebootInBot.Types
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``should throttle with limit 0`` () =
    let processWork _ = async { () }
    let processor = LongRunningProcessor.Start(processWork, 0)
    
    let actual = processor.StartLongRunningTask("nothing")
    
    actual |> should equal Throttled

[<Fact>]
let ``should start operation when not over the limit`` () =
    let mutable started = false
    let processWork _ = async { started <- true }
    let processor = LongRunningProcessor.Start(processWork, 1)
    
    let actualResult = processor.StartLongRunningTask("nothing")
    
    Async.Sleep 10 |> Async.RunSynchronously
                           
    actualResult |> should equal Started
    started |> should equal true
    
    
[<Fact>]
let ``should throttle when over the limit`` () =
    let processWork _ = Async.Sleep(100)
    let processor = LongRunningProcessor.Start(processWork, 1)
    
    processor.StartLongRunningTask("nothing") |> ignore
             
    let actual = processor.StartLongRunningTask("something")          
    actual |> should equal Throttled
    

[<Fact(Skip="todo")>]
let ``should call onWorkFail if fails`` () =
    let mutable onFailedCalled = false
    let processWork _ = async { failwith "failed" }
    let onWorkFailed _ = onFailedCalled <- true
    let processor = LongRunningProcessor<_>.Start(processWork, 1, onWorkFailed)
    
    let actual = processor.StartLongRunningTask("something")
    
    Async.Sleep 10 |> Async.RunSynchronously
    actual |> should equal Started
    onFailedCalled |> should equal true 
    