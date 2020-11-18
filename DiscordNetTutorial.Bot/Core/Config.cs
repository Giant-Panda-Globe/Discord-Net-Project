using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DiscordNetTutorial.Bot.Core
{
    class Config
    {
        private static string configFile = "config.json"; // Config File name.
        private static string configFolder = "Resources"; // Config Folder.
        private static string configPath = configFolder + "/" + configFile; // Config Path.

        public static BotConfig Bot; // Bot config.
        /// <summary>
        /// Gets our config for us.
        /// Its static, because we don't want to create a new instance of the class, we just need to have the config.
        /// </summary>
        static Config()
        {
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            if(!File.Exists(configPath))
            {
                Bot = new BotConfig();
                var json = JsonConvert.SerializeObject(Bot, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            else
            {
                var json = File.ReadAllText(configPath);
                Bot = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }

    public struct BotConfig // This is the Bot Config Struct. This will structure our config.
    {
        [JsonProperty("token")] // We want to get the property called "token" and pass it into Token.
        public string Token { get; set; }
        [JsonProperty("prefix")] // We want to get the property called "prefix" and pass it into Prefix.
        public string Prefix { get; set; }
    }
}
