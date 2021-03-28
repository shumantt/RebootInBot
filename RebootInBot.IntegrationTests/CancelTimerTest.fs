module RebootInBot.IntegrationTests.CancelTimerTest

open System.Threading.Tasks
open RebootInBot.Bot
open RebootInBot.IntegrationTests.Mocks.MockMessenger
open RebootInBot.Types
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Test cancel timer`` () =
    let sent = ResizeArray([])
    let updates = ResizeArray([])
    let messageId = MessageId 29L
    let onUpdate chat messageId text =
        updates.Add (chat, messageId, text)
        Task.CompletedTask
    let onSend (chat, participants, text) =
        sent.Add (chat, participants, text)
        Task.FromResult(messageId)
    let messenger = MockMessenger(onSend, onUpdate, [ChatParticipant "author"; ChatParticipant "participant1"; ChatParticipant "participant2"])
    let chat = { ChatId = ChatId 30L }
    let author = ChatParticipant "author"
    let startTimerMessage: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = MessageId 31L
          Commands = [ BotCommand "/reboot" ] }
    let cancelTimerMessage: IncomingMessage =
        { Chat = chat
          Author = ChatParticipant "participant1"
          Text = "text"
          MessageId = MessageId 32L
          Commands = [ BotCommand "/cancel" ] }
        
    
    use bot = Bot.Start(messenger)
    
    bot.ProcessMessage(startTimerMessage) |> Async.AwaitTask |> Async.RunSynchronously
    
    Async.Sleep(5_000) |> Async.RunSynchronously
    
    bot.ProcessMessage(cancelTimerMessage) |> Async.AwaitTask |> Async.RunSynchronously
    
    updates.Count |> should lessThan 10
    
    sent.Count |> should equal 3
    let (thirdChat, thirdParticipants, thirdText) = sent.[2]
    thirdChat |> should equal chat
    thirdParticipants |> should equal [author]
    thirdText |> should equal "С перезапуском нужно подождать"
    