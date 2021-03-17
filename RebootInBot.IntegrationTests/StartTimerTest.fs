module RebootInBot.IntegrationTests.StartTimerTest

open System
open System.Threading.Tasks
open Xunit
open FsUnit
open RebootInBot.Types
open RebootInBot.Bot
open RebootInBot.IntegrationTests.Mocks.MockMessenger

[<Fact>]
let ``Test start timer`` () =
    let sent = ResizeArray([])
    let updates = ResizeArray([])
    let messageId = Guid.NewGuid()
    let onUpdate chat messageId text =
        updates.Add (chat, messageId, text)
        Task.CompletedTask
    let onSend (chat, participants, text) =
        sent.Add (chat, participants, text)
        Task.FromResult(messageId)
    let messenger = MockMessenger(onSend, onUpdate, ["author";"participant1";"participant2"] |> List.toSeq)
    let chat = { ChatId = Guid.NewGuid() }
    let author = "author"
    let message: IncomingMessage =
        { Chat = chat
          Author = author
          Text = "text"
          MessageId = Guid.NewGuid()
          Commands = [ "/reboot" ] }
    
    use bot = Bot.Start(messenger)
    
    bot.ProcessMessage(message) |> Async.AwaitTask |> Async.RunSynchronously
    
    Async.Sleep(12_000) |> Async.RunSynchronously
    
    updates.Count |> should equal 11
    (updates.TrueForAll (fun (c, m, _) -> c = chat && m = messageId)) |> should equal true
    
    sent.Count |> should equal 3
    
    let (firstChat, firstParticipants, _) = sent.[0]
    firstChat |> should equal chat
    firstParticipants |> should equal ["participant1";"participant2"]
    
    let (secondChat, secondParticipants, _) = sent.[0]
    secondChat |> should equal chat
    secondParticipants |> should equal ["participant1";"participant2"]
    
    let (thirdChat, thirdParticipants, _) = sent.[2]
    thirdChat |> should equal chat
    thirdParticipants |> should equal ["author"]
    