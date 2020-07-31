using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RiskGame.Game;
using System.Collections.ObjectModel;

namespace RiskGame.Windows
{
    /// <summary>
    /// Interaction logic for Highscores.xaml
    /// </summary>
    public partial class Highscores : Window
    {
        #region Variables and Properties
        private List<Player> players; // Used to store the list of currently logged in players
        private ObservableCollection<GameDetails> playergame; // Stores the details of the game that the user has just finished?
        private bool music_enabled; // Stores the user's music preference.
        public bool Music_enabled // Accessor for the music enabled variable.
        {
            get => music_enabled;
            set
            {
                if(players != null) // if players are logged in
                {
                    if (players.Count != 0)
                    {
                        try
                        {
                            ((Human)players[0]).music_enabled = value; // save the user's preference to the file.
                            Human.Update(players[0] as Human);
                        }
                        catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                    }
                    if (value == true) { mediaplayer.Play(); } // If music has just been enabled, play it.
                    else if (value == false) { mediaplayer.Pause(); } // If music has just been disabled, stop the music.
                }
                music_enabled = value; // Set the value
            }
        }
        private bool hints_enabled; // Stores the user's hints preference
        public bool Hints_enabled // Accessor for the hints enabled variable.
        {
            get => hints_enabled;
            set
            {
                if(players != null) // If players are logged in
                {
                    if (players.Count != 0)
                    {
                        try
                        {
                            ((Human)players[0]).hints_enabled = value;
                            Human.Update(players[0] as Human); // Save their preference to the file
                        }
                        catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                    }
                }
                hints_enabled = value; // Set the value
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor used when a player has won a game.
        /// </summary>
        /// <param name="gameDetails"> Details of the won game </param>
        /// <param name="_players"> List of currently logged in players. </param>
        public Highscores(GameDetails gameDetails, List<Player> _players)
        {
            playergame = new ObservableCollection<GameDetails>() { gameDetails }; // Create a bindable list and add the record to it.
            players = _players;
            Initialise(false, ((Human)players[0]).music_enabled, ((Human)players[0]).hints_enabled); // Initialise the window
            foreach(Player p in players) // Reset each player's stats in case they start a new game.
            {
                p.Army_strength = 0;
                p.army_undeployed = 0;
                p.Territoriesowned = 0;
                p.score = 0;
                p.Color = null;
            }
            PlayerScoreList.ItemsSource = playergame; // Setup data binding for the DataGrid to display the finished game's details.
            lblPlayerScore.Visibility = Visibility.Visible; // Make the grid and its header visible
            PlayerScoreList.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Constructor used when players are logged in.
        /// </summary>
        /// <param name="_players"> Currently logged in players </param>
        /// <param name="fullscreen"> Player's full-screen preference </param>
        public Highscores(List<Player> _players, bool fullscreen) { Initialise(fullscreen, ((Human)_players[0]).music_enabled, ((Human)_players[0]).hints_enabled); players = _players; }
        /// <summary>
        /// Constructor used when a player is not logged in
        /// </summary>
        /// <param name="_musicenabled">Player's music preference</param>
        /// <param name="_hintsenabled">Player's hints preference</param>
        /// <param name="_fullscreen">Player's full-screen preference</param>
        public Highscores(bool _musicenabled, bool _hintsenabled, bool _fullscreen)
        {
            Initialise(_musicenabled, _hintsenabled, _fullscreen);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialises the window, contains code shared between all constructors.
        /// </summary>
        /// <param name="_musicenabled">Player's music preference</param>
        /// <param name="_hintsenabled">Player's hints preference</param>
        /// <param name="fullscreen">Player's full-screen preference</param>
        private void Initialise(bool _musicenabled, bool _hintsenabled, bool fullscreen)
        {
            // Initialise UI and show list of saved scores.
            InitializeComponent();
            this.DataContext = this;
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            Music_enabled = _musicenabled;
            Hints_enabled = _hintsenabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            try
            {
                ObservableCollection<GameDetails> records = GameDetails.RetrieveGames();
                ScoreList.ItemsSource = records;
                if(records.Count == 0)
                {
                    DispErrorMsg("There aren't any completed games saved on file.");
                }
            }
            catch (Exception) { DispErrorMsg("An error occurred while attempting to retrieve the leaderboard."); }
            if (fullscreen)
            {
                ((App)Application.Current).ChangeWindowState(this);
                ((App)Application.Current).Window_StateChanged(this);
            }
        }
        /// <summary>
        /// Displays the given string as an error onscreen.
        /// </summary>
        /// <param name="Message">Output error message.</param>
        private void DispErrorMsg(String Message)
        {
            lblError.Visibility = Visibility.Visible;
            txtError.Text = Message;
        }
        #endregion

        #region Events
        /// <summary>
        /// Called when New Game clicked. Creates a new window,
        /// </summary>
        private void New_Game(object sender, RoutedEventArgs e)
        {
            if(players == null || players.Count == 0) // If players are not logged in, direct them to Login / Register
            {
                Window Login = new MainWindow(Music_enabled, Hints_enabled, ((App)Application.Current).RetrieveWindowState(this));
                App.Current.MainWindow = Login;
                this.Close();
                Login.Show();
            }
            else // If players are signed in direct them to game-setup
            {
                GameSetup Setup = new GameSetup(players, ((App)Application.Current).RetrieveWindowState(this));
                App.Current.MainWindow = Setup;
                this.Close();
                Setup.Show();
            }
        }
        /// <summary>
        /// Closes the window
        /// </summary>
        private void Quit(object sender, RoutedEventArgs e) {  this.Close(); }
        #endregion
    }
}
