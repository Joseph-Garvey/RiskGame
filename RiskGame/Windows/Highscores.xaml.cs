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
        // Variables //
        List<Player> players; // used for when highscores is clicked from within gamesetup?
        ObservableCollection<GameDetails> playergame;
        private bool music_enabled;
        public bool Music_enabled
        {
            get => music_enabled;
            set
            {

                if (players.Count != 0)
                {
                    try
                    {
                        ((Human)players[0]).music_enabled = value;
                        Human.Update(players[0] as Human);
                    }
                    catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                }
                if (value == true) { mediaplayer.Play(); }
                else if (value == false) { mediaplayer.Pause(); }
                music_enabled = value;
            }
        }
        private bool hints_enabled;
        public bool Hints_enabled
        {
            get => hints_enabled;
            set
            {

                if (players.Count != 0)
                {
                    try
                    {
                        ((Human)players[0]).hints_enabled = value;
                        Human.Update(players[0] as Human);
                    }
                    catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                }
                hints_enabled = value;
            }
        }
        // Constructor(s) //
        public Highscores(GameDetails gameDetails)
        {
            // If loading from completed game show the current players game.
            GameDetails.Save(gameDetails);
            playergame = new ObservableCollection<GameDetails>() { gameDetails };
            Initialise();
            PlayerScoreList.ItemsSource = playergame;
            lblPlayerScore.Visibility = Visibility.Visible;
            PlayerScoreList.Visibility = Visibility.Visible;
        }
        public Highscores(List<Player> _players) { Initialise(); players = _players; }
        public Highscores(bool _musicenabled)
        {
            music_enabled = _musicenabled;
            Initialise();
        }
        // Methods //
        private void Initialise()
        {
            // Initialise UI and show list of saved scores.
            InitializeComponent();
            this.DataContext = this;
            if (players != null)
            {
                music_enabled = ((Human)players[0]).music_enabled;
            }
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            try
            {
                ScoreList.ItemsSource = GameDetails.RetrieveGames();
            }
            catch (Exception) { DispErrorMsg("An error occurred while attempting to retrieve the leaderboard."); }
        }
        private void DispErrorMsg(String Message)
        {
            lblError.Visibility = Visibility.Visible;
            txtError.Text = Message;
        }
        // Button Events //
        private void New_Game(object sender, RoutedEventArgs e)
        {
            if(players == null || players.Count == 0) // Verified working
            {
                Window Login = new MainWindow();
                App.Current.MainWindow = Login;
                this.Close();
                Login.Show();
            }
            else
            {
                GameSetup Setup = new GameSetup(players);
                App.Current.MainWindow = Setup;
                this.Close();
                Setup.Show();
            }
        }
        private void Quit(object sender, RoutedEventArgs e) {  this.Close(); }
    }
}
