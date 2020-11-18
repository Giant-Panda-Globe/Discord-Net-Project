using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Rest;
using Discord.WebSocket;
using Discord;
using System.Drawing;
using System.Linq;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordNetTutorial.Bot.Core.Commands
{
    public class RoleCommands : InteractiveBase<SocketCommandContext>
    {
        ILogger _Logger { get; set; }

        public RoleCommands(IServiceProvider Services)
        {
            _Logger = Services.GetRequiredService<ILogger>();
        }

        [Command("addrole")]
        [Summary("Adds a role to a user.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "", Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "", Group = "Permission")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "", Group = "Permission")]
        public async Task AddRoleToUserAsync(string user, [Remainder] string role)
        {
            string username = "";
            ulong userid = 0;
            string rolename = "";
            ulong roleid = 0;

            username = user;
            rolename = role;

            if (ulong.TryParse(username, out ulong result))
                userid = result;
            if (ulong.TryParse(rolename, out ulong _result))
                roleid = _result;
            SocketGuildUser _user;
            if (Context.Message.MentionedUsers.Count != 0)
                _user = Context.Guild.GetUser(Context.Message.MentionedUsers.FirstOrDefault().Id);
            else if (userid != 0)
                _user = Context.Guild.GetUser(userid);
            else
                _user = Context.Guild.Users.Where(x => x.Username.ToLower() == username.ToLower()).FirstOrDefault();
            _Logger.Debug(username);
            if(_user is null)
            {
                await ReplyAndDeleteAsync("I couldn't find that user", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            SocketRole _role;
            if (roleid != 0)
                _role = Context.Guild.GetRole(roleid);
            else
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == rolename.ToLower()).FirstOrDefault();

            if(_role is null)
            {
                await ReplyAndDeleteAsync("I couldn't find that role", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            await _user.AddRoleAsync(_role);
            await ReplyAndDeleteAsync($"Added role {_role.Name} to user {_user.Username}");
        }

        [Command("createrole")]
        [Summary("Adds a new role to the Server.")]
        [RequireUserPermission(Discord.ChannelPermission.ManageRoles, Group = "Permission")]
        [RequireUserPermission(Discord.GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [RequireBotPermission(Discord.ChannelPermission.ManageRoles, Group = "Permission")]
        [RequireBotPermission(GuildPermission.Administrator, Group = "Permission")]
        public async Task CreateRoleAsync()
        {
            var selfMessage = new List<RestUserMessage>();
            var userMessage = new List<SocketMessage>();
            var msg = await Context.Channel.SendMessageAsync("What would you like to name the role?\nTo cancel type in `<cancel>`");
            var input = await NextMessageAsync();
            selfMessage.Add(msg);
            userMessage.Add(input);


            if (input.Content.ToLower().Equals("<cancel>"))
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            string name = input.Content;
            if(Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault() is SocketRole)
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Sorry, but a role with that name already exist. Exitting out of the process now.", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            msg = await Context.Channel.SendMessageAsync("Would you like to color the role?(y/n)\nTo cancel type in `<cancel>`");
            input = await NextMessageAsync();
            selfMessage.Add(msg);
            userMessage.Add(input);
            string color = "";
            if(input.Content.ToLower().Equals("<cancel>"))
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            if(input.Content.ToLower().Equals("yes") || input.Content.ToLower().Equals("y"))
            {
                msg = await Context.Channel.SendMessageAsync("What color would you like the role to have?\nTo Cancel type in `<cancel>`");
                input = await NextMessageAsync();
                selfMessage.Add(msg);
                userMessage.Add(input);

                if(input.Content.ToLower().Equals("<cancel>"))
                {
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessage)
                            await message.DeleteAsync();
                    foreach (var message in selfMessage)
                        await message.DeleteAsync();
                    await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                    return;
                } // else by default.

                color = input.Content;
            }
            if(string.IsNullOrWhiteSpace(color))
                await Context.Guild.CreateRoleAsync(name, null, null, false, null);
            else
            {
                var htmlColor = ColorTranslator.FromHtml(color);
                Discord.Color _color = new Discord.Color(htmlColor.R, htmlColor.G, htmlColor.B);
                await Context.Guild.CreateRoleAsync(name, null, _color, false, null);
            }

            await ReplyAndDeleteAsync($"Role named: {name} was created.", timeout: TimeSpan.FromSeconds(30));
            if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                foreach (var message in userMessage)
                    await message.DeleteAsync();
            foreach (var message in selfMessage)
                await message.DeleteAsync();

        }

        [Command("editrole")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "I Don't have permission to do this.", Group = "Permissions")]
        [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "You don't have permission to do this.", Group = "Permission")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task EditRoleAsync([Remainder] string role)
        {
            var selfMessages = new List<RestUserMessage>();
            var userMessages = new List<SocketMessage>();
            string name = "";
            ulong id = 0;

            name = role;
            if (ulong.TryParse(name, out ulong result))
                id = result;
            SocketRole _role;
            if (id != 0)
                _role = Context.Guild.GetRole(id);
            else
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

            if(_role is null)
            {
                await ReplyAndDeleteAsync("Couldn't find that role.", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            var msg = await Context.Channel.SendMessageAsync("What would you like to change?(name, color)\nTo cancel, type in `<cancel>`");
            var input = await NextMessageAsync();
            selfMessages.Add(msg);
            userMessages.Add(input);
            if (input.Content.ToLower().Equals("<cancel>"))
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessages)
                        await message.DeleteAsync();
                foreach (var message in selfMessages)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
            }

            switch (input.Content.ToLower())
            {
                case "name":
                    msg = await Context.Channel.SendMessageAsync("What would you like to name it instead?\nTo cancel type in `<cancel>`");
                    input = await NextMessageAsync();

                    if(input.Content.ToLower().Equals("<cancel>"))
                    {
                        if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                            foreach (var message in userMessages)
                                await message.DeleteAsync();
                        foreach (var message in selfMessages)
                            await message.DeleteAsync();
                        await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
                        return;
                    }

                    await _role.ModifyAsync(x => x.Name = input.Content);
                    await ReplyAndDeleteAsync($"Updated the role's name to {input.Content}");
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessages)
                            await message.DeleteAsync();
                    foreach (var message in selfMessages)
                        await message.DeleteAsync();
                    break;
                case "color":
                    msg = await Context.Channel.SendMessageAsync("What color would you like to have on the role instead?");
                    input = await NextMessageAsync();
                    if (input.Content.ToLower().Equals("<cancel>"))
                    {
                        if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                            foreach (var message in userMessages)
                                await message.DeleteAsync();
                        foreach (var message in selfMessages)
                            await message.DeleteAsync();
                        await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
                        return;
                    }

                    var htmlColor = ColorTranslator.FromHtml(input.Content);
                    Discord.Color _color = new Discord.Color(htmlColor.R, htmlColor.G, htmlColor.B);
                    await _role.ModifyAsync(x => x.Color = _color);
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessages)
                            await message.DeleteAsync();
                    foreach (var message in selfMessages)
                        await message.DeleteAsync();
                    break;
            }
        }

        [Command("getrole")]
        [Summary("Gets info about a role.")]
        public async Task GetRoleAsync([Remainder] string role)
        {
            string name = "";
            ulong id = 0;

            name = role;
            if (ulong.TryParse(name, out ulong result))
                id = result;

            SocketRole _role;
            if (id != 0)
                _role = Context.Guild.GetRole(id);
            else
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if(_role is null)
            {
                await ReplyAndDeleteAsync("Couldn't find that role", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            var builder = new EmbedBuilder
            {
                Title = $"Role: {_role.Name}",
                Description = $"Mention:{_role.Mention}\nID: {_role.Id}\nMentionable: {_role.IsMentionable}",
                Color = _role.Color
            };

            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
