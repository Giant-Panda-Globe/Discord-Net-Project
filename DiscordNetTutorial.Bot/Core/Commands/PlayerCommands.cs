using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordNetTutorial.DAL.Databases;
using DiscordNetTutorial.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordNetTutorial.Bot.Core.Commands
{
    public class PlayerCommands : InteractiveBase<SocketCommandContext>
    {
        private PlayerDatabase db = new PlayerDatabase();
        [Command("profile")]
        [Summary("Displays your profile for th bot.")]
        public async Task GetProfile(ulong id = 0)
        {
            if (id.Equals(0))
                id = Context.User.Id;
            if (!db.CheckIfPlayerIsCreated(id)) db.CreatePlayer(new PlayerModel { UserId = id });
            var player = db.GetPlayer(id);
            var user = Context.Guild.GetUser(id);
            var builder = new EmbedBuilder
            {
                Title = $"{user.Username}'s Profile",
                Description = $"Level: {player.Level}\nXp: {player.Xp}\nCoins: {player.Coins}",
                Color = Color.LightOrange
            };

            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
