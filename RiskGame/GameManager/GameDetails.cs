using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;

namespace RiskGame.Game
{
    [Serializable]
    public class GameDetails
    {
        private static readonly String FileName = "Leaderboard.bin";
        private readonly String gameID;
        private String lastsave;
        private readonly String player; // stores owner of save file on load, winning player for saves.
        private readonly String noPlayers;
        private readonly String score;
        private readonly String turns;

        public GameDetails(string gameID, string lastsave, string player, string noPlayers)
        {
            this.gameID = gameID ?? throw new ArgumentNullException(nameof(gameID));
            this.lastsave = lastsave ?? throw new ArgumentNullException(nameof(lastsave));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.noPlayers = noPlayers ?? throw new ArgumentNullException(nameof(noPlayers));
        }

        public GameDetails(string lastsave, string player, string noPlayers, string score, string turns)
        {
            this.lastsave = lastsave ?? throw new ArgumentNullException(nameof(lastsave));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.noPlayers = noPlayers ?? throw new ArgumentNullException(nameof(noPlayers));
            this.score = score ?? throw new ArgumentNullException(nameof(score));
            this.turns = turns ?? throw new ArgumentNullException(nameof(turns));
        }

        public static void Save(GameDetails gameDetails)
        {
            /// Saves records to binary file via serialisation.
            using(FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { }
            // Open the new/existing file.
            using (FileStream sr = new FileStream(FileName, FileMode.Append))
            {
                BinaryFormatter bf = new BinaryFormatter();
                gameDetails.lastsave = DateTime.Now.ToString("g");
                bf.Serialize(sr, gameDetails);
            }
        }

        public static ObservableCollection<GameDetails> RetrieveGames() // Return actual game perhaps? // Can be made more efficient and combined with LoadGame once I learn more about DataBinding to objects.
        { // adapt
            /// Retrieve the list of games for the datagrid on GameSetup.
            ObservableCollection<GameDetails> games = new ObservableCollection<GameDetails>();
            if (File.Exists(FileName))
            {
                using (Stream sr = new FileStream(FileName, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    while (sr.Position < sr.Length)
                    {
                        GameDetails read = ((GameDetails)bf.Deserialize(sr));
                        games.Add(read);
                    }
                }
            }
            return games;
        }

        public string NoPlayers => noPlayers;

        public string Player => player;

        public string LastSave => lastsave;

        public string GameID => gameID;
    }
}
