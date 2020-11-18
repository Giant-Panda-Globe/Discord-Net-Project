using DiscordNetTutorial.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace DiscordNetTutorial.DAL.Databases
{
    public class PlayerDatabase
    {
        SqliteDbConnection Conn = new SqliteDbConnection();
        public PlayerDatabase()
        {
            CreateTableIfNotExist();
        }
        public void CreateTableIfNotExist()
        {
            string query = "CREATE TABLE IF NOT EXISTS 'players'(id varchar(22), coins int, xp int, level int)";
            SQLiteCommand cmd = new SQLiteCommand(query, Conn.MyConnection);
            Conn.OpenConnection();

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            cmd.Dispose();
            Conn.CloseConnection();
        }
        public bool CheckIfPlayerIsCreated(ulong id)
        {
            string query = $"SELECT * FROM 'players' WHERE id = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, Conn.MyConnection);
            Conn.OpenConnection();

            cmd.Prepare();
            var reader = cmd.ExecuteReader();
            bool doesExist = false;
            if (reader.HasRows) doesExist = true;
            reader.Close();
            reader.Dispose();

            cmd.Dispose();

            Conn.CloseConnection();

            return doesExist;
        }

        public PlayerModel GetPlayer(ulong id)
        {
            string query = $"SELECT * FROM 'players' WHERE id = {id}";
            SQLiteCommand cmd = new SQLiteCommand(query, Conn.MyConnection);
            Conn.OpenConnection();
            cmd.Prepare();

            var reader = cmd.ExecuteReader();
            var player = new PlayerModel();
            if(reader.HasRows) while(reader.Read())
                {
                    player.UserId = id;
                    player.Coins = Convert.ToInt32(reader["coins"]);
                    player.Xp = Convert.ToInt32(reader["xp"]);
                    player.Level = Convert.ToInt32(reader["level"]);
                }

            cmd.Dispose();
            Conn.CloseConnection();
            return player;
        }

        public void UpdatePlayer(PlayerModel player)
        {
            string query = $"UPDATE 'players' SET coins = $coins, xp = $xp, level = $level WHERE id = {player.UserId}";
            SQLiteCommand cmd = new SQLiteCommand(query, Conn.MyConnection);
            Conn.OpenConnection();

            cmd.Prepare();
            cmd.Parameters.AddWithValue("$coins", player.Coins);
            cmd.Parameters.AddWithValue("$xp", player.Xp);
            cmd.Parameters.AddWithValue("$level", player.Level);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            Conn.CloseConnection();
        }

        public void CreatePlayer(PlayerModel player)
        {
            string query = "INSERT INTO 'players'(id, coins, xp, level) VALUES($id, $coins, $xp, $level)";
            SQLiteCommand cmd = new SQLiteCommand(query, Conn.MyConnection);
            Conn.OpenConnection();
            
            cmd.Prepare();
            
            cmd.Parameters.AddWithValue("$id", player.UserId);
            cmd.Parameters.AddWithValue("$coins", player.Coins);
            cmd.Parameters.AddWithValue("$xp", player.Xp);
            cmd.Parameters.AddWithValue("$level", player.Level);

            cmd.ExecuteNonQuery();

            cmd.Dispose();
            Conn.CloseConnection();
        }
    }
}
