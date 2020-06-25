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
        public Highscores() { Initialise(); }
        // Methods //
        private void Initialise()
        {
            // Initialise UI and show list of saved scores.
            InitializeComponent();
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

        // Window Controls //
        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ChangeWindowState();
            }
            if (e.Key == Key.Escape && this.WindowState == WindowState.Maximized)
            {
                ChangeWindowState();
            }
        }
        private void ChangeWindowState()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.ResizeMode = ResizeMode.CanResize;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowState = WindowState.Normal;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
