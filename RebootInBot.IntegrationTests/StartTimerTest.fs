module RebootInBot.IntegrationTests.StartTimerTest

open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open RebootInBot.Types
open RebootInBot.Bot
open RebootInBot.IntegrationTests.Mocks.MockMessenger

[<Fact>]
let ``Test start timer`` () =
    let sent = ResizeArray([])
    let updates = ResizeArray([])
    let messageId = MessageId 106L
    let onUpdate chat messageId text =
        updates.Add (chat, messageId, text)
        Task.CompletedTask
    let onSend (chat, participants, text) =
        sent.Add (chat, participants, text)
        Task.FromResult(messageId)
    let messenger = MockMessenger(onSend, onUpdate, [ChatParticipant "author"; ChatParticipant "participant1"; ChatParticipant "participant2"] |> List.toSeq)
    let chat = { ChatId = ChatId 108L }
    let author = ChatParticipant "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = MessageId 924L
          Commands = [ BotCommand "/reboot" ] }
    
    use bot = Bot.Start(messenger)
    
    bot.ProcessMessage(message) |> Async.AwaitTask |> Async.RunSynchronously
    
    Async.Sleep(12_000) |> Async.RunSynchronously
    
    updates.Count |> should equal 11
    (updates.TrueForAll (fun (c, m, _) -> c = chat && m = messageId)) |> should equal true
    
    sent.Count |> should equal 3
    
    let (firstChat, firstParticipants, _) = sent.[0]
    firstChat |> should equal chat
    Assert.Equal([ChatParticipant "participant1"; ChatParticipant "participant2"], firstParticipants)
    
    let (secondChat, secondParticipants, _) = sent.[0]
    secondChat |> should equal chat
    Assert.Equal([ChatParticipant "participant1"; ChatParticipant "participant2"], secondParticipants)
    
    let (thirdChat, thirdParticipants, _) = sent.[2]
    thirdChat |> should equal chat
    Assert.Equal([author], thirdParticipants)
    