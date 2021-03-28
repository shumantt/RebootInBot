module RebootInBot.Tests.CommandsTests

open Xunit
open FsUnit.Xunit
open RebootInBot.Types
open RebootInBot.Commands

[<Fact>]
let ``parseCommand should return StartTimer on /reboot command`` () =
    let chat = { ChatId = ChatId 24L }
    let author = ChatParticipant "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = MessageId 52L
          Commands = [ BotCommand "/reboot" ] }
    let expected =
        { Chat = chat
          Starter = author }
    
    let actual = parseCommand message
    
    actual |> should equal (Some (Command.StartTimerCommand expected) )

[<Fact>]
let ``parseCommand should return CancelTimer on /cancel command`` () =
    let chat = { ChatId = ChatId 53L }
    let author = ChatParticipant "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = MessageId 20L
          Commands = [ BotCommand "/cancel" ] }
    let expected =
        { Chat = chat  }
    
    let actual = parseCommand message
    
    actual |> should equal (Some (Command.CancelTimerCommand expected) )

[<Fact>]    
let ``parseCommand should return None on unknown command`` () =
    let chat = { ChatId = ChatId 596L }
    let author = ChatParticipant "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = MessageId 88L
          Commands = [ BotCommand "/unknown" ] }
    
    let actual = parseCommand message
    
    actual |> should equal None