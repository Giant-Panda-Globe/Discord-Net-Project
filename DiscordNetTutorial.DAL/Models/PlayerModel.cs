using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNetTutorial.DAL.Models
{
    public class PlayerModel : ModelBase
    {
        public ulong UserId { get; set; }
        public int Coins { get; set; } = 200;
        public int Xp { get; set; } = 0;
        public int Level { get; set; } = 1;
    }
}
