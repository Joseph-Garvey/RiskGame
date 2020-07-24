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

namespace RiskGame.Windows
{
    public partial class WelcomeScreen : Window
    {
        /// <summary>
        /// This window greets the user when they launch the application
        /// </summary>
        // Variables //
        private bool music_enabled;
        public bool Music_enabled
        {
            get => music_enabled;
            set
            {
                music_enabled = value;
            }
        }
        private bool hints_enabled;
        public bool Hints_enabled
        {
            get => hints_enabled;
            set
            {
                hints_enabled = value;
            }
        }

        // Constructor //
        public WelcomeScreen()
        {
            InitializeComponent();
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            Music.MusicIndex = 2;
            Music_enabled = true;
            Hints_enabled = true;
            this.DataContext = this;
        }

        // Button Events //
        private void Play_Game(object sender, RoutedEventArgs e)
        {
            // Sends the user to the Login/Registration menu and closes this window.
            Window Login = new MainWindow(music_enabled, hints_enabled, this.WindowState);
            App.Current.MainWindow = Login;
            this.Close();
            Login.Show();
        }
        private void Quit(object sender, RoutedEventArgs e)
        {
            // Closes the game when "Quit" is clicked.
            this.Close();
        }
        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores(Music_enabled, Hints_enabled, ((App)Application.Current).RetrieveWindowState(this));
            App.Current.MainWindow = highscores;
            this.Close();
            highscores.Show();
        }

        // Media Control //
        private void Media_Ended(object sender, RoutedEventArgs e)
        {
            // This event is called when the media playing in the background ends.
            // It plays the next video in the playlist and loops the playlist when finished.
            Uri first = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial.mp4");
            Uri second = new Uri("pack://siteoforigin:,,,/Images/RetroCommercialSega.mp4");
            Uri third = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial90s.mp4");
            if (BackgroundVideo.Source == first) { BackgroundVideo.Source = second; }
            else if (BackgroundVideo.Source == second) { BackgroundVideo.Source = third; }
            else if (BackgroundVideo.Source == third) { BackgroundVideo.Source = first; }
            BackgroundVideo.Position = TimeSpan.Zero;
        }
    }
}
