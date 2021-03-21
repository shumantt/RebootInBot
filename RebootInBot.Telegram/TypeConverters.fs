module RebootInBot.Telegram.TypeConverters

type RebootInBot.Types.ChatId with
    member x.ToTelegramChatId() =
       let (RebootInBot.Types.ChatId id) = x
       id
       
type RebootInBot.Types.MessageId with
    member x.ToTelegramMessageId() =
       let (RebootInBot.Types.MessageId id) = x
       id
