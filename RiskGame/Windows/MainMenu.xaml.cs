﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RiskGame.CustomExceptions;
using RiskGame.Game;
using RiskGame.Windows;

namespace RiskGame
{
    public partial class MainWindow : Window
    {
        // Variables //
        private List<Player> players;
        private Human player;
        private bool music_enabled;
        public bool Music_enabled
            {
                get => music_enabled;
                set
                {

                    if(players.Count != 0)
                    {
                        try
                        {
                            ((Human)players[0]).music_enabled = value;
                            Human.Update(players[0] as Human);
                        }
                        catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                    }
                    if(value == true) { mediaplayer.Play(); }
                    else if(value == false) { mediaplayer.Pause(); }
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
                    catch { DispErrorMsg("An error has occurred. Your hint preferences have not been saved."); }
                }
                hints_enabled = value;
            }
        }

        // Constructors //
        public MainWindow(bool _music_enabled, bool _hints_enabled, bool fullscreen)
        {
            players = new List<Player>();
            SetupWindow(_music_enabled, _hints_enabled, fullscreen);
        }
        public MainWindow(List<Player> _players, bool fullscreen)
        {
            // Called when adding a new player via gamesetup window.
            players = _players;
            SetupWindow(((Human)players[0]).music_enabled, ((Human)players[0]).hints_enabled, fullscreen);
        }
        // Methods //
        private void SetupWindow(bool _musicenabled, bool _hintsenabled, bool fullscreen)
        {
            InitializeComponent();
            GameManager.ClearEmptyFile();
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            this.DataContext = this;
            Music_enabled = _musicenabled;
            Hints_enabled = _hintsenabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            if (fullscreen)
            {
                ((App)Application.Current).ChangeWindowState(this);
                ((App)Application.Current).Window_StateChanged(this);
            }
        }
        /// Message management ///
        private void DispErrorMsg(String message)
        {
            // Shows error box with message //
            txtError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
        private void DispSuccessMsg(String message)
        {
            // Shows message on successful registration //
            txtSuccess.Text = message;
            lblSuccess.Visibility = Visibility.Visible;
        }

        // Event calls //
        private void Login(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed;
                // DEV OPTIONS // to be removed before publish // used for quick testing
                if(sender is Button)
                {
                    if ((String)((Button)sender).Content == "Admin")
                    {
                        txtLogName.Text = "Example"; txtLogPass.Password = "P@ssword123";
                        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
                        players.Add(player);
                        txtLogName.Text = "SeanF"; txtLogPass.Password = "P@ssword1";
                        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
                        players.Add(player);
                        txtLogName.Text = "HarveyD"; txtLogPass.Password = "Belf@st1";
                        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
                        players.Add(player);
                        txtLogName.Text = "BrandesTom"; txtLogPass.Password = "Cork1234%";
                    }
                }
                // Signs player in when Login button is clicked //
                // Checks entered details against those on file, retrieves the player's details, returning a player object.
                player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
                if (players.Contains(player))
                {
                    DispErrorMsg("This player is already signed in");
                    return;
                }
                players.Add(player);
                // Shows setup window and closes Login/Registration
                GameSetup Setup = new GameSetup(players);
                App.Current.MainWindow = Setup;
                this.Close();
                Setup.Show();
            }
            // Exception Handling //
            catch (ArgumentNullException)
            {
                DispErrorMsg("Please enter a username and password.");
            }
            catch (AccountNotFoundException K)
            {
                DispErrorMsg(K.Message);
            }
            catch (LoginException L)
            {
                DispErrorMsg(L.Message);
            }
            catch (IOException)
            {
                DispErrorMsg("An error reading or writing from the file has occurred. Please ensure you have registered an account or delete the Usersaves.txt file in the game directory.");
            }
            catch(Exception)
            {
                DispErrorMsg("An unknown error has occurred.");
            }
        }
        private void Register(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed;
                lblSuccess.Visibility = Visibility.Collapsed;
                if (txtRegName.Text == null || txtRegPass.Password == null || txtRegPassConf.Password == null ||
                    txtRegName.Text == "" || txtRegPass.Password == "" || txtRegPassConf.Password == "")
                    { throw new ArgumentNullException(); }
                // If passwords match, attempt to register the player.
                if (txtRegPass.Password == txtRegPassConf.Password)
                {
                    Human.Register(txtRegName.Text, txtRegPass.Password, music_enabled, hints_enabled); // Ensure details are valid, username is not taken and write details to file.
                    DispSuccessMsg("Registration successful. Click login to continue.");
                    txtLogName.Text = txtRegName.Text;
                    txtLogPass.Password = txtRegPass.Password;
                    Login(sender, e);
                }
                else { DispErrorMsg("Passwords do not match"); }
            }
            catch (AccountCreationException K)
            {
                DispErrorMsg(K.error);
            }
            catch (IOException)
            {
                DispErrorMsg("An error reading or writing from the file has occurred. Please try again or delete the Usersaves.txt file in the game directory.");
            }
            catch (ArgumentNullException)
            {
                DispErrorMsg("Please provide an input for every field.");
            }
        }
        private void RegisterKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return) { Register(sender, e); }
        }
        private void LoginKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) { Login(sender, e); }
        }
        private void ShowHidePassword(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            PasswordBox passwordBox = new PasswordBox();
            TextBox textBox = new TextBox();
            switch (checkBox.Name)
            {
                case "chkRegPass":
                    passwordBox = txtRegPass;
                    textBox = txtRegPassShow;
                    break;
                case "chkRegPassConf":
                    passwordBox = txtRegPassConf;
                    textBox = txtRegPassConfShow;
                    break;
                case "chkLogPass":
                    passwordBox = txtLogPass;
                    textBox = txtLogPassShow;
                    break;
            }
            if (checkBox.IsChecked == true)
            {
                textBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
            }
            else
            {
                passwordBox.Password = textBox.Text;
                passwordBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;

            }
        }
        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores(Music_enabled, Hints_enabled, ((App)Application.Current).RetrieveWindowState(this));
            if(players.Count > 0)
            {
                highscores = new Highscores(players, ((App)Application.Current).RetrieveWindowState(this));
            }
            App.Current.MainWindow = highscores;
            this.Close();
            highscores.Show();
        }
        private void ChangePassword(object sender, RoutedEventArgs e)
        {
            bool open = false;
            foreach (Window w in Application.Current.Windows)
            {
                if (w is ChangePassword) { open = true; break; }
            }
            if (!open)
            {
                ChangePassword window;
                if (txtLogName.Text != null)
                {
                    window = new ChangePassword(txtLogName.Text);
                }
                else { window = new ChangePassword(); }
                window.Show();
            }
        }
        // Clear password on keyboard focus to prevent user error and/or copying password.
        private void ClearPwdText(object sender, KeyboardFocusChangedEventArgs e)
        {
            PasswordBox P = (PasswordBox)sender;
            P.Password = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(Window w in App.Current.Windows)
            {
                if(w is ChangePassword) { w.Close(); }
            }
        }
    }
}
