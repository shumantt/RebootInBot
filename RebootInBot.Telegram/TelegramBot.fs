module RebootInBot.TelegramBot
    open RebootInBot
    open RebootInBot.Commands.Types
    open System.Net.Http
    open RebootInBot.Telegram.Types
    
    let config = {
      Token = ""
      Offset = Some 0L
      Limit = Some 100
      Timeout = 10000
      Client = new HttpClient()
    }
    
    let private getUri token methodName =
        sprintf "https://api.telegram.org/bot%s/%s" token methodName
    
    let private getUpdates config offset =
        async {
            let url = sprintf "%s?offset=%d" (getUri config.Token "getUpdates") offset
            let! response = config.Client.GetAsync url |> Async.AwaitTask
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let updates = Tools.parseJson<Update[]> json
            return updates
        }
    
    let private sendResponse (command:OutgoingCommand) =
        //let buildMessage comm = 
        async {
            let url = getUri config.Token "sendMessage"
            url
        }
    
    let private handleIncomming update (handleUpdate:IncomingCommand->OutgoingCommand) =
        match update.Message with
            | Some message ->
                    message
                    |> Commands.parseIncomingCommand message
                    |> handleUpdate 
    
    let run config (handleUpdate:IncomingCommand->OutgoingCommand) =
        let rec loopAsync offset =
            async {
                getUpdates config offset
                    |> Async.RunSynchronously
                    |> Seq.filter(fun update -> update.Message.IsSome)
                    |> Seq.map(fun update -> Commands.parseIncomingCommand update.Message.Value)
                    |> Seq.filter(fun command -> command.IsSome)
                    |> Seq.map (fun command -> handleUpdate command.Value)
                
                outgoingCommands
                |> Seq.iter(fun command -> sendResponse command)
                
                let newOffset = if (Seq.isEmpty updates) then offset
                                                         else  updates
                                                                |> Seq.map (fun f -> f.UpdateId)
                                                                |> Seq.max
                                                                |> fun x -> x + 1L
                do! Async.Sleep(config.Timeout)
                return! loopAsync newOffset
            }
        loopAsync (config.Offset |> Option.defaultValue 0L)