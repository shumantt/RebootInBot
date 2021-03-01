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
    let mutable messageSent = false
    let mutable updateCount = 0
    let sendMessage _ _ = messageSent <- true
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
    
    messageSent |> should equal true
    updateCount |> should equal N