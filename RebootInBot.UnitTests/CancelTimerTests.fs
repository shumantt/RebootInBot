module RebootInBot.Tests.CancelTimerTests

open System
open RebootInBot.Types
open RebootInBot.CancelTimer
open Xunit
open FsUnit

[<Fact>]
let ``processCancelTimer cancels existing chat process`` () =
    let chatId = Guid.NewGuid()

    let cancelTimer =
        { Chat = { ChatId = chatId } }

    let getProcess chatId =
        Some { ChatId = chatId; Starter = "starter" }

    let mutable deletedChat = Guid.Empty
    let deleteProcess chatId =
        deletedChat <- chatId
    
    let mutable messageSentСount = 0
    let sendMessage _ _ _ =
        messageSentСount <- messageSentСount + 1
    
    processCancelTimer getProcess deleteProcess sendMessage cancelTimer
    
    messageSentСount |> should equal 1
    deletedChat |> should equal chatId