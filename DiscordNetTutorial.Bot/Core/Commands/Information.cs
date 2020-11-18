using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetTutorial.Bot.Core.Commands
{
    public class Information : ModuleBase<SocketCommandContext>
    {

        [Command("latency")]
        [Alias("ping")]
        [Summary("Displays the bot's latency in milliseconds.")]
        public async Task LatencyCommand()
        {
            var latency = Context.Client.Latency;
            var builder = new EmbedBuilder()
            {
                Title = "Latency",
                Color = latency <= 100 ? Color.Green : latency <= 200 ? Color.Orange : Color.Red,
                Description = $"My latency is **{latency} ms**"
            };
            await Context.Channel.SendMessageAsync(embed: builder.Build()); 
        }
    }
}
