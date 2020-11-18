using System;

namespace DiscordNetTutorial.Bot
{
    class Program
    {
        static void Main(string[] args)
            => new Core.Bot().MainAsync().GetAwaiter().GetResult();
    }
}
