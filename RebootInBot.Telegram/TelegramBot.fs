module RebootInBot.TelegramBot
    open RebootInBot
    open RebootInBot.Telegram
    open System.Net.Http
    open RebootInBot.Telegram.Types
    open RebootInBot.Processing.Types
    
    let config = {
      Token = ""
      Offset = Some 0L
      Limit = Some 100
      Timeout = 1000
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
    
    let run config handleUpdate =
        let rec loopAsync offset =
            async {
                let! updates = getUpdates config offset
                let newOffset = if (Seq.isEmpty updates) then offset
                                                         else  updates
                                                                |> Seq.map (fun f -> f.UpdateId)
                                                                |> Seq.max
                                                                |> fun x -> x + 1L
                updates
                |> Seq.filter(fun update -> update.Message.IsSome)
                |> Seq.map(fun update -> Commands.parseIncomingCommand update.Message.Value)
                |> Seq.filter(fun command -> command.IsSome)
                |> Seq.iter (fun command -> handleUpdate command.Value)
                
                do! Async.Sleep(config.Timeout)
                return! loopAsync newOffset
            }
        loopAsync (config.Offset |> Option.defaultValue 0L)