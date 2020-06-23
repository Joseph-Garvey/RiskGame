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

namespace RiskGame
{
    public partial class GameSetup : Window
    {
        // Variables //
        private List<Player> players = new List<Player>();
        private List<SolidColorBrush> playercolours = new List<SolidColorBrush>() { Brushes.OrangeRed, Brushes.DeepSkyBlue, Brushes.LimeGreen, Brushes.Gold, Brushes.Red, Brushes.Violet, Brushes.Blue };
        private bool music_enabled;
        public bool Music_enabled
        {
            get => music_enabled;
            set
            {

                if (players.Count != 0)
                {
                    try
                    {
                        ((Human)players[0]).music_enabled = value;
                        Human.Update(players[0] as Human);
                    }
                    catch { ShowError("An error has occurred. Your music preferences have not been saved."); }
                }
                if (value == true) { mediaplayer.Play(); }
                else if (value == false) { mediaplayer.Pause(); }
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
                    catch { ShowError("An error has occurred. Your music preferences have not been saved."); }
                }
                hints_enabled = value;
            }
        }

        // Constructor //
        public GameSetup(List<Player> _players)
        {
            // Takes in the list of players from Login/Register.
            InitializeComponent();
            players = _players;
            // Binding setup //
            this.DataContext = this;
            // Music Setup //
            music_enabled = ((Human)players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            // Retrieves list of games //
            GameList.ItemsSource = GameManager.RetrieveGames();
            // Updates UI with details of currently logged in players, showing new "Player Panels" as required.
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
            if (players.Count >= 6)
            {
                panelPlayer6.Visibility = Visibility.Visible;
                CyclePlayerColours(btnPlayer6Forward);
                lblPlayer6.Content = players[5].Username;
                lblPlayer6Type.Visibility = Visibility.Visible;
                cmbPlayer6.Visibility = Visibility.Collapsed;
                btnAddPlayer.IsEnabled = false;
            }
        }
        /////////////////////////////////////////////////////////

        // UI Buttons //
        private void CyclePlayerColours(object sender, RoutedEventArgs e)
        {
            /// Cycles the player's colour forward and back, allowing the payer to choose any colour from the list - except for those already in use by another player. //
            Button btnClicked = (Button)sender;
            Rectangle R;
            bool colortaken = false;
            do
            {
                colortaken = false;
                // Determines which UI Element to change //
                if (btnClicked.Name.Contains("1")) { R = rectPlayer1Color; }
                else if (btnClicked.Name.Contains("2")) { R = rectPlayer2Color; }
                else if (btnClicked.Name.Contains("3")) { R = rectPlayer3Color; }
                else if (btnClicked.Name.Contains("4")) { R = rectPlayer4Color; }
                else if (btnClicked.Name.Contains("5")) { R = rectPlayer5Color; }
                else R = rectPlayer6Color;
                // Is player cycling forward or back? based on name of the button clicked.
                if (btnClicked.Name.Contains("Forward"))
                {
                    // cycles forward and loops if at end of list.
                    try
                    { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) + 1)]; }
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[0]; }
                }
                else {
                    // cycles backwards and brings to end if at end of list.
                    try { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) - 1)]; }
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[(playercolours.Count - 1)]; }
                }
                if( ((R != rectPlayer1Color && R.Fill == rectPlayer1Color.Fill) || (R != rectPlayer2Color && R.Fill == rectPlayer2Color.Fill) || (R != rectPlayer3Color && R.Fill == rectPlayer3Color.Fill) || (R != rectPlayer4Color && R.Fill == rectPlayer4Color.Fill) || (R != rectPlayer5Color && R.Fill == rectPlayer5Color.Fill) || (R != rectPlayer6Color && R.Fill == rectPlayer6Color.Fill)))
                {
                    // If the colour is already in use by another player or by self, loop script to cycle to next colour in list.
                    colortaken = true;
                }
            } while (colortaken == true);
        }

        // overloaded script allows the same method to be executed from the code behind when new players are added.
        private void CyclePlayerColours(object sender)
        {
            Button btnClicked = (Button)sender;
            Rectangle R;
            bool colortaken = false;
            do
            {
                colortaken = false;
                if (btnClicked.Name.Contains("1")) { R = rectPlayer1Color; }
                else if (btnClicked.Name.Contains("2")) { R = rectPlayer2Color; }
                else if (btnClicked.Name.Contains("3")) { R = rectPlayer3Color; }
                else if (btnClicked.Name.Contains("4")) { R = rectPlayer4Color; }
                else if (btnClicked.Name.Contains("5")) { R = rectPlayer5Color; }
                else R = rectPlayer6Color;
                if (btnClicked.Name.Contains("Forward"))
                {
                    try
                    { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) + 1)]; }
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[0]; }
                }
                else
                {
                    try { R.Fill = playercolours[(playercolours.IndexOf((SolidColorBrush)R.Fill) - 1)]; }
                    catch (ArgumentOutOfRangeException) { R.Fill = playercolours[(playercolours.Count - 1)]; }
                }
                if (((R != rectPlayer1Color && R.Fill == rectPlayer1Color.Fill) || (R != rectPlayer2Color && R.Fill == rectPlayer2Color.Fill) || (R != rectPlayer3Color && R.Fill == rectPlayer3Color.Fill) || (R != rectPlayer4Color && R.Fill == rectPlayer4Color.Fill) || (R != rectPlayer5Color && R.Fill == rectPlayer5Color.Fill) || (R != rectPlayer6Color && R.Fill == rectPlayer6Color.Fill)))
                {
                    colortaken = true;
                }
            } while (colortaken == true);
        }
        ////////////////////////////////////////////////////////////
        /// if + button clicked forward the list of players to a new Login Menu and close this window.
        private void AddPlayer(object sender, RoutedEventArgs e)
        {
            if(cmbPlayer6.SelectedIndex == 1) { ShowError("AI is not yet implemented. Please select a human player."); } // AI is not yet implemented and thus is not an option.
            else if(cmbPlayer6.SelectedIndex == 0)
            {
                // Only if selecting human, direct to Login Screen so that human can log in.
                MainWindow newLogin = new MainWindow(players);
                App.Current.MainWindow = newLogin;
                this.Close();
                newLogin.Show();
            }
        }
        private void New_Game(object sender, RoutedEventArgs e)
        {
            ClearError();
            if (players.Count >= 2)
            {
                if(cmbMap.SelectedIndex == 0)
                {
                    switch (cmbGameMode.SelectedIndex)
                    {
                        case -1:
                            ShowError("Please select a Gamemode.");
                            break;
                        case 0:
                            ShowError("Classic Risk has not yet been implemented.");
                            break;
                        case 1:
                            players[0].Color = (SolidColorBrush)rectPlayer1Color.Fill;
                            players[1].Color = (SolidColorBrush)rectPlayer2Color.Fill;
                            if (players.Count >= 3) { players[2].Color = (SolidColorBrush)rectPlayer3Color.Fill; }
                            if (players.Count >= 4) { players[3].Color = (SolidColorBrush)rectPlayer4Color.Fill; }
                            if (players.Count >= 5) { players[4].Color = (SolidColorBrush)rectPlayer5Color.Fill; }
                            if (players.Count >= 6) { players[5].Color = (SolidColorBrush)rectPlayer6Color.Fill; }
                            GameWindow Game = new GameWindow(players, chkRandomise.IsChecked.Value) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                            App.Current.MainWindow = Game;
                            this.Close();
                            Game.Show();
                            break;
                    }
                }
                else { ShowError("Please select a map."); }
            }
            else { ShowError("There must be at least two players to start a game."); }
        }

        //////////////////////////////////////////////////////////
        // File Management ///
        private void Load_Game(object sender, RoutedEventArgs e)
        {
            // Gets the game ID of the selected game and loads the game with that gameID frm file.
            try
            {
                if(txtError.Visibility == Visibility.Visible) { ClearError(); }
                GameManager game = GameManager.LoadGame(int.Parse(((GameDetails)GameList.SelectedItem).GameID));
                // Creates a new GameWindow, sending the GameManager containing the game details to the GameWindow. Closes window on completion.
                GameWindow Game = new GameWindow(game) { WindowStartupLocation = WindowStartupLocation.CenterScreen };
                App.Current.MainWindow = Game;
                this.Close();
                Game.Show();
            }
            catch (NullReferenceException)
            {
                ShowError("Please select a game to load by clicking on the details of the game you wish to load and then 'Load Game'");
            }
            catch(Exception)
            {
                ShowError("Something went wrong.");
            }
        }

        ///////////////////////////////////////////////////////////
        /// Error message handling, shows error message when exception thrown.
        private void ShowError(String Message)
        {
            txtError.Visibility = Visibility.Visible;
            txtError.Content = Message;
        }
        private void ClearError()
        {
            txtError.Visibility = Visibility.Collapsed;
            txtError.Content = "";
        }

        // Media Controls //
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e) { mediaplayer.Volume = (double)slider_Volume.Value; }
        private void MediaBack(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex -= 1;
            ChangeMedia();
        }
        private void MediaForward(object sender, RoutedEventArgs e)
        {
            Music.MusicIndex += 1;
            ChangeMedia();
        }
        private void ChangeMedia()
        {
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            mediaplayer.Play();
        }
        private void MediaPause(object sender, RoutedEventArgs e) { mediaplayer.Pause(); }
        private void MediaPlay(object sender, RoutedEventArgs e) { mediaplayer.Play(); }
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaForward(sender, e);
        }
        private void UpdateMediaText(object sender, RoutedEventArgs e)
        {
            lblMediaDetails.Content = mediaplayer.Source.ToString().Substring(30);
        }

        // Navigate to and from settings menu //
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
        private void Tutorial_Window(object sender, RoutedEventArgs e)
        {
            Tutorial tutorial = new Tutorial();
            App.Current.MainWindow = tutorial;
            tutorial.Show();
        }
    }
}
