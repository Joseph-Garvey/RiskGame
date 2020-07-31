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
using RiskGame.Game;
using RiskGame.Windows;

namespace RiskGame
{
    public partial class MainWindow : Window
    {
        #region Variables and Properties
        private List<Player> players;  // Used to store the list of currently logged in players
        private Human player;  // Stores the user's music preference.
        private bool music_enabled; // Stores the user's music preference.
        public bool Music_enabled // Accessor for the music enabled variable.
        {
            get => music_enabled;
            set
            {
                if (players.Count != 0) // if players are logged in
                {
                    try
                    {
                        ((Human)players[0]).music_enabled = value; // save the user's preferences to the file
                        Human.Update(players[0] as Human);
                    }
                    catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); } // Output an error if the preference did not save
                }
                if (value == true) { mediaplayer.Play(); } // If music is enabled, start playback.
                else if (value == false) { mediaplayer.Pause(); } // If music is disabled, stop playback.
                music_enabled = value; // Set the value
            }
        }
        private bool hints_enabled; // Stores the user's hints preference.
        public bool Hints_enabled // Accessor for the hints enabled variable.
        {
            get => hints_enabled;
            set
            {
                if (players.Count != 0) // If players are logged in
                {
                    try
                    {
                        ((Human)players[0]).hints_enabled = value; // Save the preference
                        Human.Update(players[0] as Human);
                    }
                    catch { DispErrorMsg("An error has occurred. Your music preferences have not been saved."); }
                }
                hints_enabled = value; // Set the value
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Constructor used when a player is not logged in
        /// </summary>
        /// <param name="_musicenabled">Player's music preference</param>
        /// <param name="_hintsenabled">Player's hints preference</param>
        /// <param name="_fullscreen">Player's full-screen preference</param>
        public MainWindow(bool _music_enabled, bool _hints_enabled, bool fullscreen)
        {
            players = new List<Player>(); // INstantiates player list
            SetupWindow(_music_enabled, _hints_enabled, fullscreen); // Setup the window.
        }
        /// <summary>
        /// Constructor used when (a) player(s) is/are logged in.
        /// </summary>
        /// <param name="_players">The list of currently logged in players.</param>
        /// <param name="fullscreen">The player's full-screen preference</param>
        public MainWindow(List<Player> _players, bool fullscreen)
        {
            players = _players; // Set player list
            SetupWindow(((Human)players[0]).music_enabled, ((Human)players[0]).hints_enabled, fullscreen); // Setup window with user's saved preferences
        }
        #endregion
        #region Methods
        /// <summary>
        /// Contains code shared between constructors.
        /// Sets up events, UI and data-binding
        /// </summary>
        /// <param name="_musicenabled">Player's music preference</param>
        /// <param name="_hintsenabled">Player's hints preference</param>
        /// <param name="fullscreen">Player's full-screen preference</param>
        private void SetupWindow(bool _musicenabled, bool _hintsenabled, bool fullscreen)
        {
            InitializeComponent(); // Setup UI
            GameManager.ClearEmptyFile(); // Clear empty game-save files.
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged); // Add Statechanged event
            this.DataContext = this; // Setup binding
            // Set preferences and music //
            Music_enabled = _musicenabled;
            Hints_enabled = _hintsenabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            if (fullscreen)
            {
                // Make window fullscreen //
                ((App)Application.Current).ChangeWindowState(this);
                ((App)Application.Current).Window_StateChanged(this);
            }
        }
        /// <summary>
        /// Displays given string as an error on-screen.
        /// </summary>
        /// <param name="Message">The output error message.</param>
        private void DispErrorMsg(String message)
        {
            txtError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Displays given string as an green message on-screen.
        /// </summary>
        /// <param name="Message">The output message.</param>
        private void DispSuccessMsg(String message)
        {
            // Shows message on successful registration //
            txtSuccess.Text = message;
            lblSuccess.Visibility = Visibility.Visible;
        }
        #endregion
        #region Events
        /// <summary>
        /// Attempts to Sign-in the user with the details they enter.
        /// </summary>
        private void Login(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed; // Hide previous error
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
                if (players.Contains(player)) // If player already signed in
                {
                    DispErrorMsg("This player is already signed in"); // Output error
                    return;
                }
                players.Add(player); // Add player to list
                // Shows setup window and closes Login/Registration
                GameSetup Setup = new GameSetup(players, ((App)Application.Current).RetrieveWindowState(this));
                App.Current.MainWindow = Setup;
                this.Close();
                Setup.Show();
            }
            // Error output //
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
        /// <summary>
        /// Attempts to register a user account with the provided details.
        /// Verifies account details before registering.
        /// Automatically signs user in if registration successful.
        /// </summary>
        private void Register(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed; // Hide previous output
                lblSuccess.Visibility = Visibility.Collapsed;
                if (txtRegName.Text == null || txtRegPass.Password == null || txtRegPassConf.Password == null ||
                    txtRegName.Text == "" || txtRegPass.Password == "" || txtRegPassConf.Password == "")
                    { throw new ArgumentNullException(); } // If field empty alert user
                if (txtRegPass.Password == txtRegPassConf.Password) // If passwords match,
                {
                    Human.Register(txtRegName.Text, txtRegPass.Password, music_enabled, hints_enabled); // Attempt to register the user
                    DispSuccessMsg("Registration successful. Click login to continue."); // Alert user
                    txtLogName.Text = txtRegName.Text; // Set field for login
                    txtLogPass.Password = txtRegPass.Password;
                    Login(sender, e); // Sign-in the user
                }
                else { DispErrorMsg("Passwords do not match"); }
            }
            catch (AccountCreationException K)
            {
                DispErrorMsg(K.error); // Alert user that their details did not meet criteria.
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
        /// <summary>
        /// Checks key-presses while registering.
        /// If enter pressed, registers user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The pressed key.</param>
        private void RegisterKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return) { Register(sender, e); }
        }
        /// <summary>
        /// Checks key-presses while logging in.
        /// If enter pressed, signs-in user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The pressed key.</param>
        private void LoginKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) { Login(sender, e); }
        }
        /// <summary>
        /// Toggles password text visibility.
        /// </summary>
        private void ShowHidePassword(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender; // The clicked checkbox
            PasswordBox passwordBox = new PasswordBox(); // The adjacent password-box
            TextBox textBox = new TextBox(); // The adjacent text-box
            switch (checkBox.Name)
            {
                // Match the clicked checkbox to its adjacent text-box and password-box
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
            if (checkBox.IsChecked == true) // If the checkbox is "Show"
            {
                textBox.Text = passwordBox.Password; // Set text
                passwordBox.Visibility = Visibility.Collapsed; // Hide password-box
                textBox.Visibility = Visibility.Visible; // Show text-box
            }
            else
            {
                passwordBox.Password = textBox.Text; // Set text
                passwordBox.Visibility = Visibility.Visible; // Hide password-box
                textBox.Visibility = Visibility.Collapsed; // Show text-box
            }
        }
        /// <summary>
        /// Opens the high-scores window with the user's preferences.
        /// </summary>
        private void Leaderboard(object sender, RoutedEventArgs e)
        {
            Highscores highscores = new Highscores(Music_enabled, Hints_enabled, ((App)Application.Current).RetrieveWindowState(this));
            if(players.Count > 0) // If players are signed in, call the alternate constructor.
            {
                highscores = new Highscores(players, ((App)Application.Current).RetrieveWindowState(this));
            }
            // Show the new window and close this window.
            App.Current.MainWindow = highscores;
            this.Close();
            highscores.Show();
        }
        /// <summary>
        /// Creates a new change password window if one is not already open.
        /// </summary>
        private void ChangePassword(object sender, RoutedEventArgs e)
        {
            bool open = false;
            foreach (Window w in Application.Current.Windows) // Check each open window
            {
                if (w is ChangePassword) { open = true; break; } // If the open window is a change password window, exit.
            }
            if (!open) // If a window is not already open
            {
                ChangePassword window;
                if (txtLogName.Text != null) // If the user has entered a username into the login field
                {
                    window = new ChangePassword(txtLogName.Text); // Call the alternate constructor to pass the username to the new window.
                }
                else { window = new ChangePassword(); } // Else call the default
                window.Show();
            }
        }
        /// <summary>
        /// Closes the change password window when the window is closing,
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach(Window w in App.Current.Windows) // Searches for open change password windows and closes them
            {
                if(w is ChangePassword) { w.Close(); }
            }
        }
        #endregion
    }
}
