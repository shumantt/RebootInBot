module RebootInBot.Telegram.Types

open System
open System.Net.Http
open System.Runtime.Serialization

type BotConfig = 
    { Token: string
      Offset: int64 option
      Limit: int option
      Timeout: int
      Client: HttpClient }

type ChatType =
| Private
| Group
| [<DataMember(Name = "supergroup")>] SuperGroup
| Channel
| Unknown

[<CLIMutable>]
/// This object represents a Telegram user or bot.
type User =
  { /// Unique identifier for this user or bot
    Id: int64
    /// True, if this user is a bot
    IsBot: bool
    /// User‘s or bot’s first name
    FirstName: string
    /// User‘s or bot’s last name
    LastName: string option
    /// User‘s or bot’s username
    Username: string option
    /// IETF language tag of the user's language
    LanguageCode: string option }

[<CLIMutable>]
/// This object represents a chat photo
type ChatPhoto =
  { /// Unique file identifier of small (160x160) chat photo. This file_id can be used only for photo download.
    SmallFileId: string
    /// Unique file identifier of big (640x640) chat photo. This file_id can be used only for photo download.
    BigFileId: string }

[<CLIMutable>]
/// Describes actions that a non-administrator user is allowed to take in a chat.
type ChatPermissions =
  {
    /// True, if the user is allowed to send text messages, contacts, locations and venues
    CanSendMessages: bool option
    /// True, if the user is allowed to send audios, documents, photos, videos, video notes and voice notes, implies can_send_messages
    CanSendMediaMessages: bool option
    /// True, if the user is allowed to send polls, implies can_send_messages
    CanSendPools: bool option
    /// True, if the user is allowed to send animations, games, stickers and use inline bots, implies can_send_media_messages
    CanSendOtherMessages: bool option
    /// True, if the user is allowed to add web page previews to their messages, implies can_send_media_messages
    CanAddWebPagePreviews: bool option
    /// True, if the user is allowed to change the chat title, photo and other settings. Ignored in public supergroups
    CanChangeInfo: bool option
    /// True, if the user is allowed to invite new users to the chat
    CanInviteUsers: bool option
    /// True, if the user is allowed to pin messages. Ignored in public supergroups
    CanPinMessages: bool option
  }
  
[<CLIMutable>]
/// This object contains information about one answer option in a poll.
type PollOption =
  {
    /// Option text, 1-100 characters
    Text: string
    /// Number of users that voted for this option
    VoterCount: int
  }

[<CLIMutable>]
/// This object contains information about a poll
type Poll =
  {
    /// Unique poll identifier
    Id: string
    /// Poll question, 1-255 characters
    Question: string
    /// List of poll options
    Options: PollOption[]
    /// True, if the poll is closed
    IsClosed: bool
  }
  
 

[<CLIMutable>]
/// This object represents a chat.
type Chat =
  {  /// Unique identifier for this chat.
    Id: int64
    /// Type of chat, can be either "private", "group", "supergroup" or "channel"
    Type: ChatType
  }

/// This object represents one special entity in a text message. For example, hashtags, usernames, URLs, etc.
and [<CLIMutable>] MessageEntity =
  {  /// Type of the entity. Can be mention (@username), hashtag, bot_command, url, email, bold (bold text), 
    /// italic (italic text), code (monowidth string), pre (monowidth block), text_link (for clickable text URLs), 
    /// text_mention (for users without usernames)    
    Type: string
    /// Offset in UTF-16 code units to the start of the entity
    Offset: int64
    /// Length of the entity in UTF-16 code units
    Length: int64
    /// For “text_link” only, url that will be opened after user taps on the text
    Url: string option
    /// For “text_mention” only, the mentioned user
    User: User option }

/// This object represents one size of a photo or a file / sticker thumbnail.
and [<CLIMutable>] PhotoSize =
  {  /// Unique identifier for this file
    FileId: string
    /// Photo width
    Width: int
    /// Photo height
    Height: int
    /// File size
    FileSize: int option }

and MaskPoint =
  | Forehead
  | Eyes
  | Mouth
  | Chin

/// This object describes the position on faces where a mask should be placed by default.
and [<CLIMutable>] MaskPosition =
  { /// The part of the face relative to which the mask should be placed. One of “forehead”, “eyes”, “mouth”, or “chin”.
    Point: MaskPoint
    /// Shift by X-axis measured in widths of the mask scaled to the face size, from left to right. For example, choosing -1.0 will place mask just to the left of the default mask position.
    [<DataMember(Name = "x_shift")>]
    XShift: float
    /// Shift by Y-axis measured in heights of the mask scaled to the face size, from top to bottom. For example, 1.0 will place the mask just below the default mask position.
    [<DataMember(Name = "y_shift")>]
    YShift: float
    /// Mask scaling coefficient. For example, 2.0 means double size.
    Scale: float
  }

/// This object represents a sticker.
and [<CLIMutable>] Sticker =
  {  /// Unique identifier for this file
    FileId: string
    /// Sticker width
    Width: int
    /// Sticker height
    Height: int
    /// True, if the sticker is animated
    IsAnimated: bool
    /// Sticker thumbnail in .webp or .jpg format
    Thumb: PhotoSize option
    /// Emoji associated with the sticker
    Emoji: string option
    /// Name of the sticker set to which the sticker belongs
    SetName: string option
    /// For mask stickers, the position where the mask should be placed
    MaskPosition: MaskPosition option
    /// File size
    FileSize: int option }


/// This object represent a user's profile pictures.
and [<CLIMutable>] UserProfilePhotos =
  { /// Total number of profile pictures the target user has
    TotalCount: int
    /// Requested profile pictures (in up to 4 sizes each)
    Photos: seq<seq<PhotoSize>> }

/// This object represents a file ready to be downloaded. The file can be downloaded via the link https://api.telegram.org/file/bot<token>/<file_path>. 
/// It is guaranteed that the link will be valid for at least 1 hour. When the link expires, a new one can be requested by calling getFile.
and [<CLIMutable>] File =
  { /// Unique identifier for this file
    FileId: string
    /// File size, if known
    FileSize: int option
    /// File path. Use https://api.telegram.org/file/bot<token>/<file_path> to get the file.
    FilePath: string option }
      
/// You can provide an animation for your game so that it looks stylish in chats (check out Lumberjack for an example). This object represents an animation file to be displayed in the message containing a game.
and [<CLIMutable>] Animation =
  {  /// Unique file identifier
    FileId: string
    /// Animation width as defined by sender
    Width: int
    /// Animation height as defined by sender
    Height: int
    /// Duration of the animation in seconds as defined by sender
    Duration: int
    /// Animation thumbnail as defined by sender
    Thumb: PhotoSize option
    /// Original animation filename as defined by sender
    FileName: string option
    /// MIME type of the file as defined by sender
    MimeType: string option
    /// File size
    FileSize: string option }

/// This object represents a message
and [<CLIMutable>] Message = 
  {  /// Unique message identifier inside this chat
    MessageId: int64
    /// Sender, can be empty for messages sent to channels
    From: User option
    /// Date the message was sent in Unix time
    Date: DateTime
    /// Conversation the message belongs to
    Chat: Chat
    /// Date the message was last edited
    EditDate: DateTime option
    /// For text messages, the actual UTF-8 text of the message, 0-4096 characters.
    Text: string option
    /// For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text
    Entities: MessageEntity seq option }


[<CLIMutable>]
/// This object represents an incoming update. At most one of the optional parameters can be present in any given update
type Update = 
  { /// The update‘s unique identifier. Update identifiers start from a certain positive number and increase sequentially. This ID becomes especially handy if you’re using Webhooks, since it allows you to ignore repeated updates or to restore the correct update sequence, should they get out of order.
    UpdateId: int64
    /// New incoming message of any kind — text, photo, sticker, etc.
    Message: Message option
    /// New version of a message that is known to the bot and was edited
    EditedMessage: Message option}