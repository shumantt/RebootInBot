module RebootInBot.Tests.StartTimerTests

open System
open RebootInBot.StartTimer
open RebootInBot.Types
open Xunit
open FsUnit
    

[<Fact>]
let  ``test buildStartTimer`` () =
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/reboot" ] }
    
    let actual = buildStartTimerCommand message
    
    let expected =
        { Chat = chat
          Starter = author }
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
    let getProcess chatId =
        Some {
            ChatId = chatId
            Starter = "starter"
        }
    let saveProcess chatId =
        ()
    let getParticipants _ =
        ["starter"; "participant"]
    let startTimer =
          { Chat = { ChatId = Guid.NewGuid() }
            Starter = "starter" }
    let config = {
        Delay = 0
        CountsNumber = N
    }
    
    processStartTimer getParticipants sendMessage updateMessage saveProcess getProcess config startTimer
    |> Async.RunSynchronously
    
    messageSentСount |> should equal 2
    updateCount |> should equal N
    
[<Fact>]
let ``processStartTimer is cancelable`` () =
    let N = 10
    let canceltAt = 3
    let mutable messages = ResizeArray([])
    let mutable updateCount = 0
    let sendMessage _ _ text = messages.Add text
    let updateMessage _ _ _ =
        updateCount <- updateCount + 1
    
    let getProcess chatId =
        match updateCount = canceltAt with
        | true -> None
        | false ->  Some {
            ChatId = chatId
            Starter = "starter"
        }
                               
    let saveProcess chatId =
        ()
        
    let getParticipants _ =
        ["starter"; "participant"]
    let startTimer =
          { Chat = { ChatId = Guid.NewGuid() }
            Starter = "starter" }
    let config = {
        Delay = 0
        CountsNumber = N
    }
    
    processStartTimer getParticipants sendMessage updateMessage saveProcess getProcess config startTimer
    |> Async.RunSynchronously
    
    messages.Count |> should equal 1
    updateCount |> should equal canceltAt