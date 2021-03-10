using System;
using System.Collections.Generic;
using System.Linq;
using RebootInBot.Types;

namespace RebootInBot.ConsoleBot
{
    public class ConsoleMessenger : IBotMessenger
    {
        private Dictionary<Guid, ConsoleMessage> messages = new Dictionary<Guid, ConsoleMessage>();
        
        public Guid SendMessage(Chat chat, IEnumerable<string> mentions, string text)
        {
            var prefix = BuildPrefix(chat);
            var mentionsList = BuildMentionsList(mentions);
            var messageText = $"{prefix}: {text} {mentionsList}";
            var messageId = Guid.NewGuid();
            var message = new ConsoleMessage(System.Console.CursorLeft, System.Console.CursorTop, messageText);
            messages.Add(messageId, message);
            Console.WriteLine(messageText);
            return messageId;
        }

        public IEnumerable<string> GetParticipants(Chat chat)
        {
            return new[]
            {
                "Andrey",
                "Ivan",
                "Other"
            };
        }

        public void UpdateMessage(Chat chat, Guid messageId, string newText)
        {
            var newMessageText = $"{BuildPrefix(chat)}: {newText}";
            var message = messages[messageId];

            Console.SetCursorPosition(message.Left, message.Top);
            Console.WriteLine(newMessageText.PadRight(message.Text.Length, ' '));

            messages[messageId] = message.Rewrite(newMessageText);
        }

        private static string BuildPrefix(Chat chat) => chat.ChatId.ToString().Substring(0, 3);
        private static string BuildMentionsList(IEnumerable<string> mentions) => string.Join(',',mentions.Select(x => $"@{x}").ToArray());
    }
}