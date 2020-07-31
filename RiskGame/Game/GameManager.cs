using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RiskGame.CustomExceptions;
using System.Collections.ObjectModel;
using RiskGame.Game.Locations;

namespace RiskGame.Game
{
    /// <summary>
    /// Class that contains all game properties and objects.
    /// Also contains additional methods for file management.
    /// </summary>
    [Serializable]
    public class GameManager
    {
        #region Variables and Properties
        /// <summary>
        /// File-path for saved games.
        /// </summary>
        private static readonly String FileName = "GameSaves.bin";
        /// <summary>
        /// Players in game.
        /// </summary>
        public List<Player> players = new List<Player>();
        /// <summary>
        /// List of territories in map.
        /// </summary>
        public List<Territory> territories;
        /// <summary>
        /// List of regions in map.
        /// </summary>
        public List<Continent> continents;
        /// <summary>
        /// Currently selected territory.
        /// </summary>
        public Territory slctTerritory;
        /// <summary>
        /// Second selected territory.
        /// </summary>
        public Territory nextTerritory;
        public Player currentplayer;
        /// <summary>
        /// Number of completed turns in game.
        /// </summary>
        public int turn;
        /// <summary>
        /// Maximum time allowed for a turn.
        /// </summary>
        public int time;
        /// <summary>
        /// New Risk bias towards defending players.
        /// </summary>
        /// <remarks>Default 0.5, range 0 to 1</remarks>
        public double defenderbias;
        public DateTime lastsave;
        public GameState gameState;
        public GameMode gamemode;
        public GameMap map;
        /// <summary>
        /// Unique identifier for each saved game object.
        /// </summary>
        private int gameID;
        /// <summary>
        /// Read-only accessor for gameID.
        /// </summary>
        public int GameID { get => gameID;}
        #endregion

        #region Constructor and related method(s).
        /// <summary>
        /// Default constructor, sets gameID.
        /// </summary>
        public GameManager()
        {
            SetGameID();
        }
        /// <summary>
        /// Sets unique gameID
        /// </summary>
        private void SetGameID()
        {
            /// Check what gameIDs currently exist and assign a unique ID
            if (File.Exists(FileName))
            {
                using (Stream sr = new FileStream(FileName, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    GameManager read;
                    List<int> GameIDs = new List<int>();
                    while (sr.Position < sr.Length)
                    {
                        read = (GameManager)bf.Deserialize(sr);
                        GameIDs.Add(read.GameID);
                    }
                    for (int i = 0; i < (GameIDs.Count + 1); i++)
                    {
                        if (!GameIDs.Contains(i)) { gameID = i; break; }
                    }
                }
            }
            else { this.gameID = 0; }
        }
        #endregion

        #region File Management
        /// <summary>
        /// Finds game on file matching gameID and removes the game from the file.
        /// </summary>
        /// <param name="_gameID">Game ID to be deleted.</param>
        public static void DeleteGame(int _gameID)
        {
            ClearEmptyFile();
            // Loads game objects from the file.
            if (File.Exists(FileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                List<GameManager> gm = new List<GameManager>();
                // If the file exists find the game matching that ID and retrieve the object.
                using (Stream sr = new FileStream(FileName, FileMode.Open))
                {
                    GameManager read;
                    while (sr.Position < sr.Length)
                    {
                        read = (GameManager)bf.Deserialize(sr);
                        if (read.GameID != _gameID)
                        {
                            gm.Add(read); // if game is not that to be deleted, add it to list.
                        }
                    }
                }
                File.Delete(FileName); // delete file.
                // Rewrite file without the deleted game.
                using (Stream sr = new FileStream(FileName,FileMode.Create))
                {
                    foreach (GameManager g in gm)
                    {
                        bf.Serialize(sr, g);
                    }
                }
            }
        }
        /// <summary>
        /// Saves game to file.
        /// </summary>
        /// <param name="newGame">Game object to be saved.</param>
        public static void SaveGame(GameManager newGame)
        {
            /// Saves games to binary file via serialisation.
            FileMode filemode;
            // If file exists open it, otherwise create a new file.
            if (File.Exists(FileName)){
                filemode = FileMode.Open;
            }
            else {filemode = FileMode.OpenOrCreate; }
            // Open the new/existing file.
            using (FileStream sr = new FileStream(FileName, filemode))
            {
                BinaryFormatter bf = new BinaryFormatter();
                // If the file exists check the game does not already exist.
                if(filemode == FileMode.Open)
                {
                    GameManager read;
                    while (sr.Position < sr.Length) // While not at end of file.
                    {
                        long pos = sr.Position;
                        read = (GameManager)bf.Deserialize(sr); // Read the existing games on file.
                        if (read.GameID == newGame.GameID) // If the game that is trying to be saved already exists overwrite it with the new details.
                        {
                            sr.Seek(pos, SeekOrigin.Begin);
                            newGame.lastsave = DateTime.Now;
                            bf.Serialize(sr, newGame);
                            return;
                        }
                    }
                }
                //  seek the start of the file and save the new game.
                sr.Seek(sr.Length, SeekOrigin.Begin);
                newGame.lastsave = DateTime.Now;
                bf.Serialize(sr, newGame);
            }
        }
        /// <summary>
        /// Loads game object from file.
        /// </summary>
        /// <param name="_GameID">ID of game to be loaded.</param>
        public static GameManager LoadGame(int _GameID)
        {
            // Loads game objects from the file.
            if (File.Exists(FileName))
            {
                // If the file exists find the game matching that ID and retrieve the object.
                using (Stream sr = new FileStream(FileName, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    GameManager read;
                    while (sr.Position < sr.Length)
                    {
                        read = (GameManager)bf.Deserialize(sr);
                        if (read.GameID == _GameID) {
                            foreach(Player p in read.players) { p.RetrieveColor(); }
                            return read; }
                    }
                    throw new GameNotFoundException();
                }
            }
            else { throw new GameNotFoundException(); } // If the file does not exist you cannot load a game.
        }
        /// <summary>
        /// Returns list of saved games owned by logged in players.
        /// </summary>
        /// <param name="loggedinplayers">Currently logged in players.</param>
        /// <returns>List of saved games.</returns>
        /// <remarks>// Return actual game perhaps? Can be made more efficient and combined with LoadGame once I learn more about DataBinding to objects.</remarks>
        public static ObservableCollection<GameDetails> RetrieveGames(List<Human> loggedinplayers)
        {
            /// Retrieve the list of games for the data grid on GameSetup.
            ObservableCollection<GameDetails> games = new ObservableCollection<GameDetails>();
            if (File.Exists(FileName))
            {
                using (Stream sr = new FileStream(FileName, FileMode.Open)) // Open file
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    while (sr.Position < sr.Length) // While not at end of file
                    {
                        GameManager tmp = ((GameManager)bf.Deserialize(sr)); // De-serialize next object
                        bool containsplayer = false;
                        foreach (Human h in loggedinplayers) // if a logged in player owns the game
                        {
                            if (h.Username == tmp.players[0].Username) { containsplayer = true; break; }
                        }
                        if (containsplayer) // add game to list
                        {
                            GameDetails game = new GameDetails(tmp.lastsave.ToString("g"), tmp.players[0].Username, tmp.map.ToString(), tmp.gamemode.ToString(), tmp.GameID.ToString());
                            games.Add(game);
                        }
                    }
                }
            }
            return games;
        }
        /// <summary>
        /// Deletes empty game save files to avoid errors when de-serializing.
        /// </summary>
        public static void ClearEmptyFile()
        {
            if (File.Exists(FileName))
            {
                try
                {
                    using (Stream sr = new FileStream(FileName, FileMode.Open))
                    {
                        BinaryFormatter bf = new BinaryFormatter(); // If an error occurs the file is empty or corrupted.
                        GameManager read = (GameManager)bf.Deserialize(sr);
                    }
                }
                catch (SerializationException) { File.Delete(FileName);
                }
            }
        }
        #endregion
    }
}
