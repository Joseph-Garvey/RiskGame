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
using RiskGame.enemyAI;
using RiskGame.Game;
using System.Collections.ObjectModel;
using System.Threading;
using RiskGame.Windows;

namespace RiskGame
{
    // Class Extensions for threaded randomisation of territory order //
    static class Extensions
    {
        // Fisher Yates Shuffle //
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
    public static class ThreadSafeRandom
    { // C# randomisation threading library
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    ////////////////////////////////////////////////////
    ///
    /// <summary>
    /// This is the Game Window Code
    /// </summary>
    ///
    ////////////////////////////////////////////////////

    public partial class GameWindow : Window
    {
        //// Variables ////
        // these will be ported to the gamemanager object to increase efficiency when loading and saving //
        private List<Player> players;
        private List<Territory> territories;
        private Territory slctTerritory;
        private Territory nextTerritory;
        private Player currentplayer;
        private Player defendplayer;
        private GameState gameState;
        private static Random rng = new Random();
        private static int turn = 0;
        private static List<Territory> scanterritories = new List<Territory>();
        private bool action = false; // temporary measure for saving during testing.

        //// Constructors ////
        // Load Game // Finish later
        public GameWindow(GameManager game)
        {
            // not complete // check at end once all is done
            // takes local variables and matches them to the gamemanager variables to load game. (Incomplete)
            InitializeComponent();
            players = game.players;
            territories = game.territories;
            currentplayer = game.currentplayer;
            turn = game.turn;
            gameState = game.gameState;
            UISetup();
            LoadPlayerUI();
            Output("The game has loaded.");
        }
        // New Game //
        public GameWindow(List<Player> _players, bool randomise_initial)
        {
            InitializeComponent();
            players = _players;
            // Creation of Territories and Map Setup //
            List<String> links = new List<string>{ "Kamchatka", "Alberta", "Northwest_Canada" };
            Territory Alaska = new Territory("Alaska",links);
            links = new List<string>{ "Alaska", "Alberta", "Quebec","Greenland" };
            Territory Northwest_Canada = new Territory("Northwest_Canada", links);
            links = new List<string> { "Northwest_Canada","Quebec","Ontario","Iceland"};
            Territory Greenland = new Territory("Greenland", links);
            links = new List<string> {"Alaska","Northwest_Canada","Quebec","Western_US"};
            Territory Alberta = new Territory("Alberta", links);
            links = new List<string> {"Alberta","Ontario","Greenland","Northwest_Canada","Western_US","Eastern_US" };
            Territory Quebec = new Territory("Quebec", links);
            links = new List<string> {"Greenland","Quebec","Eastern_US" };
            Territory Ontario = new Territory("Ontario", links);
            links = new List<string> {"Alberta","Quebec","Eastern_US","Central_America"};
            Territory Western_US = new Territory("Western_US", links);
            links = new List<string> {"Western_US","Ontario","Central America"};
            Territory Eastern_US = new Territory("Eastern_US", links);
            links = new List<string> {"Western_US","Eastern_US","Venezuela"};
            Territory Central_America = new Territory("Central_America", links);
            links = new List<string> {"Central_America","Peru","Brazil"};
            Territory Venezuela = new Territory("Venezuela", links);
            links = new List<string> {"Venezuela","Brazil","Argentina"};
            Territory Peru = new Territory("Peru", links);
            links = new List<string> {"Venezuela","Peru","Argentina","North_Africa"};
            Territory Brazil = new Territory("Brazil", links);
            links = new List<string> {"Peru","Brazil" };
            Territory Argentina = new Territory("Argentina", links);
            links = new List<string> {"Greenland","Scandinavia","UK_Ireland" };
            Territory Iceland = new Territory("Iceland", links);
            links = new List<string> {"Iceland","Western_Europe","Northern_Europe","Scandinavia" };
            Territory UK_Ireland = new Territory("UK_Ireland", links);
            links = new List<string> {"Iceland","UK_Ireland","Northern_Europe","Soviet_Bloc" };
            Territory Scandinavia = new Territory("Scandinavia", links);
            links = new List<string> { "Scandinavia","Northern_Europe","Southern_Europe","Ural","Afghanistan","Middle_East" };
            Territory Soviet_Bloc  = new Territory( "Soviet_Bloc" , links);
            links = new List<string> {"Western_Europe","Northern_Europe","Soviet_Bloc","Middle_East","Egypt"   };
            Territory Southern_Europe = new Territory("Southern_Europe", links);
            links = new List<string> { "UK_Ireland","Scandinavia","Soviet_Bloc","Southern_Europe","Western_Europe" };
            Territory Northern_Europe = new Territory("Northern_Europe", links);
            links = new List<string> {"UK_Ireland","Northern_Europe","Southern_Europe","North_Africa" };
            Territory Western_Europe = new Territory("Western_Europe", links);
            links = new List<string> {"Brazil","Egypt","East_Africa","Central_Africa","Western_Europe" };
            Territory North_Africa = new Territory("North_Africa", links);
            links = new List<string> {"North_Africa","Southern_Europe","Middle_East","East_Africa"};
            Territory Egypt = new Territory("Egypt", links);
            links = new List<string> {"North_Africa","East_Africa","South_Africa" };
            Territory Central_Africa = new Territory("Central_Africa", links);
            links = new List<string> {"Egypt","Middle_East","Madagascar","South_Africa","Central_Africa","North_Africa" };
            Territory East_Africa = new Territory("East_Africa", links);
            links = new List<string> { "Central_Africa","East_Africa","Madagascar" };
            Territory South_Africa = new Territory("South_Africa", links);
            links = new List<string> { "South_Africa","East_Africa"};
            Territory Madagascar = new Territory("Madagascar", links);
            links = new List<string> {"Southern_Europe","Soviet_Bloc","Afghanistan","India","East_Africa","Egypt" };
            Territory Middle_East= new Territory("Middle_East", links);
            links = new List<string> {"Middle_East","Soviet_Bloc","Ural","China","India" };
            Territory Afghanistan = new Territory("Afghanistan", links);
            links = new List<string> {"Middle_East","Afghanistan","China","Southeast_Asia" };
            Territory India = new Territory("India", links);
            links = new List<string> {"India","China","Indonesia" };
            Territory Southeast_Asia = new Territory("Southeast_Asia", links);
            links = new List<string> {"Afghanistan","Ural","Siberia","Mongolia","Southeast_Asia","India" };
            Territory China = new Territory("China", links);
            links = new List<string> {"Soviet_Bloc","Siberia","China","Afghanistan" };
            Territory Ural = new Territory("Ural", links);
            links = new List<string> {"Ural","Yakutsk","Irkutsk","Mongolia","China" };
            Territory Siberia = new Territory("Siberia", links);
            links = new List<string> {"China","Siberia","Irkutsk","Kamchatka","Japan" };
            Territory Mongolia = new Territory("Mongolia", links);
            links = new List<string> {"Mongolia","Kamchatka" };
            Territory Japan = new Territory("Japan", links);
            links = new List<string> {"Siberia","Yakutsk","Kamchatka","Mongolia" };
            Territory Irkutsk = new Territory("Irkutsk", links);
            links = new List<string> {"Siberia","Kamchatka","Irkutsk" };
            Territory Yakutsk = new Territory("Yakutsk", links);
            links = new List<string> {"Yakutsk", "Alaska","Japan","Mongolia","Irkutsk"};
            Territory Kamchatka = new Territory("Kamchatka", links);
            links = new List<string> {"New_Guinea","Southeast_Asia" };
            Territory Indonesia = new Territory("Indonesia", links);
            links = new List<string> {"Indonesia","Eastern_Australia","Western_Australia"};
            Territory New_Guinea = new Territory("New_Guinea", links);
            links = new List<string> {"Eastern_Australia","New_Guinea" };
            Territory Western_Australia = new Territory("Western_Australia", links);
            links = new List<string> {"Western_Australia","New_Guinea" };
            Territory Eastern_Australia = new Territory("Eastern_Australia", links);
            territories = new List<Territory>
            {
                Alaska, Northwest_Canada,Greenland,Alberta,Quebec,Ontario,Western_US,Eastern_US,Central_America,
                Venezuela,Peru,Brazil,Argentina,
                Iceland,Scandinavia,UK_Ireland,Soviet_Bloc,Northern_Europe,Western_Europe,Southern_Europe,
                North_Africa,Egypt,East_Africa,Central_Africa,South_Africa,Madagascar,
                Indonesia,New_Guinea,Eastern_Australia,Western_Australia,
                Middle_East,Afghanistan,India,Ural,Siberia,China,Southeast_Asia,Mongolia,Irkutsk,Yakutsk,Kamchatka,Japan
            };
            SetupGame(randomise_initial);
        }

        // Game Setup and UI Management //
        private void SetupGame(bool randomise_initial)
        {
            // Determines how many armies players have.
            int initialarmies = (50 - (5 * players.Count));
            currentplayer = players[0];
            // Setup Board and initial armies //
            UISetup();
            foreach (Player p in players) { p.army_undeployed = initialarmies; }
            UpdateState(GameState.InitialArmyPlace);
            if (randomise_initial == true) { SetupRandom(); StartGame(); }
            else
            {
                Output("Place armies around the map using left click.");
                Output("You can capture any territory not already taken by another player.");
            }
        }
        private void StartGame()
        {
            Output("The Game is beginning.");
            gameState = GameState.PlacingArmy;
            NextTurn();
        }
        private void SetupRandom()
        {
            // originally tried various methods of randomising player and territory assignments to varying degrees of success.
            // However many would get stuck in loops, or to avoid loops would become overly complex.
            // I realised I could instead randomise the order of territories in the list and then assign each territory to a player methodically.
            // This achieves the same effect with far less complexity.
            territories.Shuffle();
            // Assigned initial placements of armies.
            foreach (Territory t in territories)
            {
                // ensures each territory is owned by a player.
                bool assigned = false;
                do
                {
                    CyclePlayers();
                    if (currentplayer.army_undeployed > 0)
                    {
                        Place_Reinforce(t, 1);
                        assigned = true;
                    }
                } while (assigned == false);
            }
            // Places remaining armies around map in friendly territory until there are none left //
            foreach (Player p in players)
            {
                currentplayer = p;
                while (p.army_undeployed > 0)
                {
                    foreach (Territory t in territories)
                    {
                        if (t.owner == p)
                        {
                            if(p.army_undeployed > 0) { Place_Reinforce(t, 1); } // temp fix, change to cycling territories but break on an empty cycle
                            else { break; }
                        }
                    }
                }
            }
        }
        private void UISetup()
        {
            // This code sets up the "player panel" with the players details, resizing certain elements to avoid white borders.
            // Needs values tweaked.
            lblPlayerName1.Content = players[0].Username;
            lblPlayerName2.Content = players[1].Username;
            rectPlayerColor1.Fill = (SolidColorBrush)players[0].Color;
            rectPlayerColor2.Fill = (SolidColorBrush)players[1].Color;
            Thickness th = new Thickness(0, 20, 0, 0);
            int fs = 11;
            int rect_height = 20;
            if (players.Count >= 3)
            {
                panel_Players.Margin = new Thickness(20, 0, 10, 0);
                th = new Thickness(0, 15, 0, 0);
                lblPlayerName3.Content = players[2].Username;
                rectPlayerColor3.Fill = (SolidColorBrush)players[2].Color;
                panel_Player3.Visibility = Visibility.Visible;
            }
            if (players.Count >= 4)
            {
                th = new Thickness(0, 10, 0, 0);
                lblPlayerName4.Content = players[3].Username;
                rectPlayerColor4.Fill = (SolidColorBrush)players[3].Color;
                panel_Player4.Visibility = Visibility.Visible;
            }
            if (players.Count >= 5)
            {
                rect_height = 15;
                th = new Thickness(0, 5, 0, 0);
                lblPlayerName5.Content = players[4].Username;
                rectPlayerColor5.Fill = (SolidColorBrush)players[4].Color;
                panel_Player5.Visibility = Visibility.Visible;
            }
            if (players.Count >= 6)
            {
                fs = 9;
                rect_height = 12;
                rectPlayerColor6.Fill = (SolidColorBrush)players[5].Color;
                lblPlayerName6.Content = players[5].Username;
                panel_Player6.Visibility = Visibility.Visible;
            }
            SetMargin(th);
            SetFontSize(fs);
            SetHeight(rect_height);
        }
        private void SetMargin(Thickness th)
        {
            panel_Player1.Margin = th;
            panel_Player2.Margin = th;
            panel_Player3.Margin = th;
            panel_Player4.Margin = th;
            panel_Player5.Margin = th;
            panel_Player6.Margin = th;
        }
        private void SetFontSize(int i)
        {
            foreach (UIElement L in panel_Player1.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
            foreach (UIElement L in panel_Player2.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
            foreach (UIElement L in panel_Player3.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
            foreach (UIElement L in panel_Player4.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
            foreach (UIElement L in panel_Player5.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
            foreach (UIElement L in panel_Player6.Children)
            {
                if (L is Label)
                {
                    ((Label)L).FontSize = i;
                }

            }
        }
        private void SetHeight(int i)
        {
            rectPlayerColor1.Height = 20;
            rectPlayerColor2.Height = 20;
            rectPlayerColor3.Height = 20;
            rectPlayerColor4.Height = 20;
            rectPlayerColor5.Height = 20;
            rectPlayerColor6.Height = 20;
        }

        //// Game Methods ////
        private void CyclePlayers()
        { // Cycles through the list of players, for a new turn or placing armies.
            if ((players.IndexOf(currentplayer) + 1) == (players.Count)) { currentplayer = players[0]; }
            else { currentplayer = players[(players.IndexOf(currentplayer) + 1)]; }
        }
        private bool AllPlaced()
        {
            bool allplaced = true;
            foreach(Player p in players)
            {
                if(p.army_undeployed > 0 && !(p is NeutralAI))
                {
                    allplaced = false;
                    break;
                }
            }
            return allplaced;
        }
        private void NextTurn()
        {
            ClearSelections();
            if(gameState == GameState.InitialArmyPlace)
            {
                if ((!(currentplayer is NeutralAI)) && currentplayer.army_undeployed > 0)
                {
                    Output(String.Format("It is now {0}'s turn.", currentplayer.Username));
                    UpdatePlayerPanelUI();
                }
                else
                {
                    if(currentplayer is NeutralAI) { CyclePlayers(); NextTurn(); }
                    if (AllPlaced())
                    {
                        // Neutral AI conquer
                        StartGame();
                    }
                }
            }
            else
            {
                turn += 1;
                CyclePlayers();
                currentplayer.army_undeployed += (currentplayer.territoriesowned / 3);
                UpdatePlayerPanelUI();
                UpdateState(GameState.PlacingArmy);
            }
        }
        private void Win()
        {
            int finalscore = currentplayer.score / turn;
            GameDetails game = new GameDetails(DateTime.Now.ToString(), currentplayer.Username, players.Count.ToString(), finalscore.ToString(),turn.ToString());
            GameDetails.Save(game);
            Highscores Setup = new Highscores(game);
            App.Current.MainWindow = Setup;
            this.Close();
            Setup.Show();
        }

        //// UI /////
        private void LoadPlayerUI()
        { // to be replaced by more efficient case by case update country UI
            UpdatePlayerPanelUI();
            UpdateStateUI();
            if(gameState == GameState.PlacingArmy)
            {
                UpdatePlayerUndeployed();
            }
            UpdatePlayerArmies(false);
            UpdatePlayerTerritories(false);
            foreach(Territory t in territories)
            {
                Button b = SelectButton(t.name);
                b.Background = t.owner.Color;
                b.Content = t.currentarmies;
                // if place, show temp
                // if attack show moving etc
                // put in code that allows the current action to be done
                // rn i'm temporarily going to make it so that one cannot save mid-action.
            }
        }
        private void ConquerTerritoryUI()
        {
            Button b = SelectButton(nextTerritory.name);
            b.Background = nextTerritory.owner.Color;
            b.Content = nextTerritory.temparmies;
        }
        private void AttackTerritoryUI()
        {
            Button b = SelectButton(slctTerritory.name);
            b.Content = slctTerritory.currentarmies;
        }
        private void UpdateNumOutput()
        {
            switch (gameState)
            {
                case GameState.PlacingArmy:
                    lblNumber.Content = slctTerritory.temparmies;
                    break;
                case GameState.Attacking:
                    lblNumber.Content = nextTerritory.temparmies;
                    break;
                case GameState.Conquer:
                    lblNumber.Content = nextTerritory.temparmies;
                    break;
                case GameState.Move:
                    lblNumber.Content = nextTerritory.temparmies;
                    break;
            }
        }
        private void UpdatePlayerPanelUI()
        {
            int i = players.IndexOf(currentplayer);
            foreach (StackPanel s in panel_Players.Children) { s.Background = Brushes.LightGray; }
            panel_UI.Background = currentplayer.Color;
            switch (i)
            {
                case 0:
                    panel_Player1.Background = Brushes.LightBlue;
                    break;
                case 1:
                    panel_Player2.Background = Brushes.LightBlue;
                    break;
                case 2:
                    panel_Player3.Background = Brushes.LightBlue;
                    break;
                case 3:
                    panel_Player4.Background = Brushes.LightBlue;
                    break;
                case 4:
                    panel_Player5.Background = Brushes.LightBlue;
                    break;
                case 5:
                    panel_Player6.Background = Brushes.LightBlue;
                    break;
            }
        }
        private void UpdatePlayerUI()
        {
            if(gameState == GameState.PlacingArmy) { UpdatePlayerUndeployed(); }
            if(gameState == GameState.Conquer) { SelectButton(nextTerritory.name).Content = nextTerritory.currentarmies; }
            UpdatePlayerTerritories(true);
            UpdatePlayerArmies(true);
        }
        private void UpdatePlayerUndeployed()
        { // Updates the UI to show the player's undeployed armies.
            Output(String.Format("You have {0} armies to place.", currentplayer.army_undeployed));
        }
        private void UpdatePlayerTerritories(bool single)
        { // Updates the UI to show the number of territories currently owned by player.
          // True indicates that only the current player must be updated.
          // False indicates that multiple players' UI must be updated.
        }
        private void UpdatePlayerArmies(bool single)
        { // Update to show the player(s) army strength
        }
        private void UpdateStateUI()
        {
            switch (gameState)
            {
                case GameState.Attacking:
                    lblState.Content = "Attacking";
                    Output("Click on the territory you wish to attack from.");
                    Output("The territories you can attack will be highlighted.");
                    break;
                case GameState.InitialArmyPlace:
                    lblState.Content = "Placing Army";
                    Output("Click to place army.");
                    break;
                case GameState.PlacingArmy:
                    lblState.Content = "Placing Army";
                    Output("Click to select a territory and place armies. Right-click to remove.");
                    Output("Right-Click to remove.");
                    Output(String.Format("You have {0} armies to place.", currentplayer.army_undeployed));
                    break;
                case GameState.Move:
                    lblState.Content = "Fortifying";
                    break;
                case GameState.Conquer:
                    lblState.Content = "Conquer";
                    Output("You have won this battle.");
                    Output("Use + - and confirm to send armies to the newly captured territory.");
                    break;
            }
        }
        private void ClearSelectionsUI()
        {
            foreach(Button b in GameGrid.Children)
            {
                b.BorderBrush = Brushes.Gray;
            }
            lblNumber.Content = 0;
            ClearSelections();
        }

        //// Backend Methods ////
        private void SelectTerritory(String territoryname)
        { // Matches name of selected territory to territory in game code. //
            // can be made more efficient as is finding button that has already been registered.
            slctTerritory = RetrieveTerritory(territoryname);
            Button b = SelectButton(slctTerritory.name);
            b.BorderBrush = Brushes.Lime;
        }
        private Territory RetrieveTerritory(String territoryname)
        {
            for (int i = 0; i < territories.Count; i++)
            {
                if (territoryname == territories[i].name) { return territories[i]; }
            }
            throw new Exception("Territory does not exist");
        }
        private Button SelectButton(String territoryname)
        { // Retrieves a button from the Game Grid so that it's UI can be updated.
            foreach (Button b in GameGrid.Children)
            {
                if (territoryname == b.Name) { return b; }
            }
            throw new Exception("Button not found");
        }
        private void Output(String s)
        {
            if (txtOutput.Text == "") { txtOutput.Text = s; }
            else
            {
                String[] tmp = txtOutput.Text.Split('\n');
                if (tmp.Length >= 8)
                {
                    tmp[0] = tmp[1];
                    for (int i = 1; i < (tmp.Length - 1); i++)
                    {
                        tmp[i] = "\n" + tmp[i + 1];
                    }
                    tmp[7] = ("\n" + s);
                    txtOutput.Text = tmp[0] + tmp[1] + tmp[2] + tmp[3] + tmp[4] + tmp[5] + tmp[6] + tmp[7];
                }
                else { txtOutput.Text += String.Format("\n{0}", s); }
            }
        }
        private void ClearSelections()
        { // Clears the selected Territories and players so as to prevent bugs.
            // This method is not entirely necessary and the program would be more efficient
            // without, however it protects against human error in the code.
            slctTerritory = null;
            nextTerritory = null;
            defendplayer = null;
        }
        private void UpdateState(GameState g)
        {
            gameState = g;
            if(gameState == GameState.Conquer) { ConquerTerritoryUI(); }
            UpdateStateUI();
        }
        private void ShowAttack()
        { // Shows territories that can be attacked from current position
            // merge with move in next update

            bool canmove = false;
            foreach(String s in slctTerritory.links)
            {
                if(RetrieveTerritory(s).owner != currentplayer)
                {
                    canmove = true;
                    Button b = SelectButton(s);
                    b.BorderBrush = Brushes.Aqua;
                }
            }
            if(canmove == false)
            {
                Output("There is nowhere to attack from here.");
            }
        }
        private bool ShowMoves(Territory t)
        { // merge with show attack for efficiency
            // this is very much a bodge. please fix with a proper solution later.
            scanterritories.Add(t);
            bool canmove = false;
            foreach(String s in t.links)
            {
                Territory y = RetrieveTerritory(s);
                if (!scanterritories.Contains(y))
                {
                    scanterritories.Add(y);
                    if (y.owner == currentplayer)
                    {
                        canmove = true;
                        Button b = SelectButton(s);
                        b.BorderBrush = Brushes.Aqua;
                        ShowMoves(y);
                    }
                }
            }
            return canmove;
        }
        private void AdjustAttackMoves(int i)
        {
            if(i >= 1)
            {
                if (slctTerritory.currentarmies < 2)
                {
                    Output("At least one army must remain in a friendly territory.");
                    return;
                }
            }
            if(i <= -1)
            {
                if(nextTerritory.temparmies <= 1)
                {
                    switch (gameState)
                    {
                        case GameState.Attacking:
                            Output("You cannot attack with less than one army.");
                            Output("Click cancel to stop the attack.");
                            break;
                        case GameState.Conquer:
                            Output("You must keep at least one army in the new territory.");
                            break;
                        case GameState.Move:
                            Output("You must move at least one army, if you wish to cancel click 'cancel'");
                            break;
                    }
                    return;
                }
            }
            slctTerritory.currentarmies -= i;
            nextTerritory.temparmies += i;
            UpdateNumOutput();
            AttackTerritoryUI();
            if(gameState == GameState.Conquer) { ConquerTerritoryUI(); }
        }
        private void CancelUnconfirmedActions()
        {
            switch (gameState)
            {
                case GameState.PlacingArmy:
                    foreach (Territory t in territories)
                    {
                        if (t.owner == currentplayer)
                        {
                            currentplayer.army_undeployed += t.temparmies;
                            t.temparmies = 0;
                        }
                    }
                    break;
                case GameState.Attacking:
                    if(slctTerritory != null)
                    {
                        if(nextTerritory != null)
                        {
                            slctTerritory.currentarmies += nextTerritory.temparmies;
                            nextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI();
                    }
                    break;
                case GameState.Conquer:
                    Output("You must move armies into the newly captured territory.");
                    break;
                case GameState.Move:
                    if(slctTerritory != null)
                    {
                        if(nextTerritory != null)
                        {
                            slctTerritory.currentarmies += nextTerritory.temparmies;
                            nextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI();
                    }
                    break;
            }
            ClearSelectionsUI();
        }

        ////  Button Events  ////
        private void Click(object sender, RoutedEventArgs e)
        {   // Called when a territory is clicked on and performs an action based on the
            // context in which it was clicked.
            Button btnTerritory = (Button)sender;
            switch (gameState)
            {
                case GameState.InitialArmyPlace:
                    SelectTerritory(btnTerritory.Name);
                    if (slctTerritory.owner == null || slctTerritory.owner == currentplayer)
                    {
                        Place_Reinforce(slctTerritory, 1);
                        currentplayer.army_strength += 1;
                        CyclePlayers();
                        UpdatePlayerUI();
                        NextTurn();
                    }
                    else { Output("You cannot capture this territory."); slctTerritory = null; }
                    break;
                case GameState.PlacingArmy:
                    ClearSelectionsUI();
                    SelectTerritory(btnTerritory.Name);
                    if (slctTerritory.owner == null || slctTerritory.owner == currentplayer)
                    {
                        UpdateNumOutput();
                        btnTerritory.BorderBrush = Brushes.Lime;
                    }
                    else { Output("This is not your territory."); slctTerritory = null; }
                    break;
                case GameState.Attacking:
                    Territory t = RetrieveTerritory(btnTerritory.Name);
                    if(t.owner == currentplayer)
                    {
                        if(t.currentarmies > 1)
                        {
                            ClearSelections();
                            SelectTerritory(t.name);
                            ShowAttack();
                        }
                        else { Output("You do not have enough armies to attack from here."); break; }
                    }
                    else if (slctTerritory != null)
                    {
                        if(t.owner != null)
                        {
                            if(btnTerritory.BorderBrush == Brushes.Aqua)
                            {
                                nextTerritory = t;
                                btnTerritory.BorderBrush = Brushes.Red;
                                AdjustAttackMoves((slctTerritory.currentarmies - 1));
                                Output("Select the number of armies you wish to attack with.");
                            }
                            else if(btnTerritory.BorderBrush == Brushes.Red){
                                // do the stuff for adding / subtracting how many armies on click
                            }
                            else { Output("You cannot attack this territory from here"); break; }
                        }
                        else { Output("There is nothing here to attack."); break; }
                    }
                    else { Output("You do not own this territory");
                        Output("Select where you wish to attack from"); }
                    break;
                case GameState.Move:
                    t = RetrieveTerritory(btnTerritory.Name);
                    if(t.owner != currentplayer) { Output("You do not own this territory."); break; }
                    else if(slctTerritory == null)
                    {
                        ClearSelectionsUI();
                        SelectTerritory(t.name);
                        if (ShowMoves(slctTerritory)) { Output("You can move armies to the highlighted territories."); }
                        else { Output("There are no friendly territories to move to from here."); ClearSelectionsUI(); }
                        List<Territory> blank = new List<Territory>();
                        scanterritories = blank;
                    }
                    else if (btnTerritory.BorderBrush == Brushes.Aqua)
                    {
                        nextTerritory = t;
                        btnTerritory.BorderBrush = Brushes.Green;
                        AdjustAttackMoves(1);
                    }
                    else { Output("You cannot move armies to here from your selected territory."); }
                    break;
            }
        }
        private void RightClick(object sender, RoutedEventArgs e) { }
        private void Confirm(object sender, RoutedEventArgs e)
        { // Confirms the current action(s)
            switch (gameState)
            {
                case GameState.PlacingArmy:
                    foreach(Territory t in territories)
                    {
                        if(t.owner == currentplayer)
                        {
                            Place_Reinforce(t, t.temparmies);
                            currentplayer.army_strength += t.temparmies;
                            t.temparmies = 0;
                        }
                    }
                    if(currentplayer.army_undeployed == 0) {
                        UpdatePlayerTerritories(true);
                        UpdatePlayerArmies(true);
                        NextAction(); }
                    else { UpdatePlayerUI(); ClearSelectionsUI(); }
                    break;
                case GameState.Attacking:
                    if((slctTerritory != null) && (nextTerritory != null))
                    {
                        double num = rng.NextDouble();
                        double prob = 1 / (1 + Math.Exp(-((nextTerritory.temparmies - nextTerritory.currentarmies) - 0.5)));
                        if(num <= prob)
                        {
                            // add in loss of troops
                            nextTerritory.owner.territoriesowned -= 1;
                            nextTerritory.owner.army_strength -= nextTerritory.currentarmies;
                            nextTerritory.currentarmies = 0;
                            nextTerritory.owner = currentplayer;
                            currentplayer.territoriesowned += 1;
                            bool won = true;
                            foreach(Player p in players) // check this actually works
                            {
                                if(p.territoriesowned > 0) { won = false; }
                            }
                            if (won) { Win(); }
                            UpdateState(GameState.Conquer);
                        }
                        else { Output("Lose"); nextTerritory.temparmies = 0; ClearSelectionsUI(); }
                    }
                    else { Output("You must select the territories you wish to attack to/from first."); }
                    break;
                case GameState.Conquer:
                    Place_Reinforce(nextTerritory, nextTerritory.temparmies);
                    nextTerritory.temparmies = 0;
                    UpdatePlayerUI(); // maybe make this into nextAction();
                    NextAction();
                    break;
                case GameState.Move:
                    Place_Reinforce(nextTerritory, nextTerritory.temparmies);
                    nextTerritory.temparmies = 0;
                    UpdatePlayerUI(); // maybe make this into nextAction();
                    NextAction();
                    break;
            }
        }
        private void Continue(object sender, RoutedEventArgs e) { CancelUnconfirmedActions(); if (gameState != GameState.Conquer) { NextAction(); } }
        private void Cancel(object sender, RoutedEventArgs e) { CancelUnconfirmedActions(); }
        private void Increase(object sender, RoutedEventArgs e) { PlayerActions(true); }
        private void Decrease(object sender, RoutedEventArgs e) { PlayerActions(false); }

        ////  Player Actions ////
        private void Place_Reinforce(Territory T, int num)
        {
            Button b = SelectButton(T.name);
            if (T.owner != currentplayer)
            {
                // Update UI to reflect ownership
                if (T.owner != null) { T.owner.territoriesowned -= 1; }
                T.owner = currentplayer;
                b.Background = T.owner.Color;
                currentplayer.territoriesowned += num;
            }
            // Sets up game-board, sets owner and places army into territory
            T.currentarmies += num;
            if(gameState == GameState.InitialArmyPlace) { currentplayer.army_undeployed -= num; }
            b.Content = T.currentarmies;
        }
        private void NextAction()
        {
            ClearSelectionsUI();
            switch (gameState)
            {
                case GameState.InitialArmyPlace:
                    Output("You must place all of your armies.");
                    break;
                case GameState.PlacingArmy:
                    UpdateState(GameState.Attacking);
                    break;
                case GameState.Attacking:
                    UpdateState(GameState.Move);
                    break;
                case GameState.Conquer:
                    UpdateState(GameState.Attacking);
                    break;
                case GameState.Move:
                    NextTurn();
                    break;
            }
        }
        private void PlayerActions(bool increase)
        { // This method carries out the player's instructions, which are only confirmed when "confirm" is clicked.
            if (slctTerritory != null)
            {
                int i = -1;
                switch (gameState)
                {
                    case GameState.PlacingArmy:
                        if (increase == true)
                        {
                            if (currentplayer.army_undeployed > 0) { i = 1; }
                            else
                            {
                                Output("You have no armies left to place");
                                break;
                            }
                        }
                        slctTerritory.temparmies += i;
                        currentplayer.army_undeployed -= i;
                        UpdateNumOutput();
                        break;
                    case GameState.Attacking:
                        if ((slctTerritory != null) && (nextTerritory != null))
                        {
                            if (increase == true) { i = 1; }
                            AdjustAttackMoves(i);
                        }
                        else { Output("You must select the territories you wish to attack to/from."); }
                        break;
                    case GameState.Conquer:
                        if (increase == true) { i = 1; } // merge this with above
                        AdjustAttackMoves(i);
                        break;
                    case GameState.Move:
                        if (increase == true) { i = 1; } // merge this with above
                        AdjustAttackMoves(i);
                        break;
                }
            }
            else { Output("Please select a territory."); }
        }

        // Save Game //
        private void SaveGame(object sender, RoutedEventArgs e)
        { // Creates a gamemanager instance, serializes it and saves it to a file. Reporting back to the player if the save was successful.
            if(gameState == GameState.InitialArmyPlace) { Output("You must finish setup before attempting to save."); }
            else if(action == true) { Output("You must finish your current action before saving"); }
            else
            {
                try
                {
                    GameManager G = new GameManager(players, territories, currentplayer, turn, gameState);
                    GameManager.SaveGame(G);
                    Output("Game saved successfully");
                }
                catch { Output("An error has occurred. The game may not have saved, please try again."); };
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Win();
        }
    }
}
