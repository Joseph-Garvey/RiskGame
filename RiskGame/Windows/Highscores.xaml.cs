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
        List<Player> players;

        // Constructor(s) //
        public Highscores(GameDetails gameDetails)
        {
            Initialise();
            ObservableCollection<GameDetails> playergame = new ObservableCollection<GameDetails>() { gameDetails };
            PlayerScoreList.ItemsSource = playergame;
            lblPlayerScore.Visibility = Visibility.Visible;
            PlayerScoreList.Visibility = Visibility.Visible;
        }
        public Highscores() { Initialise(); }

        private void Initialise()
        {
            InitializeComponent();
            ScoreList.ItemsSource = GameDetails.RetrieveGames();
        }
        // Button Events //
        private void New_Game(object sender, RoutedEventArgs e)
        {
            if(players == null) // Verified working
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
