using System;
using System.IO;
using System.Data.SQLite;

namespace Kontur.GameStats.DataBase {
    static class DataBaseInitializer {

        /// <summary>
        /// Создает файл базы данных, если еще не создан и добавляет все необходимые таблицы.
        /// </summary>
        /// <param name="name">Имя файла БД.</param>
        public static void InitializeDataBase(string name) {
            if(!File.Exists (name)) {
                SQLiteConnection.CreateFile (name);
            }
            DataBase.fileName = name;
            CreateTables ();
        }

        /// <summary>
        /// Создает файл базы данных с именем statisticsBase.db3, если еще не создан и добавляет все необходимые таблицы.
        /// </summary>
        public static void InitializeDataBase() {
            if (!File.Exists(DataBase.fileName)) {
                SQLiteConnection.CreateFile (DataBase.fileName);
            }
            CreateTables ();
            /*
            string createQuery = @"CREATE TABLE IF NOT EXISTS
                                 [Servers] (
                                 [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                 [Name] NVARCHAR(2048) NULL,
                                 [Gender] NVARCHAR(2048) NULL)";

            
            using(var connection = new SQLiteConnection ("data source=base.db3")) {
                using(var command = new SQLiteCommand (connection)) {
                    connection.Open ();
                    command.CommandText = createQuery;
                    command.ExecuteNonQuery ();
                    command.CommandText = "INSERT INTO Servers(Name,Gender) values('alex','male')";
                    //command.ExecuteNonQuery ();

                    command.CommandText = "SELECT * from Servers";
                    using(var reader = command.ExecuteReader ()) {
                        while(reader.Read ()) {
                            Console.WriteLine (reader["Name"] + ":" + reader["Gender"]);
                        }
                    }
                    connection.Close ();
                }
            }*/
        }
  
        static void CreateTables() {
            string createQuery =
                createServers +
                createPlayers +
                createMaps +
                createGameMods +
                createMatches +
                createServerGameModes +
                createServerMatchesPerDay +
                createServerTopMaps +
                createPlayerMatchesPerDay +
                createPlayerStatsOnGameMode +
                createPlayerStatsOnServer +
                createScoreBoards;

            using(var connection = new SQLiteConnection ("data source=" + DataBase.fileName)) {
                using(var command = new SQLiteCommand (connection)) {
                    connection.Open ();
                    command.CommandText = createQuery;
                    command.ExecuteNonQuery ();
                    connection.Close ();
                }
            }
            try {

            } catch(SQLiteException ex) {
                var code = ex.ErrorCode;
            }
        }

        const string createServers = @"CREATE TABLE IF NOT EXISTS
                                    servers (
                                        serverID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                        endpoint TEXT UNIQUE,
                                        serverName TEXT,
                                        matchesCount INTEGER DEFAULT 0,
                                        maxPopulation INTEGER DEFAULT 0,
                                        totalPopulation INTEGER DEFAULT 0,
                                        maxMatchesPerDay INTEGER DEFAULT 0, 
                                        daysCount INTEGER DEFAULT 0
                                    );
                                    
                                    CREATE UNIQUE INDEX IF NOT EXISTS IX_servers ON servers (endpoint);";

        const string createPlayers = @"CREATE TABLE IF NOT EXISTS
                                        players (
                                            playerID INTEGER PRIMARY KEY AUTOINCREMENT,
                                            nickname TEXT UNIQUE,
                                            lastMatchPlayed INTEGER,
                                            totalMatchesPlayed INTEGER DEFAULT 0,
                                            totalMatchesWon INTEGER DEFAULT 0,
                                            avScoreBoardPercent REAL DEFAULT 0,
                                            totalKills INTEGER DEFAULT 0,
                                            totalDeaths INTEGER DEFAULT 0,
                                            maxMatchesPerDay INTEGER DEFAULT 0);

                                        CREATE UNIQUE INDEX IF NOT EXISTS IX_playerNickname ON players (nickname COLLATE NOCASE);";

        const string createMaps = @"CREATE TABLE IF NOT EXISTS maps (
                                        mapID INTEGER PRIMARY KEY AUTOINCREMENT,
                                        map TEXT UNIQUE
                                    );
                                    CREATE INDEX IF NOT EXISTS IX_maps ON maps (map);";

        const string createGameMods = @"CREATE TABLE IF NOT EXISTS gameModes
                                        (
                                            gameModeID INTEGER PRIMARY KEY AUTOINCREMENT,
                                            gameMode TEXT
                                        );
                                        CREATE UNIQUE INDEX IF NOT EXISTS IX_gameMode ON gameModes (gameMode);";

        const string createMatches = @"CREATE TABLE IF NOT EXISTS matches (
                                            matchID INTEGER PRIMARY KEY AUTOINCREMENT,
                                            serverID INTEGER,
                                            timeStamp INTEGER,
                                            gameModeID INTEGER,
                                            population INTEGER,
                                            fragLimit INTEGER,
                                            timeLimit INTEGER,
                                            timeElapsed REAL,
                                            mapID INTEGER,
    
                                            FOREIGN KEY(serverID) REFERENCES servers(serverID),
                                            FOREIGN KEY(gameModeID) REFERENCES gameModes(gameModeID),
                                            FOREIGN KEY(mapID) REFERENCES maps(mapID)
                                        );
                                        CREATE UNIQUE INDEX IF NOT EXISTS IX_matches ON matches (serverID, timeStamp);
                                        CREATE UNIQUE INDEX IF NOT EXISTS IX_matchesByDate ON matches (timeStamp DESC);";

        const string createServerGameModes = @"CREATE TABLE IF NOT EXISTS serverGameModes (
                                                    serverID INTEGER,
                                                    gameModeID INTEGER,
                                                    playCount INTEGER DEFAULT 0,

                                                    FOREIGN KEY(serverID) REFERENCES servers(serverID),
                                                    FOREIGN KEY(gameModeID) REFERENCES gameModes(gameModeID)
                                                );
                                                CREATE INDEX IF NOT EXISTS IX_TopGameModes ON serverGameModes (serverID, playCount DESC);";

        const string createServerMatchesPerDay = @"CREATE TABLE IF NOT EXISTS serverMatchesPerDay (
                                                        serverID INTEGER,
                                                        dayID INTEGER,
                                                        matchesCount INTEGER DEFAULT 0,
                                                        totalPopulation INTEGER DEFAULT 0,
                                                        PRIMARY KEY(serverID, dayID),

                                                        FOREIGN KEY(serverID) REFERENCES servers(serverID)
                                                    );

                                                 CREATE INDEX IF NOT EXISTS IX_serverMatchesPerDay ON serverMatchesPerDay (serverID, matchesCount DESC);";

        const string createServerTopMaps = @"CREATE TABLE IF NOT EXISTS serverTopMaps (
                                                mapID INTEGER,
                                                serverID INTEGER,
                                                playCount INTEGER DEFAULT 0,

                                                PRIMARY KEY(mapID, serverID),
                                                FOREIGN KEY(mapID) REFERENCES maps(mapID)
                                            );

                                            CREATE INDEX IF NOT EXISTS IX_serverTopMaps ON serverTopMaps (serverID, playCount DESC);";

        const string createPlayerMatchesPerDay = @"CREATE TABLE IF NOT EXISTS playerMatchesPerDay (
                                                        playerID INTEGER,
                                                        dayID INTEGER,
                                                        playCount INTEGER DEFAULT 0,
                                                        PRIMARY KEY(playerID, dayID),
                                                        FOREIGN KEY(playerID) REFERENCES players(playerID)
                                                    );

                                                    CREATE INDEX IF NOT EXISTS IX_PlayerMatchesPerDay ON playerMatchesPerDay (playerID, playCount DESC);";

        const string createPlayerStatsOnGameMode = @"CREATE TABLE IF NOT EXISTS playerStatsOnGameMode (
                                                        playerID INTEGER,
                                                        gameModeID INTEGER,
                                                        playCount INTEGER DEFAULT 0,
                                                        winCount INTEGER DEFAULT 0,

                                                        PRIMARY KEY(playerID, gameModeID),
                                                        FOREIGN KEY(playerID) REFERENCES players(playerID),
                                                        FOREIGN KEY(gameModeID) REFERENCES gameModes(gameModeID)
                                                    );

                                                    CREATE INDEX IF NOT EXISTS IX_PlayerOnGameMode ON playerStatsOnGameMode (playerID, playCount DESC);";

        const string createPlayerStatsOnServer = @"CREATE TABLE IF NOT EXISTS playerStatsOnServer (
                                                        playerID INTEGER,
                                                        serverID INTEGER,
                                                        playCount INTEGER DEFAULT 0,
                                                        winsCount INTEGER DEFAULT 0,

                                                        PRIMARY KEY(playerID, serverID),
                                                        FOREIGN KEY(playerID) REFERENCES players(playerID),
                                                        FOREIGN KEY(serverID) REFERENCES servers(serverID)
                                                    );

                                                    CREATE INDEX IF NOT EXISTS IX_PlayerOnServer ON playerStatsOnServer (playerID, playCount DESC);";

        const string createScoreBoards = @"CREATE TABLE IF NOT EXISTS scoreBoards (
                                                matchID INTEGER,
                                                playerID INTEGER,
                                                Frags INTEGER,
                                                Kills INTEGER,
                                                Deaths INTEGER,
                                                Place INTEGER,

                                                PRIMARY KEY(matchID, playerID),
                                                FOREIGN KEY(matchID) REFERENCES matches(matchID),
                                                FOREIGN KEY(playerID) REFERENCES players(playerID)
                                            );";
    }
}
