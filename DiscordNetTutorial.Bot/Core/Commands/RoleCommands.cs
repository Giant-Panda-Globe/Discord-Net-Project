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
    // Prefix is +
    public class RoleCommands : InteractiveBase<SocketCommandContext>
    {
        ILogger _Logger { get; set; }

        public RoleCommands(IServiceProvider Services)
        {
            _Logger = Services.GetRequiredService<ILogger>();
        }

        [Command("addrole")]
        [Summary("Adds a role to a user.")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "", Group = "Permission")] // Needs to have Manage Roles so it can add the role to the suer.
        [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "", Group = "Permission")] // Needs to have Manage Roles as a permission check.
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "", Group = "Permission")] // Or needs to have Administrator as a permission check.
        [RequireOwner(Group = "Permission")] // Or Is the owner of the bot.
        public async Task AddRoleToUserAsync(string user, [Remainder] string role) // +addrole <user mention/user name/user id> <role id/role name>
        {
            string username = ""; // The user's name
            ulong userid = 0; // The user's id

            username = user; // Pass the user param into username.
            if (ulong.TryParse(username, out ulong result)) // If parse passes, then pass out result.
                userid = result; // pass result into userid.
            // The user can't be a bot. Bots apparently are not consider not a user, so it will always be null if the user mentions a bot.
            IGuildUser _user; // The user object.

            if (Context.Message.MentionedUsers.Count != 0) // Checking if the message doesn't mention a user, if so, then we are going to use that to find the user.
                _user = Context.Guild.GetUser(Context.Message.MentionedUsers.FirstOrDefault().Id);
            else if (userid != 0) // if id is not equal to 0, then we are going search for the user by their id.
                _user = Context.Guild.GetUser(userid);
            else // else we are going to search for the user by their username.
                _user = Context.Guild.Users.Where(x => x.Username.ToLower().Equals(username.ToLower())).FirstOrDefault();

            if(_user is null) // if user object is type of null.
            {
                await ReplyAndDeleteAsync("I couldn't find that user", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            string rolename = ""; // the role name.
            ulong roleid = 0; // The role id.

            rolename = role; // pass the role name into name.
            if (ulong.TryParse(rolename, out ulong _result)) // we are going to try and parse it, and pass it into a variable called result.
                roleid = _result; // pass the results into id.


            SocketRole _role; // the role.
            if (roleid != 0) // if the role id is not 0, then we are going to use the id to find the role.
                _role = Context.Guild.GetRole(roleid);
            else // else we are going to search for the role by it's name.
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == rolename.ToLower()).FirstOrDefault();

            if(_role is null) // if role is the type of null.
            {
                await ReplyAndDeleteAsync("I couldn't find that role", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            await _user.AddRoleAsync(_role); // add the role to the user.
            await ReplyAndDeleteAsync($"Added role {_role.Name} to user {_user.Username}");
        }

        [Command("createrole")]
        [Summary("Adds a new role to the Server.")]
        [RequireUserPermission(Discord.ChannelPermission.ManageRoles, Group = "Permission")] // Needs to have Manage roles as a permission check.
        [RequireUserPermission(Discord.GuildPermission.Administrator, Group = "Permission")] // Or Administaror
        [RequireOwner(Group = "Permission")] // Or is Owner.
        [RequireBotPermission(Discord.ChannelPermission.ManageRoles, Group = "Permission")] // Needs to have Manage Roles to create the role.
        [RequireBotPermission(GuildPermission.Administrator, Group = "Permission")] // Or Administrator.
        public async Task CreateRoleAsync() // +createrole
        {
            var selfMessage = new List<RestUserMessage>(); // This is going to hold the bots sent messages, so we can delete them afterwards.
            var userMessage = new List<SocketMessage>(); // This is going to hold the users sent messages, so we can delete them afterwards.
            var msg = await Context.Channel.SendMessageAsync("What would you like to name the role?\nTo cancel type in `<cancel>`");
            var input = await NextMessageAsync(); // this is using Interactivity. We are going to use this, so we can wait for the user to reply back to us.
            selfMessage.Add(msg); // add the bot's sent message.
            userMessage.Add(input); // add the user's sent message.


            if (input.Content.ToLower().Equals("<cancel>")) // <cancel>
            {
                // We need to make sure we have the permission of Manage Messages, and if we don't then we will not delete them, else we will.
                // This is needed to prevent any kind of Permission errors we might get.
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            string name = input.Content; // The name they want the role to be.
            // Checks if the role name already exist, if so then we are not going to continue, else its safe to do so.
            if (Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault() is SocketRole)
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Sorry, but a role with that name already exist. Exitting out of the process now.", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            // Next we are going see if they want to add color to the role or not.
            msg = await Context.Channel.SendMessageAsync("Would you like to color the role?(y/n)\nTo cancel type in `<cancel>`");
            input = await NextMessageAsync();
            selfMessage.Add(msg);
            userMessage.Add(input);
            string color = ""; // the color they wish to use for the role.
            if (input.Content.ToLower().Equals("<cancel>")) // <cancel>
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessage)
                        await message.DeleteAsync();
                foreach (var message in selfMessage)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            if (input.Content.ToLower().Equals("yes") || input.Content.ToLower().Equals("y")) // yes, y
            {
                // What color would they like to use.
                msg = await Context.Channel.SendMessageAsync("What color would you like the role to have?\nTo Cancel type in `<cancel>`");
                input = await NextMessageAsync();
                selfMessage.Add(msg);
                userMessage.Add(input);

                if (input.Content.ToLower().Equals("<cancel>")) // <cancel>
                {
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessage)
                            await message.DeleteAsync();
                    foreach (var message in selfMessage)
                        await message.DeleteAsync();
                    await ReplyAndDeleteAsync("Cancelled", timeout: TimeSpan.FromSeconds(30));
                    return;
                } // else by default.

                color = input.Content; // Pass the selection into the variable called color.
            }

            msg = await Context.Channel.SendMessageAsync("Would you like the role to be mentionable?(y/n)\nTo cancel type in `<cancel>`.");
            input = await NextMessageAsync();

            selfMessage.Add(msg);
            userMessage.Add(input);
            bool isMentionable = false;
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

            if (input.Content.ToLower().Equals("yes") || input.Content.ToLower().Equals("y"))
            {
                isMentionable = true;
            }

            // if color is string or is null, then we are going to create role with no color.
            if (string.IsNullOrWhiteSpace(color))
                await Context.Guild.CreateRoleAsync(name, null, null, false, null);
            // else we are going to create role with color.
            else
            {
                // This is to convert the String into a rgb byte array that Discord.Colors can read.
                var htmlColor = ColorTranslator.FromHtml(color);
                Discord.Color _color = new Discord.Color(htmlColor.R, htmlColor.G, htmlColor.B);
                await Context.Guild.CreateRoleAsync(name, null, _color, false, null);
            }

            if (isMentionable)
            {
                var role = Context.Guild.Roles.Where(x => x.Name.ToLower().Equals(name)).FirstOrDefault();
                if (role is null)
                    await ReplyAndDeleteAsync("Something happened, and i wasn't able to make the role mentionable at this time. Please try using my `editrole` command to make it mentionable if you wish.", timeout: TimeSpan.FromSeconds(30));
                else
                    await role.ModifyAsync(x => x.Mentionable = true);
            }
            // Confirmation Message.
            await ReplyAndDeleteAsync($"Role named: {name} was created.", timeout: TimeSpan.FromSeconds(30));
            if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                foreach (var message in userMessage)
                    await message.DeleteAsync();
            foreach (var message in selfMessage)
                await message.DeleteAsync();

        }

        [Command("deleterole")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage ="", Group = "Permission")]
        [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "", Group = "Permission")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "", Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task DeleteRoleAsync([Remainder] string role)
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
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower().Equals(name.ToLower())).FirstOrDefault();

            if(_role is null)
            {
                await ReplyAndDeleteAsync("Couldn't find that role.", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            await ReplyAndDeleteAsync($"Deleted role {_role.Name}!", timeout: TimeSpan.FromSeconds(30));
            await _role.DeleteAsync();
        }

        [Command("editrole")]
        [RequireBotPermission(GuildPermission.ManageRoles, ErrorMessage = "I Don't have permission to do this.", Group = "Permissions")]
        [RequireUserPermission(GuildPermission.ManageRoles, ErrorMessage = "You don't have permission to do this.", Group = "Permission")]
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task EditRoleAsync([Remainder] string role) // +editrole <role id/role name>
        {
            var selfMessages = new List<RestUserMessage>();
            var userMessages = new List<SocketMessage>();
            string name = ""; // the role name.
            ulong id = 0; // The role id.

            name = role; // pass the role name into name.
            if (ulong.TryParse(name, out ulong result)) // we are going to try and parse it, and pass it into a variable called result.
                id = result; // pass the results into id.
            SocketRole _role; // The role variable.
            if (id != 0) // If id is not equal to 0, then that means there is an id present.
                _role = Context.Guild.GetRole(id); // search for the role by id.
            else // else we are going to search for it by name.
                // This is using System.Linq, to use the Where() method.
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();

            if(_role is null) // If role is null, then...
            {
                await ReplyAndDeleteAsync("Couldn't find that role.", timeout: TimeSpan.FromSeconds(30));
                return;
            }
            // Ask what property do they want to edit.
            var msg = await Context.Channel.SendMessageAsync("What would you like to change?(name, color)\nTo cancel, type in `<cancel>`");
            var input = await NextMessageAsync();
            selfMessages.Add(msg);
            userMessages.Add(input);
            if (input.Content.ToLower().Equals("<cancel>")) // <cancel>
            {
                if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                    foreach (var message in userMessages)
                        await message.DeleteAsync();
                foreach (var message in selfMessages)
                    await message.DeleteAsync();
                await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
            }

            switch (input.Content.ToLower()) // switch case the selection.
            {
                case "name": // If the selection is name.
                    msg = await Context.Channel.SendMessageAsync("What would you like to name it instead?\nTo cancel type in `<cancel>`");
                    input = await NextMessageAsync();

                    if(input.Content.ToLower().Equals("<cancel>")) // <cancel>
                    {
                        if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                            foreach (var message in userMessages)
                                await message.DeleteAsync();
                        foreach (var message in selfMessages)
                            await message.DeleteAsync();
                        await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
                        return;
                    }

                    await _role.ModifyAsync(x => x.Name = input.Content); // We need to modify it to edit it.
                    await ReplyAndDeleteAsync($"Updated the role's name to {input.Content}");
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessages)
                            await message.DeleteAsync();
                    foreach (var message in selfMessages)
                        await message.DeleteAsync();
                    break; // We are breaking out of the switch case.
                case "color": // if selection is color
                    msg = await Context.Channel.SendMessageAsync("What color would you like to have on the role instead?");
                    input = await NextMessageAsync();
                    if (input.Content.ToLower().Equals("<cancel>")) // <cancel>
                    {
                        if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                            foreach (var message in userMessages)
                                await message.DeleteAsync();
                        foreach (var message in selfMessages)
                            await message.DeleteAsync();
                        await ReplyAndDeleteAsync("Cancelling", timeout: TimeSpan.FromSeconds(30));
                        return;
                    }
                    // Converting string to readable RGB byte array for Discord.Color to read.
                    var htmlColor = ColorTranslator.FromHtml(input.Content);
                    Discord.Color _color = new Discord.Color(htmlColor.R, htmlColor.G, htmlColor.B);
                    await _role.ModifyAsync(x => x.Color = _color);
                    if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                        foreach (var message in userMessages)
                            await message.DeleteAsync();
                    foreach (var message in selfMessages)
                        await message.DeleteAsync();
                    break; // Break out of the Switch case.
                case "mention":
                    msg = await Context.Channel.SendMessageAsync($"The role is {(!_role.IsMentionable ? "mentionable" : "not mentionable")}\n Would you like to {(_role.IsMentionable ? "make this role be mentionable" : "make this role not be mentionable")}?(y/n)\nTo cancel type in `<cancel>`");
                    input = await NextMessageAsync();
                    if (input.Content.ToLower().Equals("<cancel>"))
                    {

                    }
                    if(input.Content.ToLower().Equals("yes") || input.Content.ToLower().Equals("y"))
                    {
                        if (Context.Guild.CurrentUser.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
                            foreach (var message in userMessages)
                                await message.DeleteAsync();
                        foreach (var message in selfMessages)
                            await message.DeleteAsync();
                        await _role.ModifyAsync(x => x.Mentionable = !_role.IsMentionable);
                        await ReplyAndDeleteAsync($"The role is now {(_role.IsMentionable ? "not mentionable" : "mentionable")}.", timeout: TimeSpan.FromSeconds(30));
                    }
                    break;
            }
        }

        [Command("getrole")]
        [Summary("Gets info about a role.")]
        public async Task GetRoleAsync([Remainder] string role) // +getrole <role id/ role name>
        {
            string name = ""; // role name
            ulong id = 0; // role id

            name = role; // Pass the role into role name.
            if (ulong.TryParse(name, out ulong result)) // Try and parse name into a ulong, and pass out result.
                id = result; // pass result into id.

            SocketRole _role; // The role object.
            if (id != 0) // If id is not 0, then id is present.
                _role = Context.Guild.GetRole(id); // Get role by id.
            else // else
                // Get role by name. this is using System.Linq, for the Where method.
                _role = Context.Guild.Roles.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if(_role is null) // if role is null
            {
                await ReplyAndDeleteAsync("Couldn't find that role", timeout: TimeSpan.FromSeconds(30));
                return;
            }

            var builder = new EmbedBuilder // make an embed builder.
            {
                Title = $"Role: {_role.Name}", // Embed Title.
                Description = $"Mention:{_role.Mention}\nID: {_role.Id}\nMentionable: {_role.IsMentionable}", // Embed Description
                Color = _role.Color // Embed color, using the role's color.
            };

            await Context.Channel.SendMessageAsync(embed: builder.Build()); // Send the embed, but we need to build it first.
        }
    }
}
