﻿using System;
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
            catch (TaskCanceledException) { }
        }


        /// Window Management ///
        // Events //
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Window window = (Window)sender;
            if (e.Key == Key.F11) { ChangeWindowState(window); }
            if (e.Key == Key.Escape)
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
        private void Fullscreen_Click(object sender, RoutedEventArgs e) { ChangeWindowState(RetrieveMainWindow()); }
        // Methods //
        public void ChangeWindowState(Window window)
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
        private Window RetrieveMainWindow()
        {
            return Application.Current.MainWindow;
        }
        public bool RetrieveWindowState(Window window)
        {
            if (window.WindowState == WindowState.Maximized) { return true; }
            else { return false; }
        }

        /// Panel Management ///
        // Events //
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
        private void ChangeDisplay(Window window)
        {
            if(window is ChangePassword || window is Tutorial) { return; }
            StackPanel panel_MainUI = (StackPanel)window.FindName("panel_MainUI");
            UIElement object_Settings = (UIElement)window.FindName("panel_Settings");
            if (panel_MainUI.Visibility == Visibility.Visible)
            { Settings(panel_MainUI, object_Settings); }
            else
            { Return(panel_MainUI, object_Settings); }
        }
        private void Settings(StackPanel panel_MainUI, UIElement panel_Settings)
        {
            panel_MainUI.Visibility = Visibility.Collapsed;
            panel_Settings.Visibility = Visibility.Visible;
        }
        private void Return(StackPanel panel_MainUI, UIElement panel_Settings)
        {
            panel_MainUI.Visibility = Visibility.Visible;
            panel_Settings.Visibility = Visibility.Collapsed;
        }
        // Login, Register and Change Password //
        private void ShowPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            Window window = RetrieveMainWindow();
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
            Tutorial tutorial = new Tutorial();
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
            Window w = RetrieveMainWindow();
            MediaEnabled(w);
            RetrieveMediaPlayer(w).Pause();
        }
        private void MediaPlay(object sender, RoutedEventArgs e)
        {
            Window w = RetrieveMainWindow();
            MediaEnabled(w);
            RetrieveMediaPlayer(w).Play();
        }
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e) { MediaForward(sender, e); }
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
        private void MediaEnabled(Window w)
        {
            if (w is GameSetup)
            { (w as GameSetup).Music_enabled = true; }
            else if (w is RiskGame.MainWindow)
            { (w as RiskGame.MainWindow).Music_enabled = true; }
            else if (w is RiskGame.GameWindow)
            { (w as RiskGame.GameWindow).Music_enabled = true; }
            else if (w is RiskGame.Windows.Highscores)
            { (w as RiskGame.Windows.Highscores).Music_enabled = true; }
        }
        private void UpdateMediaText(MediaElement sender, Label l)
        {
            l.Content = sender.Source.ToString().Substring(30);
        }
        private void UpdateMediaText(Window window, object sender)
        {
            try
            {
                UpdateMediaText((MediaElement)sender, (Label)window.FindName("lblMediaDetails"));
            }
            catch (Exception) { }
        }
        private MediaElement RetrieveMediaPlayer()
        {
            Window window = RetrieveMainWindow();
            return RetrieveMediaPlayer(window);
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
            ChangeMedia(mediaplayer);
        }
        private void ChangeMedia(MediaElement sender)
        {
            sender.Source = Music.sources[Music.MusicIndex];
            sender.Play();
        }
    }
}
