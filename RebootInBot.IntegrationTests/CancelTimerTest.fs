module RebootInBot.IntegrationTests.CancelTimerTest

open System
open RebootInBot.Bot
open RebootInBot.IntegrationTests.Mocks.MockMessenger
open RebootInBot.Types
open Xunit
open FsUnit

[<Fact>]
let ``Test cancel timer`` () =
    let sent = ResizeArray([])
    let updates = ResizeArray([])
    let messageId = Guid.NewGuid()
    let onUpdate chat messageId text =
        updates.Add (chat, messageId, text)
    let onSend (chat, participants, text) =
        sent.Add (chat, participants, text)
        messageId
    let messenger = MockMessenger(onSend, onUpdate, ["author";"participant1";"participant2"])
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let startTimerMessage: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/reboot" ] }
    let cancelTimerMessage: IncomingMessage =
        { Chat = chat
          Author = "participant1"
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/cancel" ] }
        
    
    use bot = Bot.Start(messenger)
    
    bot.ProcessMessage(startTimerMessage)
    
    Async.Sleep(5_000) |> Async.RunSynchronously
    
    bot.ProcessMessage(cancelTimerMessage)
    
    updates.Count |> should lessThan 10
    
    sent.Count |> should equal 3
    let (thirdChat, thirdParticipants, thirdText) = sent.[2]
    thirdChat |> should equal chat
    thirdParticipants |> should equal ["author"]
    thirdText |> should equal "С перезапуском нужно подождать"
    