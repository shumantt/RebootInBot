using System;
using System.Linq;
using RebootInBot.Types;

namespace RebootInBot.ConsoleBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using var bot = Bot.Bot.Start(new ConsoleMessenger());
            Console.WriteLine("Hello! It is bot. write command or /exit to exit.");
            var chat = new Chat(Guid.NewGuid());
            while (true)
            {
                Console.WriteLine("Input message");
                var input = Console.ReadLine();
                if (input == "/exit")
                {
                    return;
                }

                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }

                var commands = input.Split(" ").Where(s => s.StartsWith("/"));

                var message = new Message("Andrey", input, chat, Guid.NewGuid(), commands);
                bot.ProcessMessage(message);
            }
        }
    }
}