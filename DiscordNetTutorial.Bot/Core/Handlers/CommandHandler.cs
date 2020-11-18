using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Reflection;
using Serilog;

namespace DiscordNetTutorial.Bot.Core.Handlers
{
    public class CommandHandler
    {
        private CommandService Commands { get; set; }
        private ILogger Logger { get; set; }
        private IServiceProvider _Services { get; set; }

        public CommandHandler(IServiceProvider Services)
        {
            Commands = Services.GetRequiredService<CommandService>();
            Logger = Services.GetRequiredService<ILogger>();
            _Services = Services;
        }

        public async Task InitAsync()
        {
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _Services);
            foreach (var commands in Commands.Commands)
                Logger.Information($"{commands.Name} was loaded.");
        }
    }
}
