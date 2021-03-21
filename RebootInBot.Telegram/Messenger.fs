module RebootInBot.Telegram.Messenger

open System
open System.Linq
open System.Threading.Tasks
open Funogram.Telegram
open RebootInBot.Types
open Funogram.Api
open TypeConverters

let sendMessage (chat: Chat) mentions text config =
    let mentionsList =
        mentions
        |> Seq.map (fun mention ->
            let (ChatParticipant nickname) = mention
            "@" + nickname)
        |> String.concat " "

    let messsage = mentionsList + " " + text

    Api.sendMessage (chat.ChatId.ToTelegramChatId()) messsage
    |> api config

let updateMessage (chat: Chat) (messageId: MessageId) newText config =
    let chatId =
        Types.ChatId.Int(chat.ChatId.ToTelegramChatId())

    Api.editMessageTextBase (Some chatId) (Some(messageId.ToTelegramMessageId())) None newText None None None
    |> api config

type Messenger(config: Funogram.Types.BotConfig) =
    interface IBotMessenger with
        member x.SendMessage chat mentions text =
            let send =
                async {
                    let! result = sendMessage chat mentions text config

                    match result with
                    | Ok sendResult -> return MessageId sendResult.MessageId
                    | _ -> return raise (Exception("Error sending message")) //todo
                }

            send |> Async.StartAsTask

        member this.GetParticipants chat =
            Task.FromResult(Enumerable.Empty<ChatParticipant>()) //todo redo

        member this.UpdateMessage chat messageId newText =
            let update =
                async {
                    let! result = updateMessage chat messageId newText config

                    match result with
                    | Ok _ -> ()
                    | _ -> return raise (Exception("Error updating message")) //todo
                }
            update |> Async.StartAsTask :> Task
