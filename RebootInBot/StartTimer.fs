module RebootInBot.StartTimer

open System.Threading
open RebootInBot.Types
open RebootInBot.Mentions
open RebootInBot.TimerProcess

let private createTimerProcess updateMessage checkIsCancelled =
    let mutable numberOfUpdates = 10
    use cts = new CancellationTokenSource()
    let (task, timerTicks) = createTimer 1 numberOfUpdates

    timerTicks
    |> Observable.subscribe (fun _ ->
        updateMessage (sprintf "Перезапуск через %i" numberOfUpdates)
        numberOfUpdates <- numberOfUpdates - 1
        if checkIsCancelled () then cts.Cancel())
    |> ignore

    async { Async.Start(task, cts.Token) }

let buildStartTimer getParticipants (message: IncomingMessage) =
    { Chat = message.Chat
      Starter = message.Author
      ChatParticipants = getParticipants message.Chat }

let processStartTimer sendMessage updateMessage checkIsCancelled startTimerCommand =
    buildMentionList startTimerCommand.Starter startTimerCommand.ChatParticipants
    |> sendMessage startTimerCommand.Chat
    |> fun messageId ->
        let updateMessage = updateMessage startTimerCommand.Chat messageId
        let checkIsCancelled = checkIsCancelled startTimerCommand.Chat
        createTimerProcess updateMessage checkIsCancelled
