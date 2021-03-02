module RebootInBot.Tests.StartTimerTests

open System
open RebootInBot.StartTimer
open RebootInBot.Types
open Xunit
open FsUnit
    

[<Fact>]
let  ``test buildStartTimer`` () =
    let participants = ["participant 1"; "participant 2"]
    let getParticipants _ =
        participants
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/reboot" ] }
    
    let actual = buildStartTimer getParticipants message
    
    let expected =
        { Chat = chat
          Starter = author
          ChatParticipants = participants }
    actual |> should equal expected

[<Fact>]
let ``processStartTimer sends message and updates it N times if not cancelled`` () =
    let N = 10
    let mutable messageSentСount = 0
    let mutable updateCount = 0
    let sendMessage _ _ _ = messageSentСount <- messageSentСount + 1
    let updateMessage _ _ (text:string) =
        Console.WriteLine(text)
        updateCount <- updateCount + 1
    let checkIsCancelled _ () = false
    let startTimer =
          { Chat = { ChatId = Guid.NewGuid() }
            Starter = "starter"
            ChatParticipants = ["starter"; "p2"] }
    
    processStartTimer sendMessage updateMessage checkIsCancelled (N, 0) startTimer
    |> Async.RunSynchronously
    
    messageSentСount |> should equal 2
    updateCount |> should equal N
    
[<Fact>]
let ``processStartTimer is cancelable`` () =
    let N = 10
    let canceltAt = 3
    let mutable messageSentСount = 0
    let mutable updateCount = 0
    let sendMessage _ _ _ = messageSentСount <- messageSentСount + 1
    let updateMessage _ _ _ =
        updateCount <- updateCount + 1
    let checkIsCancelled _ () = updateCount = canceltAt
    let startTimer =
          { Chat = { ChatId = Guid.NewGuid() }
            Starter = "starter"
            ChatParticipants = ["starter"; "p2"] }
    
    processStartTimer sendMessage updateMessage checkIsCancelled (N, 0) startTimer
    |> Async.RunSynchronously
    
    messageSentСount |> should equal 1
    updateCount |> should equal canceltAt