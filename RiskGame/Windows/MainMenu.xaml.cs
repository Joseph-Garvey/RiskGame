using System;
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
using RiskGame.CustomExceptions.Game;
using RiskGame.Game;
using RiskGame.Windows;

namespace RiskGame
{
    public partial class MainWindow : Window
    {
        // Variables //
        private List<Player> players;
        private Human player;
        private int musicIndex;
        private int MusicIndex
        {
            get { return musicIndex; }
            set
            {
                if(value >= Music.sources.Count)
                {
                    musicIndex = 0;
                }
                else if(value <= 0) { musicIndex = Music.sources.Count - 1; }
                else { musicIndex = value; }
            }
        }
        private bool music_enabled;
        public bool Music_enabled { get => music_enabled; set => music_enabled = value; }

        // Constructors //
        public MainWindow()
        {
            // Called on first launch
            // Clears empty save files to prevent crashes, hides the error message.
            InitializeComponent();
            GameManager.ClearEmptyFile();
            players = new List<Player>();
            music_enabled = true;
            SetupWindow();
        }
        public MainWindow(List<Player> _players)
        {
            // Called when adding a new player via gamesetup window.
            InitializeComponent();
            players = _players;
            music_enabled = ((Human)players[0]).music_enabled;
            SetupWindow();
        }
        private void SetupWindow()
        {
            this.DataContext = this;
            MusicIndex = 1;
            if (music_enabled) { mediaplayer.Play(); }
        }
        // Methods //
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
                lblSuccess.Visibility = Visibility.Hidden;
                // DEV OPTIONS // to be removed before publish // used for quick testing
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
                lblSuccess.Visibility = Visibility.Hidden;
                // If passwords match, attempt to register the player.
                if(txtRegPass.Password == txtRegPassConf.Password)
                {
                    Human.Register(txtRegName.Text, txtRegPass.Password, music_enabled); // Ensure details are valid, username is not taken and write details to file.
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
        private void ShowPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            switch (textBox.Name)
            {
                case "txtRegPassShow":
                    txtRegPass.Password = textBox.Text;
                    break;
                case "txtRegPassConfShow":
                    txtRegPassConf.Password = textBox.Text;
                    break;
                case "txtLogPassShow":
                    txtLogPass.Password = textBox.Text;
                    break;
            }
        }

        // Clear password on keyboard focus to prevent user error and/or copying password.
        private void ClearPwdText(object sender, KeyboardFocusChangedEventArgs e)
        {
            PasswordBox P = (PasswordBox)sender;
            P.Password = "";
        }

        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores(players);
            App.Current.MainWindow = highscores;
            this.Close();
            highscores.Show();
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e) { mediaplayer.Volume = (double)slider_Volume.Value; }

        private void MediaBack(object sender, RoutedEventArgs e)
        {
            MusicIndex -= 1;
            ChangeMedia();
        }
        private void MediaForward(object sender, RoutedEventArgs e)
        {
            MusicIndex += 1;
            ChangeMedia();
        }
        private void ChangeMedia()
        {
            mediaplayer.Source = Music.sources[MusicIndex];
            mediaplayer.Play();
        }
        private void MediaPause(object sender, RoutedEventArgs e) { mediaplayer.Pause(); }
        private void MediaPlay(object sender, RoutedEventArgs e) { mediaplayer.Play(); }
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaForward(sender, e);
        }

        private void Settings(object sender, RoutedEventArgs e) { Settings(); }
        private void Return(object sender, RoutedEventArgs e) { Return(); }
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11) { ChangeWindowState(); }
            if(e.Key == Key.Escape)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    ChangeWindowState();
                }
                else
                {
                    if(panel_MainUI.Visibility == Visibility.Visible)
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

        private void Music_EnableDisable(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateMediaText(object sender, RoutedEventArgs e)
        {
            lblMediaDetails.Content = mediaplayer.Source.ToString().Substring(30);
        }
    }
}
