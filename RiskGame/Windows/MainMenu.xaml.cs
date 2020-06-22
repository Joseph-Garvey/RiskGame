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
        List<Player> players;
        Human player;

        // Constructors //
        public MainWindow()
        {
            // Called on first launch
            // Clears empty save files to prevent crashes, hides the error message.
            InitializeComponent();
            GameManager.ClearEmptyFile();
            players = new List<Player>();
        }
        public MainWindow(List<Player> _players)
        {
            // Called when adding a new player via gamesetup window.
            InitializeComponent();
            players = _players;
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
            // Signs player in when Login button is clicked //
                // Checks entered details against those on file, retrieves the player's details, returning a player object.
                player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
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
                    Human.Register(txtRegName.Text, txtRegPass.Password); // Ensure details are valid, username is not taken and write details to file.
                    DispSuccessMsg("Registration successful. Click login to continue.");
                    txtLogName.Text = txtRegName.Text;
                    txtLogPass.Password = txtRegPass.Password;
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


        //        try
        //{
        //    lblError.Visibility = Visibility.Collapsed;
        //    // DEV OPTIONS // to be removed before publish // used for quick testing
        //    if((String)((Button) sender).Content == "DEVUSER")
        //    { txtLogName.Text = "Example"; txtLogPass.Password = "P@ssword123";
        //        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
        //        players.Add(player);
        //        txtLogName.Text = "SeanF"; txtLogPass.Password = "P@ssword1";
        //        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
        //        players.Add(player);
        //        txtLogName.Text = "HarveyD"; txtLogPass.Password = "Belf@st1";
        //        player = Human.SignIn(txtLogName.Text, txtLogPass.Password);
        //        players.Add(player);
        //        txtLogName.Text = "BrandesTom"; txtLogPass.Password = "Cork1234%";
        //    }
    }
}
