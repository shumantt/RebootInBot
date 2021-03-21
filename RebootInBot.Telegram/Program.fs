// Learn more about F# at http://fsharp.org

open System
open Funogram.Telegram.Bot
open Funogram.Telegram.Types
open RebootInBot.Bot
open RebootInBot.Types


let updateArrived (bot:Bot) (updateContext:UpdateContext) =
    let author (message: Funogram.Telegram.Types.Message) =
        message.From
        |> Option.bind (fun u -> u.Username)
        |> Option.defaultValue String.Empty
        |> ChatParticipant
        
    let text (message: Funogram.Telegram.Types.Message) =
         message.Text
        |> Option.defaultValue String.Empty
    let chat (message: Funogram.Telegram.Types.Message) =
        { ChatId = ChatId message.Chat.Id }
        
    let commands (message: Funogram.Telegram.Types.Message) (text:string) =
        message.Entities
        |> Option.defaultValue Seq.empty
        |> Seq.filter (fun x -> x.Type = "bot_command")
        |> Seq.map (fun x -> BotCommand text.[int32(x.Offset)..int32(x.Offset + x.Length - int64(1))])
    
    let getIncomingMessage =
        updateContext.Update.Message
        |> Option.map (fun m ->
            let text = text m
            { Author = author m
              Text = text
              Chat = chat m
              MessageId = MessageId m.MessageId
              Commands = commands m text })
        
    match getIncomingMessage with
    | Some incomingMessage -> bot.ProcessMessage(incomingMessage) |> Async.AwaitTask |> Async.RunSynchronously //todo why not async? https://github.com/Dolfik1/Funogram/issues/22
    | None _ -> ()


[<EntryPoint>]
let main argv =
    let token = argv.[0]
    let config = { defaultConfig with Token = token }
    use bot = Bot.Start(RebootInBot.Telegram.Messenger.Messenger(config))
    let updateArrived = updateArrived bot
    startBot config (updateArrived) None |> Async.RunSynchronously
    printfn "Hello World from F#!"
    0 
