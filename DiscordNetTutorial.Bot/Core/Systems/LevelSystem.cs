using DiscordNetTutorial.DAL.Databases;
using DiscordNetTutorial.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNetTutorial.Bot.Core.Systems
{
    public class LevelSystem
    {
        private PlayerDatabase DB { get; set; }
        public LevelSystem()
        {
            DB = new PlayerDatabase();
        }
        public double LevelEquation(int level)
        {
            return Math.Floor(Math.Round(50 * Math.Pow(level, 2)));
        }
        public bool CheckIfPlayerCanLevelUp(ulong id)
        {
            if (!DB.CheckIfPlayerIsCreated(id)) DB.CreatePlayer(new PlayerModel { UserId = id });
            var player = DB.GetPlayer(id);
            if (player.Xp >= LevelEquation(player.Level)) return true;
            return false;
        }

        public void AddXp(ulong id, int xp)
        {
            if (!DB.CheckIfPlayerIsCreated(id)) DB.CreatePlayer(new PlayerModel { UserId = id });
            var player = DB.GetPlayer(id);
            player.Xp += xp;

            DB.UpdatePlayer(player);
        }

        public void LeveledUp(ulong id)
        {
            if (!DB.CheckIfPlayerIsCreated(id)) DB.CreatePlayer(new PlayerModel { UserId = id });
            var player = DB.GetPlayer(id);
            player.Xp = 0;
            player.Level += 1;
            player.Coins += player.Level * 100;

            DB.UpdatePlayer(player);
        }
    }
}
