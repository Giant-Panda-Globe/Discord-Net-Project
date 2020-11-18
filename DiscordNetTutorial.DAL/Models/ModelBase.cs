using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DiscordNetTutorial.DAL.Models
{
    public abstract class ModelBase
    {
        [Key]
        public int Id { get; set; }
    }
}
