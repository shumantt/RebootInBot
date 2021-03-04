module RebootInBot.Tests.CommandsTests

open System
open Xunit
open FsUnit
open RebootInBot.Types
open RebootInBot.Commands

[<Fact>]
let ``parseCommand should return StartTimer on /reboot command`` () =
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/reboot" ] }
    let expected =
        { Chat = chat
          Starter = author }
    
    let actual = parseCommand message
    
    actual |> should equal (Some (Command.StartTimer expected) )
    
let ``parseCommand should return None on unknown command`` () =
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/unknown" ] }
    
    let actual = parseCommand message
    
    actual |> should equal None