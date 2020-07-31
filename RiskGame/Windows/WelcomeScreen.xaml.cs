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
    /// <summary>
    /// Interaction logic for welcome window.
    /// </summary>
    public partial class WelcomeScreen : Window
    {
        #region Variables and Properties
        private bool music_enabled; // Stores the user's music preference.
        public bool Music_enabled // Accessor for the music enabled variable.
        {
            get => music_enabled;
            set
            {
                music_enabled = value;
            }
        }
        private bool hints_enabled; // Stores the user's hints preference
        public bool Hints_enabled
        {
            get => hints_enabled;
            set
            {
                hints_enabled = value;
            }
        } // Accessor for the hints enabled variable.
        #endregion
        #region Constructor
        /// <summary>
        /// Default Constructor.
        /// Sets up data-binding, UI, events and default properties.
        /// </summary>
        public WelcomeScreen()
        {
            InitializeComponent(); // UI Setup
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged); // StateChanged event set
            Music.MusicIndex = 2; // Default music track
            Music_enabled = true; // Music is enabled by default
            Hints_enabled = true; // Hints are enabled by default.
            this.DataContext = this; // Data-binding setup
        }
        #endregion
        #region Button OnClick Events
        /// <summary>
        /// Launches a new Login / Register window.
        /// Closes this window.
        /// </summary>
        private void Play_Game(object sender, RoutedEventArgs e)
        {
            // Sends the user to the Login/Registration menu and closes this window.
            Window Login = new MainWindow(music_enabled, hints_enabled, ((App)Application.Current).RetrieveWindowState(this)); // Create new window and pass user preferences as parameters.
            App.Current.MainWindow = Login;
            this.Close(); // Close this window.
            Login.Show(); // Show the new window.
        }
        /// <summary>
        /// Closes the current window
        /// </summary>
        private void Quit(object sender, RoutedEventArgs e)
        {
            // Closes the game when "Quit" is clicked.
            this.Close();
        }
        /// <summary>
        /// Open high-scores window.
        /// Closes this window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores(Music_enabled, Hints_enabled, ((App)Application.Current).RetrieveWindowState(this)); // Creates new window passing user preferences as parameters.
            App.Current.MainWindow = highscores;
            this.Close(); // Closes this window.
            highscores.Show(); // Shows new window
        }
        #endregion
        #region Background Video Events
        /// <summary>
        /// Called when background video ends.
        /// Starts next video in playlist, repeating playlist on completion of all videos.
        /// </summary>
        private void Media_Ended(object sender, RoutedEventArgs e)
        {
            // File-paths of videos
            Uri first = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial.mp4");
            Uri second = new Uri("pack://siteoforigin:,,,/Images/RetroCommercialSega.mp4");
            Uri third = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial90s.mp4");
            // Play next video
            if (BackgroundVideo.Source == first) { BackgroundVideo.Source = second; }
            else if (BackgroundVideo.Source == second) { BackgroundVideo.Source = third; }
            else if (BackgroundVideo.Source == third) { BackgroundVideo.Source = first; }
            // Set video to start
            BackgroundVideo.Position = TimeSpan.Zero;
        }
        #endregion
    }
}
