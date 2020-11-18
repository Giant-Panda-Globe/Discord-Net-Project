using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordNetTutorial.DAL.Models
{
    public class ItemModel : ModelBase
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
    }
}
