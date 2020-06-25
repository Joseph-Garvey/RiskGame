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
        private void Quit_Game(object sender, RoutedEventArgs e)
        {
            // Closes the game when "Quit" is clicked.
            this.Close();
        }
        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores();
            App.Current.MainWindow = highscores;
            this.Close();
            highscores.Show();
        }
        private void Settings(object sender, RoutedEventArgs e) { Settings(); }
        private void Return(object sender, RoutedEventArgs e) { Return(); }
        private void Tutorial_Window(object sender, RoutedEventArgs e)
        {
            Tutorial tutorial = new Tutorial();
            App.Current.MainWindow = tutorial;
            tutorial.Show();
        }
        // Methods //
        private void Settings()
        {
            panel_MainUI.Visibility = Visibility.Collapsed;
            panel_Settings.Visibility = Visibility.Visible;
        }
        private void Return()
        {
            panel_MainUI.Visibility = Visibility.Visible;
            panel_Settings.Visibility = Visibility.Collapsed;
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

        // Window Management //
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11) { ChangeWindowState(); }
            if (e.Key == Key.Escape)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    ChangeWindowState();
                }
                else
                {
                    if (panel_MainUI.Visibility == Visibility.Visible)
                    {
                        Settings();
                    }
                    else
                    {
                        Return();
                    }
                }
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
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                chkFullscreen.IsChecked = true;
            }
            else
            {
                chkFullscreen.IsChecked = false;
            }
        }
        private void Fullscreen_Click(object sender, RoutedEventArgs e) { ChangeWindowState(); }
    }
}
