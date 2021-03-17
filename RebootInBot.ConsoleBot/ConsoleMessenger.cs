using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RebootInBot.Types;

namespace RebootInBot.ConsoleBot
{
    public class ConsoleMessenger : IBotMessenger
    {
        private readonly Dictionary<Guid, ConsoleMessage> messages = new Dictionary<Guid, ConsoleMessage>();

        public Task<Guid> SendMessage(Chat chat, IEnumerable<string> mentions, string text)
        {
            var prefix = BuildPrefix(chat);
            var mentionsList = BuildMentionsList(mentions);
            var messageText = $"{prefix}: {text} {mentionsList}";
            var messageId = Guid.NewGuid();
            var message = new ConsoleMessage(System.Console.CursorLeft, System.Console.CursorTop, messageText);
            messages.Add(messageId, message);
            Console.WriteLine(messageText);
            return Task.FromResult(messageId);
        }

        public Task<IEnumerable<string>> GetParticipants(Chat chat)
        {
            return Task.FromResult(new[]
            {
                "Andrey",
                "Ivan",
                "Other"
            }.AsEnumerable());
        }

        public Task UpdateMessage(Chat chat, Guid messageId, string newText)
        {
            var newMessageText = $"{BuildPrefix(chat)}: {newText}";
            var message = messages[messageId];

            Console.SetCursorPosition(message.Left, message.Top);
            Console.WriteLine(newMessageText.PadRight(message.Text.Length, ' '));

            messages[messageId] = message.Rewrite(newMessageText);
            return Task.CompletedTask;
        }

        private static string BuildPrefix(Chat chat) => chat.ChatId.ToString().Substring(0, 3);

        private static string BuildMentionsList(IEnumerable<string> mentions) =>
            string.Join(',', mentions.Select(x => $"@{x}").ToArray());
    }
}