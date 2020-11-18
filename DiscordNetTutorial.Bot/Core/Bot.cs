using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DiscordNetTutorial.Bot.Core.Handlers;
using Discord.Addons.Interactive;
using Serilog;

namespace DiscordNetTutorial.Bot.Core
{
    public class Bot
    {
        private DiscordSocketClient Client { get; set; } // This is the client instance.
        private CommandService Commands { get; set; } // This is the command services instance.
        // This is the IServiceProvider instance which will hold our client instance, and command instance, 
        // well as our other services.
        private IServiceProvider Services { get; set; }

        private InteractiveService Interactivity { get; set; }
        public Bot()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug // We just want the neccessary information, not everything.
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = true, // this is to make commands to case sensitive, if you don't like this, then you can delete this line.
                DefaultRunMode = RunMode.Async, // We want to make sure our commands are running in asynchronous.
                LogLevel = LogSeverity.Debug // We want the neccessary information.
            });
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Debug)
            .WriteTo.Console(outputTemplate:
            "{Timestamp:HH:mm:ss} - [{Level:u4}] => {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
            Interactivity = new InteractiveService(Client, new InteractiveServiceConfig { DefaultTimeout = TimeSpan.FromMinutes(2) });

            Services = BuildServiceProvider();
        }

        public async Task MainAsync()
        {
            if (string.IsNullOrWhiteSpace(Config.Bot.Token)) return;
            await new CommandHandler(Services).InitAsync();
            await new Handlers.EventHandler(Services, Log.Logger).InitAsync();
            await Client.LoginAsync(TokenType.Bot, Config.Bot.Token);
            await Client.StartAsync();
            await Task.Delay(-1); // We need to have this here. Without it, the program will start, then stop. this is to prevent it from stopping.
        }

        /// <summary>
        /// Builds the Service Provider for us.
        /// </summary>
        /// <returns>Service Provider</returns>
        private ServiceProvider BuildServiceProvider()
            => new ServiceCollection()
            .AddSingleton(Client) // Pass our client instance in.
            .AddSingleton(Commands) // Pass our commands instance in.
            .AddSingleton(Log.Logger)
            .AddSingleton(Interactivity)
            .BuildServiceProvider(); // then we are going to build the provider.
    }
}
