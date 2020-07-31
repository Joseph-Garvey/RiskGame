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
    /// <summary>
    /// GameDetails class is used to display Highscores and details of games on Datagrids.
    /// </summary>
    [Serializable]
    public class GameDetails :IComparable<GameDetails>
    {
        #region Variables
        private static readonly String FileName = "Leaderboard.bin"; // Location of highscores records.
        private readonly String gameID; // Stores the unique identifier for the game
        private String lastsave; // Stores the last time the game was saved.
        private readonly String player; // Stores owner of save file when loading, player who won game in highscores.
        private readonly String noPlayers; // number of players in game
        private readonly String score; // The winning player's score.
        private readonly String turns; // Number of turns taken to win the game
        private readonly String map; // Map used for game.
        private readonly String gamemode; // Mode used for game.
        #endregion

        #region Constructors
        /// <summary>
        /// Used to store highscores records, takes in necessary details for displaying the highscore record.
        /// </summary>
        /// <param name="player">Player who won game</param>
        /// <param name="score">Winning player's score</param>
        /// <param name="turns">Number of turns taken to win</param>
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
        /// <summary>
        ///Used to display the details of a game for loading
        /// </summary>
        /// <param name="player">Owner of game-save</param>
        public GameDetails(string lastsave, string player, string map, string gamemode, string gameID)
        {
            this.lastsave = lastsave ?? throw new ArgumentNullException(nameof(lastsave));
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.map = map ?? throw new ArgumentNullException(nameof(map));
            this.gamemode = gamemode ?? throw new ArgumentNullException(nameof(gamemode));
            this.gameID = gameID ?? throw new ArgumentNullException(nameof(gameID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves high-score records to binary file via serialisation.
        /// </summary>
        /// <param name="gameDetails">Record of highscore</param>
        public static void Save(GameDetails gameDetails)
        {
            /// Saves highscore records to binary file via serialisation.
            using(FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate)) { } // Create file if it does not exist
            // Open the new/existing file.
            using (FileStream sr = new FileStream(FileName, FileMode.Append))
            {
                BinaryFormatter bf = new BinaryFormatter(); // Create a new BinaryFormatter
                gameDetails.lastsave = DateTime.Now.ToString("g"); // Create a string with the current date and time.
                bf.Serialize(sr, gameDetails); // Serialise the object to the file.
            }
        }
        /// <summary>
        /// Retrieves the details of every high-score.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<GameDetails> RetrieveGames()
        {
            List < GameDetails > games = new List<GameDetails>();
            if (File.Exists(FileName)) // If the file exists
            {
                using (Stream sr = new FileStream(FileName, FileMode.Open)) // Open the file
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    while (sr.Position < sr.Length) // While not reached end of file
                    {
                        GameDetails read = ((GameDetails)bf.Deserialize(sr)); // De-serialize the object
                        games.Add(read); // Add it to the list
                    }
                }
            }
            games.Sort(); // Sort the list by highest score
            ObservableCollection<GameDetails> gamessorted = new ObservableCollection<GameDetails>(games); // Convert it to a bind-able list.
            return gamessorted;
        }
        /// <summary>
        /// Default comparer for the GameDetails class,
        /// used to sort records by the highest score.
        /// </summary>
        /// <param name="other">Object to compare</param>
        public int CompareTo(GameDetails other)
        {
            return int.Parse(other.score).CompareTo(int.Parse(this.score)) ;
        }
        #endregion

        #region Accessors
        public string NoPlayers => noPlayers;
        public string Player => player;
        public string LastSave => lastsave;
        public string GameID => gameID;
        public string Score => score;
        public string Map => map;
        public string Gamemode => gamemode;
        #endregion
    }
}
