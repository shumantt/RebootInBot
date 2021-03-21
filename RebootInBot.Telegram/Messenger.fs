module RebootInBot.Telegram.Messenger

open System
open System.Linq
open System.Threading.Tasks
open Funogram.Telegram
open RebootInBot.Types
open Funogram.Api

let sendMessage (chat:Chat) mentions text accessToken =
    let messsage = (mentions |> String.concat " ") + text
    Api.sendMessage chat.ChatId messsage
    |> api {Bot.defaultConfig with Token = accessToken}
    
let updateMessage (chat:Chat) messageId newText accessToken =
    let chatId =  Types.ChatId.Int chat.ChatId
    Api.editMessageTextBase (Some chatId) (Some messageId) None newText None None None
    |> api {Bot.defaultConfig with Token = accessToken}

type Messenger(token:string) =
    interface IBotMessenger with
        member x.SendMessage chat mentions text =
            let send =
                async {
                    let! result = sendMessage chat mentions text token
                    match result with
                    | Ok sendResult -> return sendResult.MessageId
                    | _ -> return raise (Exception("Error sending message")) //todo
                }       
            
            send |> Async.StartAsTask

        member this.GetParticipants chat =
            Task.FromResult(Enumerable.Empty<ChatParticipant>()) //todo redo
        
        member this.UpdateMessage chat messageId newText =
            let update =
                async {
                    let! result = updateMessage chat messageId newText token
                    match result with
                    | Ok _ -> ()
                    | _ -> return raise (Exception("Error updating message")) //todo
                }
            
            update |> Async.StartAsTask :> Task
        