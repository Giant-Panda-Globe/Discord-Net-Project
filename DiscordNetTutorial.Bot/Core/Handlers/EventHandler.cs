using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using Serilog.Core;
using Serilog;

namespace DiscordNetTutorial.Bot.Core.Handlers
{
    public class EventHandler
    {
        private DiscordSocketClient Client { get; set; }
        private CommandService Commands { get; set; }
        private IServiceProvider _Services { get; set; }
        private ILogger _Logger { get; set; }

        public EventHandler(IServiceProvider Services, ILogger Logger)
        {
            Client = Services.GetRequiredService<DiscordSocketClient>();
            Commands = Services.GetRequiredService<CommandService>();
            _Logger = Logger;
            _Services = Services;
        }

        public Task InitAsync()
        {
            Client.Ready += Ready_Event;
            Client.Log += Client_Log;
            Commands.Log += Commands_Log;
            Client.MessageReceived += Message_Event;
            return Task.CompletedTask;
        }
        /// <summary>
        /// This is the Message Event Subscriber.
        /// </summary>
        /// <param name="MessageParam"></param>
        /// <returns></returns>
        private async Task Message_Event(SocketMessage MessageParam)
        {
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);
            if (Message.Author.IsBot || Message.Channel is SocketDMChannel) return; // Checks if the user is a bot or if the channel is a dm channel. if so then return;

            int ArgPos = 0; // The argument position.
            // Checks if the message contains our prefix or mentions the bot. if not, then return.
            if (!(Message.HasStringPrefix(Config.Bot.Prefix, ref ArgPos) || Message.HasMentionPrefix(Client.CurrentUser, ref ArgPos))) return;
            // Execute the Command.
            var result = await Commands.ExecuteAsync(Context, ArgPos, _Services);
            // If the Result of the command is not successful, then we are going to say that it didn't work, and why.
            if(!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand) return;
                else if(result.Error == CommandError.BadArgCount)
                {
                    await Context.Channel.SendMessageAsync("Sorry, but this command needs a certain amount of arguments. Please use the help command to find out what this command needs.");
                    return;
                }
            }
        }
        /// <summary>
        /// This is the command logs subscriber.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private Task Commands_Log(Discord.LogMessage log)
        {
            
            _Logger.Information($"{log.Message}");
            return Task.CompletedTask;
        }
        /// <summary>
        /// This is the client log subscriber.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private Task Client_Log(Discord.LogMessage log)
        {
            _Logger.Information($"{log.Message}");
            return Task.CompletedTask;
        }
        /// <summary>
        /// This is the ready event subscriber.
        /// </summary>
        /// <returns></returns>
        private async Task Ready_Event()
        {
            _Logger.Information($"{Client.CurrentUser.Username} is ready");
            await Client.SetStatusAsync(Discord.UserStatus.Online);
            await Client.SetGameAsync($"{Config.Bot.Prefix}help | In {Client.Guilds.Count} Servers");
        }
    }
}
