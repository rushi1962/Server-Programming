using CardGameTCPServer.Data;
using CardGameTCPServer.TCP;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Security.Principal;

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
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            #region Accounts
            string createTableQuery =
                                        @"
                                        CREATE TABLE IF NOT EXISTS Accounts
                                        (
                                            AccountID INTEGER PRIMARY KEY,
                                            DisplayName TEXT NOT NULL,
                                            CreatedAt TEXT NOT NULL
                                        );
                                        ";

            using SqliteCommand createAccountsTableCommand = new SqliteCommand(createTableQuery, connection);
            createAccountsTableCommand.ExecuteNonQuery();

            Logger.Info("Accounts table ready");
            #endregion

            #region MatchList
            createTableQuery =
                                        @"
                                        CREATE TABLE MatchResults
                                        (
                                            MatchID INTEGER PRIMARY KEY,
                                            Player1ID INTEGER NOT NULL,
                                            Player2ID INTEGER NOT NULL,
                                            WinnerPlayerID INTEGER,
                                            FinishedAt TEXT NOT NULL
                                        );
                                        ";

            using SqliteCommand createMatchResultTableCommand = new SqliteCommand(createTableQuery, connection);
            createMatchResultTableCommand.ExecuteNonQuery();

            Logger.Info("Match result table ready");
            #endregion

            #region Player statistics
            createTableQuery =
                                        @"
                                        CREATE TABLE PlayerStats
                                        (
                                            PlayerID INTEGER PRIMARY KEY,
                                            Wins INTEGER NOT NULL,
                                            Losses INTEGER NOT NULL,
                                            Ties INTEGER NOT NULL
                                        );
                                        ";

            using SqliteCommand createPlayerStatsTableCommand = new SqliteCommand(createTableQuery, connection);
            createPlayerStatsTableCommand.ExecuteNonQuery();

            Logger.Info("Player statistics table ready");
            #endregion


        }

        public Dictionary<int, PlayerAccount> LoadAccounts()
        {
            Dictionary<int, PlayerAccount> accounts = new Dictionary<int, PlayerAccount>();
            using SqliteConnection connection = new SqliteConnection(ConnectionString);
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

        public Dictionary<int, MatchData> LoadMatchesData()
        {
            Dictionary<int, MatchData> matchesData = new Dictionary<int, MatchData>();

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM MatchResults";
            using SqliteCommand command = new SqliteCommand(selectQuery, connection);
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int matchID = reader.GetInt32(reader.GetOrdinal("MatchID"));
                int player1ID = reader.GetInt32(reader.GetOrdinal("Player1ID"));
                int player2ID = reader.GetInt32(reader.GetOrdinal("Player2ID"));
                int winnerPlayerID = reader.GetInt32(reader.GetOrdinal("WinnerPlayerID"));
                string finishedAt = reader.GetString(reader.GetOrdinal("FinishedAt"));

                MatchData data = new MatchData(matchID, player1ID, player2ID, winnerPlayerID, DateTime.Parse(finishedAt));

                matchesData.Add(matchID, data);
            }

            return matchesData;
        }

        public Dictionary<int, PlayerStatsData> LoadPlayerStatsData()
        {
            Dictionary<int, PlayerStatsData> playerStatsData = new Dictionary<int, PlayerStatsData>();

            using SqliteConnection connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT * FROM PlayerStats";
            using SqliteCommand command = new SqliteCommand(selectQuery, connection);
            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int playerID = reader.GetInt32(reader.GetOrdinal("PlayerID"));
                int wins = reader.GetInt32(reader.GetOrdinal("Wins"));
                int losses = reader.GetInt32(reader.GetOrdinal("Losses"));
                int ties = reader.GetInt32(reader.GetOrdinal("Ties"));

                PlayerStatsData data = new PlayerStatsData(playerID, wins, losses, ties);

                playerStatsData.Add(playerID, data);
            }

            return playerStatsData;
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

        public void InsertMatchData(MatchData matchData)
        {
            using SqliteConnection connection =
               new SqliteConnection(ConnectionString);

            connection.Open();

            string insertQuery = @"
                                        INSERT INTO MatchResults
                                        (
                                            MatchID,
                                            Player1ID,
                                            Player2ID,
                                            WinnerPlayerID,
                                            FinishedAt
                                        )
                                        VALUES
                                        (
                                            @MatchID,
                                            @Player1ID,
                                            @Player2ID,
                                            @WinnerPlayerID,
                                            @FinishedAt
                                        );
                                        ";

            using SqliteCommand command = new SqliteCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@MatchID", matchData.MatchID);
            command.Parameters.AddWithValue("@Player1ID", matchData.Player_1_ID);
            command.Parameters.AddWithValue("@Player2ID", matchData.Player_2_ID);
            command.Parameters.AddWithValue("@WinnerPlayerID", matchData.WinnerID);
            command.Parameters.AddWithValue("@FinishedAt", matchData.FinishedAt.ToString("O"));

            command.ExecuteNonQuery();
        }

        public void UpdatePlayerStatsData(PlayerStatsData data)
        {
            using SqliteConnection connection =
               new SqliteConnection(ConnectionString);

            connection.Open();

            string insertQuery = @"
                                        INSERT INTO PlayerStats
                                        (
                                            PlayerID,
                                            Wins,
                                            Losses,
                                            Ties
                                        )
                                        VALUES
                                        (
                                            @PlayerID,
                                            @Wins,
                                            @Losses,
                                            @Ties
                                        )
                                        ON CONFLICT(PlayerID)
                                        DO UPDATE SET
                                            Wins = excluded.Wins,
                                            Losses = excluded.Losses,
                                            Ties = excluded.Ties;
                                        ";

            using SqliteCommand command = new SqliteCommand(insertQuery, connection);

            command.Parameters.AddWithValue("@PlayerID", data.PlayerID);
            command.Parameters.AddWithValue("@Wins", data.MatchesWon);
            command.Parameters.AddWithValue("@Losses", data.MatchesLost);
            command.Parameters.AddWithValue("@Ties", data.MatchesTied);

            command.ExecuteNonQuery();
        }
    }
}
