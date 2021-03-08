module RebootInBot.IntegrationTests.Mocks.MockMessenger

open System
open RebootInBot.Types

type MockMessenger(onSend, onUpdate, participants) =
    interface IBotMessenger with
        member x.SendMessage (chat) (chatParticipants) (text) =
            onSend(chat, chatParticipants, text)

        member this.GetParticipants(chat) =
            participants
        
        member this.UpdateMessage(chat) (messageId) (text) =
            onUpdate chat messageId text    