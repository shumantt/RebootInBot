module RebootInBot.Extensions

open RebootInBot.Types

type IBotMessenger with
    member x.SendMessageAsync chat mentions text =
        x.SendMessage chat mentions text
        |> Async.AwaitTask

    member x.UpdateMessageAsync chat messageId text =
        x.UpdateMessage chat messageId text
        |> Async.AwaitTask

    member x.GetParticipantsAsync chat =
        x.GetParticipants chat
        |> Async.AwaitTask