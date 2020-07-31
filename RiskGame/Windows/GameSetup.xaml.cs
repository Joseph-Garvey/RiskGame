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
using System.Collections.ObjectModel;
using RiskGame.enemyAI;
using RiskGame.Game;
using RiskGame.Windows;
using RiskGame.CustomExceptions;

namespace RiskGame
{
    public partial class GameSetup : Window
    {
        #region Variables and Properties
        private List<Player> players = new List<Player>();  // Used to store the list of currently logged in players
        // List of colours players can choose from //
        private List<SolidColorBrush> playercolours = new List<SolidColorBrush>() { Brushes.OrangeRed, Brushes.DeepSkyBlue, Brushes.LimeGreen, Brushes.Gold, Brushes.Red, Brushes.Violet, Brushes.Blue };
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
        private bool timer_enabled; // Stores if the new game will have a turn timer
        public bool Timer_enabled // Accessor for the timer enabled bool.
        {
            get { return timer_enabled; }
            set
            {
                if(value == false) // If timer is disabled,
                {
                    sldTime.Minimum = 0; // Set the timer value to 0
                    sldTime.Value = 0;
                    panel_Time.Visibility = Visibility.Collapsed; // Hide the timer adjustment panel
                }
                else
                {
                    sldTime.Minimum = 20; // If the timer is enabled, set the minimum and default value
                    sldTime.Value = 30;
                    panel_Time.Visibility = Visibility.Visible; // SHow the timer adjustment panel
                }
                timer_enabled = value; // set value
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Window constructor, sets up events, UI and shows list of players in player panel.
        /// </summary>
        /// <param name="_players">The list of currently logged in players.</param>
        /// <param name="fullscreen">The player's fullscreen preference</param>
        public GameSetup(List<Player> _players, bool fullscreen)
        {
            InitializeComponent(); // UI setup
            players = _players; // set list of players
            // StateChanged Event Handler //
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            // Binding setup //
            this.DataContext = this;
            // Timer Setup //
            timer_enabled = true;
            // Music and Hints Setup //
            Music_enabled = ((Human)players[0]).music_enabled;
            Hints_enabled = ((Human)players[0]).hints_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            // Retrieves list of games //
            List<Human> humanusers = new List<Human>();
            foreach(Player p in players) // Create a list of human users
            {
                if(p is Human) { humanusers.Add(p as Human); }
            }
            ObservableCollection<GameDetails> loadedgames = GameManager.RetrieveGames(humanusers); // Retrieve games where the primary user is a currently logged in human.
            if (loadedgames == null || loadedgames.Count == 0) { // If 0 games saved,
                panel_LoadGame.Visibility = Visibility.Collapsed; // Hide load-game panel
                btnNewGameSettings.Visibility = Visibility.Visible; // Show alternate settings button
                btnNewGame.Width = 150; // Set the new game button width to accommodate the additional settings button
            }
            GameList.ItemsSource = loadedgames; // Sets data-binding of game list to the list of loadable games.
            // Updates UI with details of currently logged in players, showing new "Player Panels" as required.
            // For each player make their item visible, give them a colour and set the content of the add player row.
            lblPlayer1.Content = players[0].Username;
            if(players.Count >= 2)
            {
                panelPlayer2.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer2Forward);
                lblPlayer2.Content = players[1].Username;
                lblPlayer6.Content = "Player 3";
            }
            if (players.Count >= 3)
            {
                panelPlayer3.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer3Forward);
                lblPlayer3.Content = players[2].Username;
                lblPlayer6.Content = "Player 4";
            }
            if (players.Count >= 4)
            {
                panelPlayer4.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer4Forward);
                lblPlayer4.Content = players[3].Username;
                lblPlayer6.Content = "Player 5";
            }
            if (players.Count >= 5)
            {
                panelPlayer5.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer5Forward);
                lblPlayer5.Content = players[4].Username;
                lblPlayer6.Content = "Player 6";
            }
            if (players.Count >= 6) // If 6 players are logged in, collapse the combo-box and disable the add player button
            {
                panelPlayer6.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer6Forward);
                lblPlayer6.Content = players[5].Username;
                lblPlayer6Type.Visibility = Visibility.Visible;
                cmbPlayer6.Visibility = Visibility.Collapsed;
                btnAddPlayer.IsEnabled = false;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Cycles the player colour forward or backward.
        /// </summary>
        private void CyclePlayerColours(object sender, RoutedEventArgs e) { CyclePlayerColours(sender); }
        /// <summary>
        /// Redirects the user to the Login / Register window to add a new player.
        /// Closes the current window.
        /// </summary>
        private void AddPlayer(object sender, RoutedEventArgs e)
        {
            if(cmbPlayer6.SelectedIndex == 1) { // If AI selected
                // create AI player
                // add to list
                // update UI
                DispErrorMsg("AI is not yet implemented. Please select a human player.");
            } // AI is not yet implemented and thus is not an option.
            else if(cmbPlayer6.SelectedIndex == 0) // If human selected
            {
                // Open a new Login / Register window
                MainWindow newLogin = new MainWindow(players, ((App)Application.Current).RetrieveWindowState(this));
                App.Current.MainWindow = newLogin;
                this.Close(); // Close this window
                newLogin.Show(); // Show the new window
            }
        }
        /// <summary>
        /// Creates a new game with the selected options.
        /// </summary>
        private void New_Game(object sender, RoutedEventArgs e)
        {
            ClearError(); // Clear the previous error
            if (players.Count >= 2) // If there are 2 or more players,
            {
                if (players.Count == 2) // If there are only 2 players
                {
                    players.Add(new NeutralAI("Neutral")); // Add a neutral AI
                    CyclePlayerColours(btnPlayer3Forward); // Assign it a colour.
                }
                if (cmbMap.SelectedIndex != -1) // If the player has selected a map
                {
                    if(cmbGameMode.SelectedIndex == -1) // If the player has not selected a game-mode
                    {
                        DispErrorMsg("Please select a Gamemode."); // Alert the user
                        return;
                    }
                    // Set the player colours
                    players[0].Color = (SolidColorBrush)rectPlayer1Color.Fill;
                    players[1].Color = (SolidColorBrush)rectPlayer2Color.Fill;
                    players[2].Color = (SolidColorBrush)rectPlayer3Color.Fill;
                    if (players.Count >= 4) { players[3].Color = (SolidColorBrush)rectPlayer4Color.Fill; }
                    if (players.Count >= 5) { players[4].Color = (SolidColorBrush)rectPlayer5Color.Fill; }
                    if (players.Count >= 6) { players[5].Color = (SolidColorBrush)rectPlayer6Color.Fill; }
                    // Create a new game-window, passing the user preferences to it.
                    GameWindow Game = new GameWindow(players, chkRandomise.IsChecked.Value, (GameMap)cmbMap.SelectedIndex, (GameMode)cmbGameMode.SelectedIndex, (int)sldTime.Value, sldBias.Value);
                    try
                    {
                        // If the window did not load successfully an exception will be thrown.
                        App.Current.MainWindow = Game;
                        Game.Show(); // Show the new window
                        this.Close(); // Close this window
                    }
                    catch (Exception) { DispErrorMsg("An error has occurred"); } // A message box alerts the user if a map did not load correctly.
                }
                else { DispErrorMsg("Please select a map."); }
            }
            else { DispErrorMsg("There must be at least two players to start a game."); } // Alert the user
        }
        /// <summary>
        /// Loads a previous saved game.
        /// Opens a new game window with the selected game.
        /// </summary>
        private void Load_Game(object sender, RoutedEventArgs e)
        {
                if(txtError.Visibility == Visibility.Visible) { ClearError(); } // Clear previous error
                GameManager game = new GameManager();
                try
                {
                    game = GameManager.LoadGame(int.Parse(((GameDetails)GameList.SelectedItem).GameID)); // Retrieve game based on gameID
                }
                catch (GameNotFoundException) // If game not found alert user
                {
                    DispErrorMsg("The selected game could not be loaded.");
                }
                catch (NullReferenceException) // If game not selected
                {
                    DispErrorMsg("Please select a game to load by clicking on the details of the game you wish to load and then 'Load Game'");
                }
                catch (Exception)
                {
                    DispErrorMsg("Something went wrong.");
                }
                // Creates a new GameWindow, sending the GameManager containing the game details to the GameWindow. Closes window on completion.
                GameWindow Game = new GameWindow(game) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            try
            {
                App.Current.MainWindow = Game;
                Game.Show();
                this.Close();
            }
            catch (Exception) { DispErrorMsg("An error has occurred"); } // If error occurs
        }
        #endregion

        #region Methods
        /// <summary>
        /// Cycles the selected player's colour through the available colours.
        /// </summary>
        /// <param name="sender">The button clicked.</param>
        private void CyclePlayerColours(object sender)
        {
            Button btnClicked = (Button)sender; // The button clicked
            Rectangle R; // Rectangle to change
            bool colortaken = false; // bool storing if the selected colour is in use by another player
            do
            {
                colortaken = false;
                // Identify which button was clicked and therfore which rectangle to change
                if (btnClicked.Name.Contains("1")) { R = rectPlayer1Color; }
                else if (btnClicked.Name.Contains("2")) { R = rectPlayer2Color; }
                else if (btnClicked.Name.Contains("3")) { R = rectPlayer3Color; }
                else if (btnClicked.Name.Contains("4")) { R = rectPlayer4Color; }
                else if (btnClicked.Name.Contains("5")) { R = rectPlayer5Color; }
                else R = rectPlayer6Color;
                // Identify direction the player is cycling colour
                if (btnClicked.Name.Contains("Forward"))
                {
                    try // Go to next colour in list
                    { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) + 1)]; }
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[0]; } // Unless at end of list, in which case go to first colour.
                }
                else
                {
                    try { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) - 1)]; } // Go to previous colour in list
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[(playercolours.Count - 1)]; } // Unless at start, in which case go to the last colour.
                }
                if (((R != rectPlayer1Color && R.Fill == rectPlayer1Color.Fill) || (R != rectPlayer2Color && R.Fill == rectPlayer2Color.Fill) || (R != rectPlayer3Color && R.Fill == rectPlayer3Color.Fill) || (R != rectPlayer4Color && R.Fill == rectPlayer4Color.Fill) || (R != rectPlayer5Color && R.Fill == rectPlayer5Color.Fill) || (R != rectPlayer6Color && R.Fill == rectPlayer6Color.Fill)))
                {
                    // If the colour matches that used by another player
                    colortaken = true;
                }
            } while (colortaken == true); // Repeat until a colour that is not in use is found.
        }
        /// <summary>
        /// Outputs error messages to the user
        /// </summary>
        /// <param name="Message">The output error message.</param>
        private void DispErrorMsg(String Message)
        {
            if(panel_LoadGame.Visibility == Visibility.Collapsed) // If load game is collapsed
            {
                lblErrorAlt.Visibility = Visibility.Visible; //show the error message under new game
                txtErrorAlt.Text = Message; // Set text
            }
            else
            {
                lblError.Visibility = Visibility.Visible; // otherwise use the error message under load game
                txtError.Text = Message; // set text
            }
        }
        /// <summary>
        /// Clear the previous error and hide error panel
        /// </summary>
        private void ClearError()
        {
            if (panel_LoadGame.Visibility == Visibility.Collapsed) // If load game is not visible
            {
                lblErrorAlt.Visibility = Visibility.Collapsed; // Collapse the error message under new game
                txtErrorAlt.Text = ""; // Clear text
            }
            else
            {
                lblError.Visibility = Visibility.Collapsed; // Collapse the error message under load game
                txtError.Text = ""; // Clear text
            }
        }
        #endregion
    }
}
