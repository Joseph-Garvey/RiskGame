using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RiskGame.Windows;

namespace RiskGame
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Start Splash Screen and Show
            SplashScreenWindow splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();
            // Show Progress Bar on a new thread
            Task.Factory.StartNew(() =>
            {
                //we need to do the work in batches so that we can report progress
                for (int i = 1; i <= 110; i++)
                {
                    //simulate a part of work being done
                    System.Threading.Thread.Sleep(10);
                    splashScreen.Dispatcher.Invoke(() => splashScreen.Progress = i);
                }
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the Welcome Screen window, set as main app window.
                    //and close the splash screen
                    WelcomeScreen mainWindow = new WelcomeScreen();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    splashScreen.Close();
                });
            });
        }

        /// Window Management ///
        // Events //
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Window window = (Window)sender;
            if (e.Key == Key.F11) { ChangeWindowState(window); }
            if (e.Key == Key.Escape && !(window is Tutorial))
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    ChangeWindowState(window);
                }
                else { ChangeDisplay(window); }
            }
        }
        public void Window_StateChanged(object sender, EventArgs e)
        {
            try
            {
                Window window = (Window)sender;
                //Window window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                if(window == null) { throw new NullReferenceException(); }
                CheckBox chkFullscreen = (CheckBox)window.FindName("chkFullscreen");
                if (window.WindowState == WindowState.Maximized)
                {
                    chkFullscreen.IsChecked = true;
                }
                else
                {
                    chkFullscreen.IsChecked = false;
                }
            }
            catch (Exception) { }
        }
        private void Fullscreen_Click(object sender, RoutedEventArgs e) { ChangeWindowState(RetrieveActiveWindow()); }
        // Methods //
        private void ChangeWindowState(Window window)
        {
            try
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.ResizeMode = ResizeMode.CanResize;
                    window.WindowState = WindowState.Normal;
                    window.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else
                {
                    window.ResizeMode = ResizeMode.NoResize;
                    window.WindowState = WindowState.Normal;
                    window.WindowStyle = WindowStyle.None;
                    window.WindowState = WindowState.Maximized;
                }
            }
            catch(NullReferenceException) { }
        }
        private Window RetrieveActiveWindow()
        {
            try
            {
                Window window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                if (window == null) { throw new NullReferenceException(); }
                return window;
            }
            catch (ArgumentNullException) { return Application.Current.MainWindow; }
            catch (NullReferenceException) { return Application.Current.MainWindow; }
        }

        /// Panel Management ///
        // Events //
        private void ChangeDisplay(object sender, RoutedEventArgs e)
        {
            try
            {
                Window window = RetrieveActiveWindow();
                if(window == null) { throw new NullReferenceException(); }
                ChangeDisplay(window);
            }
            catch(NullReferenceException) { }
        }
        // Methods //
        private void ChangeDisplay(Window window)
        {
            if(window is ChangePassword) { return; }
            StackPanel panel_MainUI = (StackPanel)window.FindName("panel_MainUI");
            Panel object_Settings = (Panel)window.FindName("panel_Settings");
            if (panel_MainUI.Visibility == Visibility.Visible)
            { Settings(panel_MainUI, object_Settings); }
            else
            { Return(panel_MainUI, object_Settings); }
        }
        private void Settings(StackPanel panel_MainUI)
        {
            panel_MainUI.Visibility = Visibility.Collapsed;
        }
        private void Settings(StackPanel panel_MainUI, Panel panel_Settings)
        {
            Settings(panel_MainUI);
            panel_Settings.Visibility = Visibility.Visible;
        }
        private void Return(StackPanel panel_MainUI)
        {
            panel_MainUI.Visibility = Visibility.Visible;
        }
        private void Return(StackPanel panel_MainUI, Panel panel_Settings)
        {
            Return(panel_MainUI);
            panel_Settings.Visibility = Visibility.Collapsed;
        }
        // Login, Register and Change Password //
        private void ShowPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Window window = RetrieveActiveWindow();
            PasswordBox passwordBox = new PasswordBox();
            switch (textBox.Name)
            {
                case "txtPassShow":
                    passwordBox = (PasswordBox)(window.FindName("txtPass"));
                    break;
                case "txtNewPassShow":
                    passwordBox = (PasswordBox)(window.FindName("txtNewPass"));
                    break;
                case "txtNewPassConfShow":
                    passwordBox = (PasswordBox)(window.FindName("txtNewPassConf"));
                    break;
                case "txtRegPassShow":
                    passwordBox = (PasswordBox)(window.FindName("txtRegPass"));
                    break;
                case "txtRegPassConfShow":
                    passwordBox = (PasswordBox)(window.FindName("txtRegPassConf"));
                    break;
                case "txtLogPassShow":
                    passwordBox = (PasswordBox)(window.FindName("txtLogPass"));
                    break;
            }
            passwordBox.Password = textBox.Text;
        }
        /// Tutorial Window Pop-up ///
        public void Tutorial_Window(object sender, RoutedEventArgs e)
        {
            // Open Tutorial Window when help button is clicked //
            Tutorial tutorial = new Tutorial(Application.Current.MainWindow);
            tutorial.Show();
        }

        /// Music Controls ///
        // Events //
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RetrieveMediaPlayer().Volume = (double)((Slider)sender).Value;
        }
        private void MediaBack(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex -= 1;
            if (sender is MediaElement) { ChangeMedia((MediaElement)sender); }
            else { ChangeMedia(); }
        }
        private void MediaForward(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex += 1;
            if (sender is MediaElement) { ChangeMedia((MediaElement)sender); }
            else { ChangeMedia(); }
        }
        private void MediaPause(object sender, RoutedEventArgs e)
        {
            foreach (Window w in Application.Current.Windows)
            {
                if(w is GameSetup)
                {
                    (w as GameSetup).Music_enabled = true;
                    RetrieveMediaPlayer(w).Pause();
                    break;
                }
                else if(w is RiskGame.MainWindow)
                {
                    (w as RiskGame.MainWindow).Music_enabled = true;
                    RetrieveMediaPlayer(w).Pause();
                    break;
                }
                else if (w is RiskGame.GameWindow)
                {
                    (w as RiskGame.GameWindow).Music_enabled = true;
                }
                else if (w is RiskGame.Windows.Highscores)
                {
                    (w as RiskGame.Windows.Highscores).Music_enabled = true;
                }
            }
        }
        private void MediaPlay(object sender, RoutedEventArgs e)
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is GameSetup)
                {
                    (w as GameSetup).Music_enabled = true;
                    RetrieveMediaPlayer(w).Play();
                    break;
                }
                else if (w is RiskGame.MainWindow)
                {
                    (w as RiskGame.MainWindow).Music_enabled = true;
                    RetrieveMediaPlayer(w).Play();
                    break;
                }
                else if (w is RiskGame.GameWindow)
                {
                    (w as RiskGame.GameWindow).Music_enabled = true;
                }
                else if (w is RiskGame.Windows.Highscores)
                {
                    (w as RiskGame.Windows.Highscores).Music_enabled = true;
                }
            }
        }
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e) { MediaForward(sender, e); }
        private void UpdateMediaText(object sender, RoutedEventArgs e)
        {
            try
            {
                Window window = RetrieveActiveWindow();
                if (window is Tutorial || window is ChangePassword)
                {
                    foreach (Window w in Application.Current.Windows)
                    {
                        if ((w is GameSetup) || (w is RiskGame.MainWindow) || (w is RiskGame.GameWindow) || (w is RiskGame.Windows.Highscores))
                        {
                            window = w;
                            break;
                        }
                    }
                }
                UpdateMediaText((MediaElement)sender, (Label)window.FindName("lblMediaDetails"));
            }
            catch (NullReferenceException) { UpdateMediaText(Application.Current.MainWindow, sender); }
        }

        // Methods //
        private void UpdateMediaText(MediaElement sender, Label l)
        {
            l.Content = sender.Source.ToString().Substring(30);
        }
        private void UpdateMediaText(Window window, object sender)
        {
            try
            {
                if (window is Tutorial)
                {
                    foreach (Window w in Application.Current.Windows)
                    {
                        if ((w is GameSetup) || (w is RiskGame.MainWindow) || (w is RiskGame.GameWindow) || (w is RiskGame.Windows.Highscores))
                        {
                            window = w;
                            break;
                        }
                    }
                }
                UpdateMediaText((MediaElement)sender, (Label)window.FindName("lblMediaDetails"));
            }
            catch (Exception) { }
        }
        private MediaElement RetrieveMediaPlayer()
        {
            Window window = RetrieveActiveWindow();
            try
            {
                MediaElement m = (MediaElement)window.FindName("mediaplayer");
                if(m != null) { return m;  }
                else { return RetrieveMediaPlayer(Application.Current.MainWindow); }
            }
            catch { return new MediaElement(); }
        }
        private MediaElement RetrieveMediaPlayer(Window window)
        {
            try
            {
                MediaElement m = (MediaElement)window.FindName("mediaplayer");
                if (m != null) { return m; }
                else { throw new NullReferenceException(); }
            }
            catch { return new MediaElement(); }
        }
        private void ChangeMedia()
        {
            MediaElement mediaplayer = RetrieveMediaPlayer();
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            mediaplayer.Play();
        }
        private void ChangeMedia(MediaElement sender)
        {
            MediaElement mediaplayer = sender;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            mediaplayer.Play();
        }
    }
}
