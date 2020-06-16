using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using RiskGame.CustomExceptions.Game;
using System.Collections.ObjectModel;

namespace RiskGame.Game
{
    [Serializable]
    public class GameManager
    {
        private static readonly String FileName = "GameSaves.bin";
        public List<Player> players;
        public List<Territory> territories;
        public Player currentplayer;
        public int turn;
        public DateTime lastsave;
        public GameState gameState;

        private int gameID;
        public int GameID { get => gameID;}

        // Constructor method for setting gameID
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
            else { gameID = 0; }
        }

        // Constructor ///
        public GameManager(List<Player> players, List<Territory> territories, Player currentplayer, int turn, GameState gameState)
        {
           // Ensures values aren't saved as null.
            this.players = players ?? throw new ArgumentNullException(nameof(players));
            this.territories = territories ?? throw new ArgumentNullException(nameof(territories));
            this.currentplayer = currentplayer ?? throw new ArgumentNullException(nameof(currentplayer));
            this.turn = turn;
            //this.gameState = gameState;
            SetGameID();
        }

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
                            break;
                        }
                    }
                }
                //  seek the start of the file and save the new game.
                sr.Seek(sr.Length, SeekOrigin.Begin);
                newGame.lastsave = DateTime.Now;
                bf.Serialize(sr, newGame);
            }
        }
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
        public static ObservableCollection<GameDetails> RetrieveGames() // Return actual game perhaps? // Can be made more efficient and combined with LoadGame once I learn more about DataBinding to objects.
        {
            /// Retrieve the list of games for the data grid on GameSetup.
            ObservableCollection<GameDetails> games = new ObservableCollection<GameDetails>();
            if (File.Exists(FileName))
            {
                using (Stream sr = new FileStream(FileName, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    while (sr.Position < sr.Length)
                    {
                        GameManager tmp = ((GameManager)bf.Deserialize(sr));
                        GameDetails game = new GameDetails(tmp.GameID.ToString(), tmp.lastsave.ToString(), tmp.players[0].Username, tmp.players.Count().ToString());
                        games.Add(game);
                    }
                }
            }
            return games;
        }
        // If an empty file exists delete it so as to avoid serialization errors later on.
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
    }
}
