using CardGameTCPServer.TCP;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace CardGameTCPServer.Services
{
    public class DatabaseService
    {
        private static DatabaseService instance = new DatabaseService();

        public static DatabaseService Instance { get { return instance; } }

        private const string ConnectionString = "Data Source=accounts.db";

        private DatabaseService()
        {

        }

        public void Initialize()
        {
            using SqliteConnection connection =
                new SqliteConnection(ConnectionString);

            connection.Open();

            string createTableQuery =
                                        @"
                                        CREATE TABLE IF NOT EXISTS Accounts
                                        (
                                            AccountID INTEGER PRIMARY KEY,
                                            DisplayName TEXT NOT NULL,
                                            CreatedAt TEXT NOT NULL
                                        );
                                        ";

            using SqliteCommand command =
                new SqliteCommand(createTableQuery, connection);

            command.ExecuteNonQuery();

            Logger.Info("Accounts table ready");
        }

        public Dictionary<int, PlayerAccount> LoadAccounts()
        {
            Dictionary<int, PlayerAccount> accounts = new Dictionary<int, PlayerAccount>();
            using SqliteConnection connection =
                new SqliteConnection(ConnectionString);

            connection.Open();

            string selectQuery = "SELECT * FROM Accounts";

            using SqliteCommand command = new SqliteCommand(selectQuery, connection);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int accountID = reader.GetInt32(reader.GetOrdinal("AccountID"));
                string displayName = reader.GetString(reader.GetOrdinal("DisplayName"));
                string createdAt = reader.GetString(reader.GetOrdinal("CreatedAt"));

                PlayerAccount account = new PlayerAccount();
                account.AccountID = accountID;
                account.DisplayName = displayName;

                accounts.Add(accountID, account);
            }

            return accounts;
        }

        public void InsertAccount(PlayerAccount account)
        {
            using SqliteConnection connection =
               new SqliteConnection(ConnectionString);

            connection.Open();

            string insertQuery =        @"
                                        INSERT INTO Accounts
                                        (AccountID, DisplayName, CreatedAt)
                                        VALUES
                                        (@AccountID, @DisplayName, @CreatedAt);
                                        ";

            using SqliteCommand command = new SqliteCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@AccountID", account.AccountID);
            command.Parameters.AddWithValue("@DisplayName", account.DisplayName);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));

            command.ExecuteNonQuery();
        }
    }
}
