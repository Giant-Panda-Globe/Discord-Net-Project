using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace DiscordNetTutorial.DAL
{
    public class SqliteDbConnection
    {
        public SQLiteConnection MyConnection { get; private set; }
        public SqliteDbConnection()
        {
            MyConnection = new SQLiteConnection(@"Data Source=Resources\Database.sqlite3");
            if (!File.Exists("Resources/Database.sqlite3"))
                SQLiteConnection.CreateFile("Resources/Database.sqlite3");
        }

        public void OpenConnection()
        {
            if (MyConnection.State == System.Data.ConnectionState.Open) return;
            MyConnection.Open();
        }

        public void CloseConnection()
        {
            if (MyConnection.State == System.Data.ConnectionState.Closed) return;
            MyConnection.Close();
        }
    }
}
