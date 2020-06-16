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
        public WelcomeScreen()
        {
            InitializeComponent();
        }

        private void Play_Game(object sender, RoutedEventArgs e)
        {
            // Sends the user to the Login/Registration menu and closes this window.
            Window Login = new MainWindow();
            App.Current.MainWindow = Login;
            this.Close();
            Login.Show();
        }

        private void Media_Ended(object sender, RoutedEventArgs e)
        {
            // This event is called when the media playing in the background ends.
            // It plays the next video in the playlist and loops the playlist when finished.
            Uri first = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial.mp4");
            Uri second = new Uri("pack://siteoforigin:,,,/Images/RetroCommercialSega.mp4");
            Uri third = new Uri("pack://siteoforigin:,,,/Images/RetroCommercial90s.mp4");
            if(BackgroundVideo.Source == first) { BackgroundVideo.Source = second; }
            else if(BackgroundVideo.Source == second) { BackgroundVideo.Source = third; }
            else if(BackgroundVideo.Source == third) { BackgroundVideo.Source = first; }
            BackgroundVideo.Position = TimeSpan.Zero;
        }

        private void Quit_Game(object sender, RoutedEventArgs e)
        {
            // Closes the game when "Quit" is clicked.
            this.Close();
        }
    }
}
