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
    public class GameDetails :IComparable<GameDetails>
    {
        private static readonly String FileName = "Leaderboard.bin";
        private readonly String gameID;
        private String lastsave;
        private readonly String player; // stores owner of save file on load, winning player for saves.
        private readonly String noPlayers;
        private readonly String score;
        private readonly String turns;
        private readonly String map;
        private readonly String gamemode;



        public GameDetails(string lastsave, string player, string noPlayers, string score, string turns, string map, string gamemode)
        {
            this.lastsave = lastsave ?? throw new ArgumentNullException(nameof(lastsave));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.noPlayers = noPlayers ?? throw new ArgumentNullException(nameof(noPlayers));
            this.score = score ?? throw new ArgumentNullException(nameof(score));
            this.turns = turns ?? throw new ArgumentNullException(nameof(turns));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            this.gamemode = gamemode ?? throw new ArgumentNullException(nameof(gamemode));
        }

        public GameDetails(string lastsave, string player, string map, string gamemode)
        {
            this.lastsave = lastsave ?? throw new ArgumentNullException(nameof(lastsave));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            this.gamemode = gamemode ?? throw new ArgumentNullException(nameof(gamemode));
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
            List < GameDetails > games = new List<GameDetails>();
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
            games.Sort();
            ObservableCollection<GameDetails> gamessorted = new ObservableCollection<GameDetails>(games);
            return gamessorted;
        }

        public int CompareTo(GameDetails other)
        {
            return int.Parse(other.score).CompareTo(int.Parse(this.score)) ;
        }

        public string NoPlayers => noPlayers;

        public string Player => player;

        public string LastSave => lastsave;

        public string GameID => gameID;

        public string Score => score;

        public string Map => map;

        public string Gamemode => gamemode;
    }
}
