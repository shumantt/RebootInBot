module RebootInBot.Telegram.Commands
open RebootInBot.Telegram.Types
open RebootInBot.Processing.Types

[<Literal>]
let botCommandType = "bot_command"

let parseIncomingCommand (message:Message) =
    let build machineName commandType =
        { ProcessId = message.Chat.Id
          Type = commandType
          Source = CommandSource.Telegram
          Data = { FromUser = message.From.Value.Username.Value
                   MachineName = machineName } }
   
    let firstBotCommand (entities: seq<MessageEntity>) =
        entities
        |> Seq.tryFind(fun x -> x.Type.Equals botCommandType)
    
    let parseCommand (commandEntity:MessageEntity) =
        let parseCommandType (command:string) =
            match command with
            | "/reboot" -> Some IncomingCommandType.StartTimer
            | "/cancel" -> Some IncomingCommandType.CancelTimer
            | "/exclude" -> Some IncomingCommandType.ExcludeMember
            | "/include" -> Some IncomingCommandType.IncludeMember
            | _ -> None
        
        let parseMachineName text command = 
            let (|Command|_|) (p:string) (s:string) =
                if s.StartsWith(p) then
                    let machineName = s.Substring(p.Length).Trim() 
                    if machineName.Length > 0 then
                        Some machineName
                    else
                        None
                else
                    None
            match text with
            | Command command machineName -> Some machineName
            | _ -> None
        
        let getCommandText offset length =
            let offsetParam = int32(offset)
            let lengthParam = int32(length)
            message.Text.Value.Substring(offsetParam, lengthParam)
        
        let commandText = getCommandText commandEntity.Offset commandEntity.Length 
        parseCommandType commandText
        |> Option.bind(fun x -> Some (x, (parseMachineName message.Text.Value commandText)))
            
    let entities = match message.Entities with
                    | Some x -> x
                    | _ -> Seq.empty<MessageEntity>
     
    let commandData = entities
                      |> firstBotCommand
                      |> Option.bind(fun x -> parseCommand x)
    match commandData with
    | Some (commandType, machineName) -> Some (build machineName commandType)
    | _ -> None