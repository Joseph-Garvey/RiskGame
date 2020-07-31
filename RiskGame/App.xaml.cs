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
    /// <summary>
    /// Application events and Application-wide logic.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called on application start.
        /// Controls splash screen.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                // Start Splash Screen and Show
                SplashScreenWindow splashScreen = new SplashScreenWindow();
                this.MainWindow = splashScreen;
                splashScreen.Show();
                // Show Progress Bar on a new thread
                Task.Factory.StartNew(() =>
                {
                    // report progress each iteration to update progress bar.
                    for (int i = 1; i <= 110; i++)
                    {
                        // simulated loading.
                        System.Threading.Thread.Sleep(10);
                        splashScreen.Dispatcher.Invoke(() => splashScreen.Progress = i);
                    }
                    // on completion do this
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
            catch (TaskCanceledException) { }
        }


        #region Window Management
        // Events //
        /// <summary>
        /// F11 toggles fullscreen.
        /// Escape exits fullscreen or toggles settings menu.
        /// </summary>
        /// <param name="sender">Window key pressed on</param>
        /// <param name="e">Contains key pressed</param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Window window = (Window)sender;
            if (e.Key == Key.F11) { ChangeWindowState(window); }
            if (e.Key == Key.Escape)
            {
                if (RetrieveWindowState(window)) // if window fullscreen, exit
                {
                    ChangeWindowState(window);
                }
                else { ChangeDisplay(window); } // otherwise toggle settings
            }
        }
        /// <summary>
        /// Updates fullscreen checkbox.
        /// </summary>
        public void Window_StateChanged(object sender, EventArgs e)
        {
            Window_StateChanged((Window)sender);
        }
        /// <summary>
        /// Updates fullscreen checkbox.
        /// </summary>
        public void Window_StateChanged(Window window)
        {
            try
            {
                if (window == null) { throw new NullReferenceException(); }
                // find checkbox on window
                CheckBox chkFullscreen = (CheckBox)window.FindName("chkFullscreen");
                if (RetrieveWindowState(window)) // if window fullscreen checkbox set to checked.
                {
                    chkFullscreen.IsChecked = true;
                }
                else // else set checkbox to unchecked.
                {
                    chkFullscreen.IsChecked = false;
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// Toggle window fullscreen when fullscreen checkbox clicked.
        /// </summary>
        private void Fullscreen_Click(object sender, RoutedEventArgs e) { ChangeWindowState(RetrieveMainWindow()); }
        // Methods //
        /// <summary>
        /// Toggle window fullscreen on call.
        /// </summary>
        /// <param name="window">Window to toggle.</param>
        public void ChangeWindowState(Window window)
        {
            try
            {
                if (RetrieveWindowState(window)) // if fullscreen make windowed.
                {
                    window.ResizeMode = ResizeMode.CanResize;
                    window.WindowState = WindowState.Normal;
                    window.WindowStyle = WindowStyle.SingleBorderWindow;
                }
                else // if windowed make fullscreen.
                {
                    window.ResizeMode = ResizeMode.NoResize;
                    window.WindowState = WindowState.Normal;
                    window.WindowStyle = WindowStyle.None;
                    window.WindowState = WindowState.Maximized;
                }
            }
            catch(NullReferenceException) { }
        }
        /// <summary>
        /// Returns app mainwindow.
        /// </summary>
        private Window RetrieveMainWindow()
        {
            return Application.Current.MainWindow;
        }
        /// <summary>
        /// Returns if window is fullscreen
        /// </summary>
        /// <param name="window">Window to test.</param>
        /// <returns>True if fullscreen.</returns>
        public bool RetrieveWindowState(Window window)
        {
            if (window.WindowState == WindowState.Maximized) { return true; }
            else { return false; }
        }
        #endregion

        #region Panel Management
        // Events //
        /// <summary>
        /// Toggles settings menu visibility.
        /// </summary>
        private void ChangeDisplay(object sender, RoutedEventArgs e)
        {
            try
            {
                Window window = RetrieveMainWindow();
                if(window == null) { throw new NullReferenceException(); }
                ChangeDisplay(window);
            }
            catch(NullReferenceException) { }
        }
        // Methods //
        /// <summary>
        /// Toggles settings menu visibility.
        /// </summary>
        /// <param name="window">Window to change.</param>
        private void ChangeDisplay(Window window)
        {
            if(window is ChangePassword || window is Tutorial) { return; }
            // find main ui
            StackPanel panel_MainUI = (StackPanel)window.FindName("panel_MainUI");
            // find settings panel
            UIElement object_Settings = (UIElement)window.FindName("panel_Settings");
            // if main ui visible make settings visible and hide main ui
            if (panel_MainUI.Visibility == Visibility.Visible)
            { Settings(panel_MainUI, object_Settings); }
            else // else perform opposite.
            { Return(panel_MainUI, object_Settings); }
        }
        /// <summary>
        /// Make settings panel visible.
        /// </summary>
        /// <param name="panel_MainUI">Window main UI panel.</param>
        /// <param name="panel_Settings">Window settings panel.</param>
        private void Settings(StackPanel panel_MainUI, UIElement panel_Settings)
        {
            panel_MainUI.Visibility = Visibility.Collapsed;
            panel_Settings.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Make main UI panel visible.
        /// </summary>
        /// <param name="panel_MainUI">Window main UI panel.</param>
        /// <param name="panel_Settings">Window settings panel.</param>
        private void Return(StackPanel panel_MainUI, UIElement panel_Settings)
        {
            panel_MainUI.Visibility = Visibility.Visible;
            panel_Settings.Visibility = Visibility.Collapsed;
        }
        // Login, Register and Change Password //
        /// <summary>
        /// Change passwordbox text to match the "showbox" text.
        /// </summary>
        private void ShowPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Window window = RetrieveMainWindow();
            PasswordBox passwordBox = new PasswordBox();
            switch (textBox.Name) // match textbox to its adjacent passwordbox
            {
                case "txtPassShow":
                    return;
                case "txtNewPassShow":
                    return;
                case "txtNewPassConfShow":
                    return;
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
            passwordBox.Password = textBox.Text; // set text
        }
        // Tutorial Window Pop-up //
        /// <summary>
        /// Open tutorial window when button clicked.
        /// </summary>
        public void Tutorial_Window(object sender, RoutedEventArgs e)
        {
            bool open = false;
            foreach(Window w in Application.Current.Windows) // if tutorial already open, exit.
            {
                if(w is Tutorial) { open = true; break; }
            }
            if (!open) // otherwise open and show new window.
            {
                Tutorial tutorial = new Tutorial();
                tutorial.Show();
            }
        }
        #endregion

        #region Music Controls
        // Events //
        /// <summary>
        /// Change music volume to match slider value.
        /// </summary>
        /// <param name="sender">Slider that was changed.</param>
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RetrieveMediaPlayer().Volume = (double)((Slider)sender).Value;
        }
        /// <summary>
        /// Change to previous track.
        /// </summary>
        private void MediaBack(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex -= 1; // go to previous track
            if (sender is MediaElement) { ChangeMedia((MediaElement)sender); }
            else { ChangeMedia(); } // change media to new source.
        }
        /// <summary>
        /// Change to next track.
        /// </summary>
        private void MediaForward(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex += 1; // go to next track
            if (sender is MediaElement) { ChangeMedia((MediaElement)sender); }
            else { ChangeMedia(); } // change media to new source.
        }
        /// <summary>
        /// Pauses music playback.
        /// </summary>
        private void MediaPause(object sender, RoutedEventArgs e)
        {
            Window w = RetrieveMainWindow();
            if (IsMediaEnabled(w)) // find the media element and pause it.
            {
                RetrieveMediaPlayer(w).Pause();
            }
        }
        /// <summary>
        /// Starts music playback.
        /// </summary>
        private void MediaPlay(object sender, RoutedEventArgs e)
        {
            Window w = RetrieveMainWindow();
            if (IsMediaEnabled(w)) // find the media element and start playback.
            {
                RetrieveMediaPlayer(w).Play();
            }
        }
        /// <summary>
        /// Start next track when current track finished.
        /// </summary>
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e) { MediaForward(sender, e); }
        /// <summary>
        /// Update media source label to match name of current media file.
        /// </summary>
        private void UpdateMediaText(object sender, RoutedEventArgs e)
        {
            try
            {
                Window window = RetrieveMainWindow();
                UpdateMediaText((MediaElement)sender, (Label)window.FindName("lblMediaDetails"));
            }
            catch (NullReferenceException) { }
        }
        // Methods //
        /// <summary>
        /// Checks if music is enabled on current window.
        /// </summary>
        /// <param name="w">Window tested.</param>
        /// <returns>True if music is enabled.</returns>
        private bool IsMediaEnabled(Window w)
        {
            if (w is GameSetup) { return (w as GameSetup).Music_enabled; }
            else if (w is MainWindow) { return (w as MainWindow).Music_enabled; }
            else if (w is GameWindow) { return (w as GameWindow).Music_enabled; }
            else if (w is Highscores) { return (w as Highscores).Music_enabled; }
            else { return false; }
        }
        /// <summary>
        /// Update media source label to match name of current media file.
        /// </summary>
        /// <param name="sender">Music element of window.</param>
        /// <param name="l">Music source label.</param>
        private void UpdateMediaText(MediaElement sender, Label l)
        {
            l.Content = sender.Source.ToString().Substring(30);
        }
        /// <summary>
        /// Update media source label to match name of current media file.
        /// </summary>
        /// <param name="window">Window playing music.</param>
        private void UpdateMediaText(Window window, object sender)
        {
            try
            {
                UpdateMediaText((MediaElement)sender, (Label)window.FindName("lblMediaDetails"));
            }
            catch (Exception) { }
        }
        /// <summary>
        /// Retrieves media player from application main window.
        /// </summary>
        private MediaElement RetrieveMediaPlayer()
        {
            Window window = RetrieveMainWindow();
            return RetrieveMediaPlayer(window);
        }
        /// <summary>
        /// Retrieves media player from window.
        /// </summary>
        private MediaElement RetrieveMediaPlayer(Window window)
        {
            try
            {
                MediaElement m = (MediaElement)window.FindName("mediaplayer"); // find mediaelement on window by name.
                if (m != null) { return m; } // return if not null
                else { throw new NullReferenceException(); }
            }
            catch { return new MediaElement(); }
        }
        /// <summary>
        /// Updates music source and plays music if it is enabled.
        /// </summary>
        private void ChangeMedia()
        {
            MediaElement mediaplayer = RetrieveMediaPlayer();
            ChangeMedia(mediaplayer);
        }
        /// <summary>
        /// Updates music source and plays music if it is enabled.
        /// </summary>
        /// <param name="sender">Current music player</param>
        private void ChangeMedia(MediaElement sender)
        {
            Window w = RetrieveMainWindow();
            sender.Source = Music.sources[Music.MusicIndex];
            if (IsMediaEnabled(w))
            {
                sender.Play();
            }
        }
        #endregion
    }
}
