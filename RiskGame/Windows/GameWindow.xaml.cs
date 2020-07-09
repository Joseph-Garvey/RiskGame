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
using RiskGame.Game.Locations;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
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
        private GameManager game;
        private List<Player> Players
        {
            get { return game.players; }
            set { game.players = value; }
        }
        private List<Territory> Territories
        {
            get { return game.territories; }
            set { game.territories = value; }
        }
        private List<Continent> Continents
        {
            get { return game.continents; }
            set { game.continents = value; }
        }
        private Territory SlctTerritory
        {
            get { return game.slctTerritory; }
            set { game.slctTerritory = value; }
        }
        private Territory NextTerritory
        {
            get { return game.nextTerritory; }
            set { game.nextTerritory = value; }
        }
        private Player CurrentPlayer
        {
            get { return game.currentplayer; }
            set { game.currentplayer = value; }
        }
        private GameState gamestate
        {
            get { return game.gameState; }
            set { game.gameState = value; }
        }
        private int time = 6000;
        private static Random rng = new Random();
        private int Turn
        {
            get { return game.turn; }
            set { game.turn = value; }
        }
        private static List<Territory> scanterritories = new List<Territory>();
        public string map
        {
            get { return game.map; }
            set { game.map = value; }
        }
        public string gamemode
        {
            get { return game.gamemode; }
            set { game.gamemode = value; }
        }
        private bool music_enabled;
        private BackgroundWorker workerthread = null;
        //// Constructors ////
        // Load Game //
        public GameWindow(GameManager _game)
        {
            // not complete // check at end once all is done
            // takes local variables and matches them to the gamemanager variables to load game. (Incomplete)
            InitializeComponent();
            game = _game;
            LoadPlayerUI();
            music_enabled = ((Human)Players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            Output("The game has loaded.");
        }
        // New Game //
        public GameWindow(List<Player> _players, bool randomise_initial)
        {
            InitializeComponent();
            GameManager.ClearEmptyFile();
            game = new GameManager();
            Players = _players;
            Turn = 0;
            music_enabled = ((Human)Players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            // Creation of Territories and Map Setup //
            map = "Default";
            gamemode = "New Risk";
            List<String> links = new List<string>{ "Kamchatka", "Alberta", "Northwest_Canada" };
            Territory Alaska = new Territory("Alaska",links, btnAlaska);
            links = new List<string>{ "Alaska", "Alberta", "Greenland", "Ontario"};
            Territory Northwest_Canada = new Territory("Northwest_Canada", links, btnNorthwest_Canada);
            links = new List<string> { "Northwest_Canada","Quebec","Ontario","Iceland"};
            Territory Greenland = new Territory("Greenland", links, btnGreenland);
            links = new List<string> {"Alaska","Northwest_Canada","Ontario","Western_US"};
            Territory Alberta = new Territory("Alberta", links, btnAlberta);
            links = new List<string> {"Ontario","Greenland","Eastern_US" };
            Territory Quebec = new Territory("Quebec", links, btnQuebec);
            links = new List<string> {"Greenland","Quebec","Eastern_US", "Western_US", "Northwest_Canada" };
            Territory Ontario = new Territory("Ontario", links, btnOntario);
            links = new List<string> {"Alberta","Ontario","Eastern_US","Central_America"};
            Territory Western_US = new Territory("Western_US", links, btnWestern_US);
            links = new List<string> {"Western_US","Ontario","Central America", "Quebec"};
            Territory Eastern_US = new Territory("Eastern_US", links, btnEastern_US);
            links = new List<string> {"Western_US","Eastern_US","Venezuela"};
            Territory Central_America = new Territory("Central_America", links, btnCentral_America);
            links = new List<string> {"Central_America","Peru","Brazil"};
            Territory Venezuela = new Territory("Venezuela", links, btnVenezuela);
            links = new List<string> {"Venezuela","Brazil","Argentina"};
            Territory Peru = new Territory("Peru", links, btnPeru);
            links = new List<string> {"Venezuela","Peru","Argentina","North_Africa"};
            Territory Brazil = new Territory("Brazil", links, btnBrazil);
            links = new List<string> {"Peru","Brazil" };
            Territory Argentina = new Territory("Argentina", links, btnArgentina);
            links = new List<string> {"Greenland","Scandinavia","UK_Ireland" };
            Territory Iceland = new Territory("Iceland", links, btnIceland);
            links = new List<string> {"Iceland","Western_Europe","Northern_Europe","Scandinavia" };
            Territory UK_Ireland = new Territory("UK_Ireland", links, btnUK_Ireland);
            links = new List<string> {"Iceland","UK_Ireland","Northern_Europe","Soviet_Bloc" };
            Territory Scandinavia = new Territory("Scandinavia", links, btnScandinavia);
            links = new List<string> { "Scandinavia","Northern_Europe","Southern_Europe","Ural","Afghanistan","Middle_East" };
            Territory Soviet_Bloc  = new Territory( "Soviet_Bloc" , links, btnSoviet_Bloc);
            links = new List<string> {"Western_Europe","Northern_Europe","Soviet_Bloc","Middle_East","Egypt"   };
            Territory Southern_Europe = new Territory("Southern_Europe", links, btnSouthern_Europe);
            links = new List<string> { "UK_Ireland","Scandinavia","Soviet_Bloc","Southern_Europe","Western_Europe" };
            Territory Northern_Europe = new Territory("Northern_Europe", links, btnNorthern_Europe);
            links = new List<string> {"UK_Ireland","Northern_Europe","Southern_Europe","North_Africa" };
            Territory Western_Europe = new Territory("Western_Europe", links, btnWestern_Europe);
            links = new List<string> {"Brazil","Egypt","East_Africa","Central_Africa","Western_Europe" };
            Territory North_Africa = new Territory("North_Africa", links, btnNorth_Africa);
            links = new List<string> {"North_Africa","Southern_Europe","Middle_East","East_Africa"};
            Territory Egypt = new Territory("Egypt", links, btnEgypt);
            links = new List<string> {"North_Africa","East_Africa","South_Africa" };
            Territory Central_Africa = new Territory("Central_Africa", links, btnCentral_Africa);
            links = new List<string> {"Egypt","Middle_East","Madagascar","South_Africa","Central_Africa","North_Africa" };
            Territory East_Africa = new Territory("East_Africa", links, btnEast_Africa);
            links = new List<string> { "Central_Africa","East_Africa","Madagascar" };
            Territory South_Africa = new Territory("South_Africa", links, btnSouth_Africa);
            links = new List<string> { "South_Africa","East_Africa"};
            Territory Madagascar = new Territory("Madagascar", links, btnMadagascar);
            links = new List<string> {"Southern_Europe","Soviet_Bloc","Afghanistan","India","East_Africa","Egypt" };
            Territory Middle_East= new Territory("Middle_East", links, btnMiddle_East);
            links = new List<string> {"Middle_East","Soviet_Bloc","Ural","China","India" };
            Territory Afghanistan = new Territory("Afghanistan", links, btnAfghanistan);
            links = new List<string> {"Middle_East","Afghanistan","China","Southeast_Asia" };
            Territory India = new Territory("India", links, btnIndia);
            links = new List<string> {"India","China","Indonesia" };
            Territory Southeast_Asia = new Territory("Southeast_Asia", links, btnSoutheast_Asia);
            links = new List<string> {"Afghanistan","Ural","Siberia","Mongolia","Southeast_Asia","India" };
            Territory China = new Territory("China", links, btnChina);
            links = new List<string> {"Soviet_Bloc","Siberia","China","Afghanistan" };
            Territory Ural = new Territory("Ural", links, btnUral);
            links = new List<string> {"Ural","Yakutsk","Irkutsk","Mongolia","China" };
            Territory Siberia = new Territory("Siberia", links, btnSiberia);
            links = new List<string> {"China","Siberia","Irkutsk","Kamchatka","Japan" };
            Territory Mongolia = new Territory("Mongolia", links, btnMongolia);
            links = new List<string> {"Mongolia","Kamchatka" };
            Territory Japan = new Territory("Japan", links, btnJapan);
            links = new List<string> {"Siberia","Yakutsk","Kamchatka","Mongolia" };
            Territory Irkutsk = new Territory("Irkutsk", links, btnIrkutsk);
            links = new List<string> {"Siberia","Kamchatka","Irkutsk" };
            Territory Yakutsk = new Territory("Yakutsk", links, btnYakutsk);
            links = new List<string> {"Yakutsk", "Alaska","Japan","Mongolia","Irkutsk"};
            Territory Kamchatka = new Territory("Kamchatka", links, btnKamchatka);
            links = new List<string> {"New_Guinea","Southeast_Asia" };
            Territory Indonesia = new Territory("Indonesia", links, btnIndonesia);
            links = new List<string> {"Indonesia","Eastern_Australia","Western_Australia"};
            Territory New_Guinea = new Territory("New_Guinea", links, btnNew_Guinea);
            links = new List<string> {"Eastern_Australia","New_Guinea" };
            Territory Western_Australia = new Territory("Western_Australia", links, btnWestern_Australia);
            links = new List<string> {"Western_Australia","New_Guinea" };
            Territory Eastern_Australia = new Territory("Eastern_Australia", links, btnEastern_Australia);
            Territories = new List<Territory>
            {
                Alaska, Northwest_Canada,Greenland,Alberta,Quebec,Ontario,Western_US,Eastern_US,Central_America,
                Venezuela,Peru,Brazil,Argentina,
                Iceland,Scandinavia,UK_Ireland,Soviet_Bloc,Northern_Europe,Western_Europe,Southern_Europe,
                North_Africa,Egypt,East_Africa,Central_Africa,South_Africa,Madagascar,
                Indonesia,New_Guinea,Eastern_Australia,Western_Australia,
                Middle_East,Afghanistan,India,Ural,Siberia,China,Southeast_Asia,Mongolia,Irkutsk,Yakutsk,Kamchatka,Japan
            };
            Continent North_America = new Continent("North America",(new List<Territory> { Alaska, Alberta, Central_America, Eastern_US, Greenland, Northwest_Canada, Ontario, Quebec, Western_US }), 5);
            Continent South_America = new Continent("South America",(new List<Territory> { Argentina, Brazil, Peru, Venezuela }), 2);
            Continent Europe = new Continent("Europe",(new List<Territory> { UK_Ireland, Iceland, Northern_Europe, Scandinavia, Southern_Europe, Soviet_Bloc, Western_Europe }), 5);
            Continent Africa = new Continent("Africa",(new List<Territory> { Central_Africa, East_Africa, Egypt, Madagascar, North_Africa, South_Africa}), 3);
            Continent Asia = new Continent("Asia",(new List<Territory>  { Afghanistan, China, India, Irkutsk, Japan, Kamchatka, Middle_East, Mongolia, Southeast_Asia, Siberia, Ural, Yakutsk }), 7);
            Continent Australia = new Continent("Australia",(new List<Territory> { Eastern_Australia, Indonesia, New_Guinea, Western_Australia }), 2);
            Continents = new List<Continent> { North_America, South_America, Europe, Africa, Asia, Australia };
            SetupGame(randomise_initial);
        }

        // Game Setup and UI Management //
        private void SetupGame(bool randomise_initial)
        {
            // Determines how many armies players have.
            int initialarmies = (50 - (5 * Players.Count));
            CurrentPlayer = Players[0];
            // Setup Board and initial armies //
            UISetup();
            foreach (Player p in Players) { p.army_undeployed = initialarmies; }
            UpdateState(GameState.InitialArmyPlace);
            if (randomise_initial == true) { SetupRandom(); StartGame(); }
            else
            {
                if(CurrentPlayer is Human)
                {
                    if (((Human)CurrentPlayer).hints_enabled)
                    {
                        Output("Place armies around the map using left click.");
                        Output("You can capture any territory not already taken by another player.");
                    }
                }
            }
        }
        private void StartGame()
        {
            Output("The Game is beginning.");
            gamestate = GameState.PlacingArmy;
            NextTurn();
            GameManager.SaveGame(game);
        }
        private void SetupRandom()
        {
            // originally tried various methods of randomising player and territory assignments to varying degrees of success.
            // However many would get stuck in loops, or to avoid loops would become overly complex.
            // I realised I could instead randomise the order of territories in the list and then assign each territory to a player methodically.
            // This achieves the same effect with far less complexity.
            Territories.Shuffle();
            // Assigned initial placements of armies.
            foreach (Territory t in Territories)
            {
                // ensures each territory is owned by a player.
                bool assigned = false;
                do
                {
                    CyclePlayers();
                    if (CurrentPlayer.army_undeployed > 0)
                    {
                        Place_Reinforce(t, 1);
                        assigned = true;
                    }
                } while (assigned == false);
            }
            // Places remaining armies around map in friendly territory until there are none left //
            foreach (Player p in Players)
            {
                CurrentPlayer = p;
                while (p.army_undeployed > 0)
                {
                    foreach (Territory t in Territories)
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
        // Game Start UI setup //
        private void UISetup()
        {
            // This code sets up the "player panel" with the players details, resizing certain elements to avoid white borders.
            // Needs values tweaked.
            lblPlayerName1.Content = Players[0].Username;
            lblPlayerName2.Content = Players[1].Username;
            rectPlayerColor1.Fill = (SolidColorBrush)Players[0].Color;
            rectPlayerColor2.Fill = (SolidColorBrush)Players[1].Color;
            Thickness th = new Thickness(0, 20, 0, 0);
            int fs = 11;
            int rect_height = 20;
            if (Players.Count >= 3)
            {
                panel_Players.Margin = new Thickness(20, 0, 10, 0);
                th = new Thickness(0, 15, 0, 0);
                lblPlayerName3.Content = Players[2].Username;
                rectPlayerColor3.Fill = (SolidColorBrush)Players[2].Color;
                panel_Player3.Visibility = Visibility.Visible;
            }
            if (Players.Count >= 4)
            {
                th = new Thickness(0, 10, 0, 0);
                lblPlayerName4.Content = Players[3].Username;
                rectPlayerColor4.Fill = (SolidColorBrush)Players[3].Color;
                panel_Player4.Visibility = Visibility.Visible;
            }
            if (Players.Count >= 5)
            {
                rect_height = 15;
                th = new Thickness(0, 5, 0, 0);
                lblPlayerName5.Content = Players[4].Username;
                rectPlayerColor5.Fill = (SolidColorBrush)Players[4].Color;
                panel_Player5.Visibility = Visibility.Visible;
            }
            if (Players.Count >= 6)
            {
                fs = 9;
                rect_height = 12;
                rectPlayerColor6.Fill = (SolidColorBrush)Players[5].Color;
                lblPlayerName6.Content = Players[5].Username;
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
            rectPlayerColor1.Height = i;
            rectPlayerColor2.Height = i;
            rectPlayerColor3.Height = i;
            rectPlayerColor4.Height = i;
            rectPlayerColor5.Height = i;
            rectPlayerColor6.Height = i;
        }

        //// Game Methods ////
        private void CyclePlayers()
        { // Cycles through the list of players, for a new turn or placing armies.
            if ((Players.IndexOf(CurrentPlayer) + 1) == (Players.Count)) { CurrentPlayer = Players[0]; }
            else { CurrentPlayer = Players[(Players.IndexOf(CurrentPlayer) + 1)]; }
        }
        private bool AllPlaced()
        {
            bool allplaced = true;
            foreach(Player p in Players)
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
            if(gamestate == GameState.InitialArmyPlace)
            {
                if ((!(CurrentPlayer is NeutralAI)) && CurrentPlayer.army_undeployed > 0)
                {
                    Output(String.Format("It is now {0}'s turn.", CurrentPlayer.Username));
                    UpdatePlayerPanelUI();
                }
                else
                {
                    if(CurrentPlayer is NeutralAI) { CyclePlayers(); NextTurn(); }
                    if (AllPlaced())
                    {
                        // Neutral AI conquer
                        StartGame();
                    }
                }
            }
            else
            {
                Turn += 1;
                CyclePlayers();
                List<String> ownedContinents = new List<string>();
                int bonus = 0;
                foreach (Continent c in Continents)
                {
                    if (ContinentOwned(c))
                    {
                        ownedContinents.Add(c.name);
                        bonus += c.bonus;
                    }
                }
                CurrentPlayer.army_undeployed += ((CurrentPlayer.territoriesowned / 3) + bonus);
                UpdatePlayerPanelUI();
                UpdateState(GameState.PlacingArmy);
                switch(ownedContinents.Count)
                {
                    case 1:
                        Output(String.Format("You have received {0} bonus armies from capturing all of {1}", bonus, ownedContinents[0]));
                        break;
                    case 2:
                        Output(String.Format("You have received {0} bonus armies from capturing all of {1} and {2}", bonus, ownedContinents[0], ownedContinents[1]));
                        break;
                    case 3:
                        Output(String.Format("You have received {0} bonus armies from {1}, {2}, and {3}", bonus, ownedContinents[0], ownedContinents[1], ownedContinents[2]));
                        break;
                    case 4:
                        Output(String.Format("You have received {0} bonus armies from {1}, {2}, {3}", bonus, ownedContinents[0], ownedContinents[1], ownedContinents[2]));
                        Output(String.Format("and {0}", ownedContinents[3]));
                        break;
                    case 5:
                        Output(String.Format("You have received {0} bonus armies from {1}, {2}, {3}", bonus, ownedContinents[0], ownedContinents[1], ownedContinents[2]));
                        Output(String.Format("{0} and {1}", ownedContinents[3], ownedContinents[4]));
                        break;
                }
                StartTimer();
            }
        }
        private void Win()
        {
            CurrentPlayer.score += CurrentPlayer.army_strength / 3;
            int finalscore = CurrentPlayer.score / Turn;
            GameDetails gamedetails = new GameDetails(DateTime.Now.ToString(), CurrentPlayer.Username, Players.Count.ToString(), finalscore.ToString(),Turn.ToString(), map, gamemode);
            GameDetails.Save(gamedetails);
            GameManager.DeleteGame(game.GameID);
            Highscores Setup = new Highscores(gamedetails);
            App.Current.MainWindow = Setup;
            this.Close();
            Setup.Show();
        }

        //// UI /////
        private void LoadPlayerUI()
        {  // use binding in future
            UISetup();
            UpdatePlayerPanelUI();
            UpdateState(game.gameState);
            if(gamestate == GameState.PlacingArmy)
            {
                UpdatePlayerUndeployed();
            }
            foreach(Territory t in Territories)
            {
                foreach(Button b in GameGrid.Children)
                {
                    if(t.name == b.Name.Substring(3))
                    {
                        t.button = b;
                    }
                }
                t.button.Background = t.owner.Color;
                t.button.Content = t.currentarmies;
            }
        }
        private void ConquerTerritoryUI()
        {
            NextTerritory.button.Background = NextTerritory.owner.Color;
            NextTerritory.button.Content = NextTerritory.temparmies;
        } // Updates nextTerritory UI for Conquer
        private void AttackTerritoryUI()
        {
            SlctTerritory.button.Content = SlctTerritory.currentarmies;
        } // Updates selected territory's armies
        private void UpdateNumOutput()
        {

            switch (gamestate)
            {
                case GameState.PlacingArmy:
                    lblNumber.Content = SlctTerritory.temparmies;
                    break;
                case GameState.Attacking:
                    lblNumber.Content = NextTerritory.temparmies;
                    break;
                case GameState.Conquer:
                    lblNumber.Content = NextTerritory.temparmies;
                    break;
                case GameState.Move:
                    lblNumber.Content = NextTerritory.temparmies;
                    break;
            }
        } // Updates UI Label Number
        private void UpdatePlayerPanelUI()
        {
            int i = Players.IndexOf(CurrentPlayer);
            foreach (StackPanel s in panel_Players.Children) { s.Background = Brushes.LightGray; }
            panel_UI.Background = CurrentPlayer.Color;
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
        }  // Updates currently highlighted player in UI stack
        private void UpdatePlayerUndeployed()
        { // Updates the UI to show the player's undeployed armies.
            Output(String.Format("You have {0} armies to place.", CurrentPlayer.army_undeployed));
        } // Use to update undeployed on place
        private void UpdateStateUI()
        {
            switch (gamestate)
            {
                case GameState.Attacking:
                    btnState.Content = "Confirm Attack";
                    if(CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output("Click on the territory you wish to attack from.");
                            Output("The territories you can attack will be highlighted.");
                        }
                    }
                    break;
                case GameState.InitialArmyPlace:
                    btnState.Content = "Confirm Army Placement";
                    if (CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output(String.Format("{0}, Click to place army.", CurrentPlayer.Username));
                        }
                    }
                    break;
                case GameState.PlacingArmy:
                    btnState.Content = "Confirm Army Placement";
                    if (CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output("Click or + to select a territory and place armies.");
                            Output("Right-Click or - to remove.");
                        }
                    }
                    Output(String.Format("You have {0} armies to place.", CurrentPlayer.army_undeployed));
                    break;
                case GameState.Move:
                    btnState.Content = "Confirm Fortify";
                    break;
                case GameState.Conquer:
                    btnState.Content = "Confirm Conquer";
                    if (CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output("Use + - or left/right click and then confirm to send armies to the newly captured territory.");
                        }
                    }
                    break;
            }
        } // Start of turn instructions and UI State change
        private void ClearSelectionsUI()
        {
            foreach(Button b in GameGrid.Children)
            {
                b.BorderBrush = Brushes.Gray;
            }
            lblNumber.Content = 0;
            ClearSelections();
        } // Clears Selections and UI

        //// Backend Methods ////
        private Territory RetrieveTerritory(String territoryname)
        {
            territoryname = territoryname.Replace('_', ' ');
            for (int i = 0; i < Territories.Count; i++)
            {
                if (territoryname.Replace('_', ' ') == Territories[i].name.Replace('_',' ')) { return Territories[i]; }
            }
            throw new Exception("Territory does not exist");
        }
        private void SelectTerritory(Territory t, Button b, Brush color, bool next)
        {
            if (next) { NextTerritory = t; }
            else { SlctTerritory = t; }
            b.BorderBrush = color;
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
            SlctTerritory = null;
            NextTerritory = null;
        }
        private void UpdateState(GameState g)
        {
            gamestate = g;
            if(gamestate == GameState.Conquer) { ConquerTerritoryUI(); }
            UpdateStateUI();
        }
        private void ShowAttack()
        { // Shows territories that can be attacked from current position
            // merge with move in next update

            bool canmove = false;
            foreach(String s in SlctTerritory.links)
            {
                Territory t = RetrieveTerritory(s);
                if(t.owner != CurrentPlayer)
                {
                    canmove = true;
                    t.button.BorderBrush = Brushes.Aqua;
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
                    if (y.owner == CurrentPlayer)
                    {
                        canmove = true;
                        y.button.BorderBrush = Brushes.Aqua;
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
                if (SlctTerritory.currentarmies < 2)
                {
                    Output("At least one army must remain in a friendly territory.");
                    return;
                }
            }
            if(i <= -1)
            {
                if(NextTerritory.temparmies <= 1)
                {
                    switch (gamestate)
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
            SlctTerritory.currentarmies -= i;
            NextTerritory.temparmies += i;
            UpdateNumOutput();
            AttackTerritoryUI();
            if(gamestate == GameState.Conquer) { ConquerTerritoryUI(); }
        }
        private void CancelUnconfirmedActions()
        {
            switch (gamestate)
            {
                case GameState.PlacingArmy:
                    foreach (Territory t in Territories)
                    {
                        if (t.owner == CurrentPlayer)
                        {
                            CurrentPlayer.army_undeployed += t.temparmies;
                            t.temparmies = 0;
                        }
                    }
                    break;
                case GameState.Attacking:
                    if(SlctTerritory != null)
                    {
                        if(NextTerritory != null)
                        {
                            SlctTerritory.currentarmies += NextTerritory.temparmies;
                            NextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI();
                    }
                    break;
                case GameState.Conquer:
                    Output("You must move armies into the newly captured territory.");
                    break;
                case GameState.Move:
                    if(SlctTerritory != null)
                    {
                        if(NextTerritory != null)
                        {
                            SlctTerritory.currentarmies += NextTerritory.temparmies;
                            NextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI();
                    }
                    break;
            }
            ClearSelectionsUI();
        }
        private bool ContinentOwned(Continent c)
        {
            bool owned = true;
            foreach(Territory t in c.territories)
            {
                if(t.owner != CurrentPlayer) { owned = false; break; }
            }
            return owned;
        }

        // Timer Control //
        private void StartTimer()
        {
            pb_Timer.Value = 0;
            workerthread = new BackgroundWorker();
            workerthread.WorkerReportsProgress = true;
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            workerthread.RunWorkerCompleted += Worker_RunWorkerCompleted;
            workerthread.RunWorkerAsync();
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i < time; i++)
            {
                int progressPercentage = Convert.ToInt32(((double)i / time) * 100);
                (sender as BackgroundWorker).ReportProgress(progressPercentage);
                Thread.Sleep(10);
            }
        }
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb_Timer.Value = e.ProgressPercentage;
        }
        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Output("Your turn has ended.");
            if(gamestate == GameState.Conquer) { Output("Move your armies to end your turn."); }
            else
            {
                CancelUnconfirmedActions();
                NextTurn();
            }
        }

        ////  Button Events  ////
        private void Click(object sender, RoutedEventArgs e)
        {   // Called when a territory is clicked on and performs an action based on the
            // context in which it was clicked.
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' }));
            Button btnTerritory = t.button;
            switch (gamestate)
            {
                case GameState.InitialArmyPlace:
                    SlctTerritory = t;
                    if (SlctTerritory.owner == null || SlctTerritory.owner == CurrentPlayer)
                    {
                        Place_Reinforce(SlctTerritory, 1);
                        CurrentPlayer.army_strength += 1;
                        CyclePlayers();
                        NextTurn();
                    }
                    else { Output("You cannot capture this territory."); SlctTerritory = null; }
                    break;
                case GameState.PlacingArmy:
                    if(t == SlctTerritory) { PlayerActions(true); break; }
                    else
                    {
                        ClearSelectionsUI();
                        if (t.owner == null || t.owner == CurrentPlayer)
                        {
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                            UpdateNumOutput();
                        }
                        else { Output("This is not your territory."); SlctTerritory = null; }
                        break;
                    }
                case GameState.Attacking:
                    if(t.owner == CurrentPlayer)
                    {
                        if(t.currentarmies > 1)
                        {
                            ClearSelections();
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                            ShowAttack();
                        }
                        else { Output("You do not have enough armies to attack from here."); break; }
                    }
                    else if (SlctTerritory != null)
                    {
                        if(t.owner != null)
                        {
                            if(btnTerritory.BorderBrush == Brushes.Aqua)
                            {
                                SelectTerritory(t, btnTerritory, Brushes.Red, true);
                                AdjustAttackMoves((SlctTerritory.currentarmies - 1));
                                if(CurrentPlayer is Human)
                                {
                                    if (((Human)CurrentPlayer).hints_enabled)
                                    {
                                        Output("Select the number of armies you wish to attack with.");
                                    }
                                }
                            }
                            else if(btnTerritory.BorderBrush == Brushes.Red){
                                PlayerActions(true);
                            }
                            else { Output("You cannot attack this territory from here"); break; }
                        }
                        else { Output("There is nothing here to attack."); break; }
                    }
                    else { Output("You do not own this territory");
                        Output("Select where you wish to attack from"); }
                    break;
                case GameState.Move:
                    if(t.owner != CurrentPlayer) { Output("You do not own this territory."); break; }
                    else if(SlctTerritory == null)
                    {
                        ClearSelectionsUI();
                        SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                        if (ShowMoves(SlctTerritory))
                        {
                            if (CurrentPlayer is Human)
                            {
                                if (((Human)CurrentPlayer).hints_enabled)
                                {
                                    Output("You can move armies to the highlighted territories.");
                                }
                            }
                        }
                        else { Output("There are no friendly territories to move to from here."); ClearSelectionsUI(); }
                        List<Territory> blank = new List<Territory>();
                        scanterritories = blank;
                    }
                    else if (btnTerritory.BorderBrush == Brushes.Aqua)
                    {
                        SelectTerritory(t, btnTerritory, Brushes.Green, true);
                        AdjustAttackMoves(1);
                    }
                    else if(btnTerritory.BorderBrush == Brushes.Green)
                    {
                        PlayerActions(true);
                    }
                    else { Output("You cannot move armies to here from your selected territory."); }
                    break;
            }
        }
        private void RightClick(object sender, MouseEventArgs e)
        {
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' }));
            Button btnTerritory = t.button;
            switch (gamestate)
            {
                case GameState.PlacingArmy:
                    if (t == SlctTerritory) { PlayerActions(false); }
                    // error will be output if temp = 0 from player actions script as is shared by decrease button.
                    break;
                case GameState.Attacking:
                    // error is output by adjust attack moves if invalid
                    try
                    {
                        // if nextterritory null break otherwise continue
                        if (t == NextTerritory) { PlayerActions(false); }
                        else if(t == SlctTerritory) { PlayerActions(true); }
                        break;
                    }
                    catch (NullReferenceException) { break; }
                case GameState.Conquer:
                    try
                    {
                        if(t == NextTerritory) { PlayerActions(false); }
                        else if (t == SlctTerritory) { PlayerActions(true); }
                        break;
                    }
                    catch (NullReferenceException) { break; }
                case GameState.Move:
                    if((SlctTerritory != null) && (NextTerritory != null))
                    {
                        if(t == SlctTerritory) { PlayerActions(true); }
                        else if(t == NextTerritory) { PlayerActions(false); }
                    }
                    break;
            }
        }
        private void Confirm(object sender, RoutedEventArgs e)
        { // Confirms the current action(s)
            switch (gamestate)
            {
                case GameState.PlacingArmy:
                    foreach(Territory t in Territories)
                    {
                        if(t.owner == CurrentPlayer)
                        {
                            Place_Reinforce(t, t.temparmies);
                            CurrentPlayer.army_strength += t.temparmies;
                            t.temparmies = 0;
                        }
                    }
                    if(CurrentPlayer.army_undeployed == 0) {
                        NextAction(); }
                    else { ClearSelectionsUI(); }
                    break;
                case GameState.Attacking:
                    if((SlctTerritory != null) && (NextTerritory != null))
                    {
                        double num = rng.NextDouble();
                        double prob = 1 / (1 + Math.Exp(-0.7*((NextTerritory.temparmies - NextTerritory.currentarmies) - 0.5)));
                        if(num <= prob)
                        {
                            NextTerritory.owner.territoriesowned -= 1;
                            NextTerritory.owner.score -= 1;
                            NextTerritory.owner.army_strength -= NextTerritory.currentarmies;
                            NextTerritory.currentarmies = 0;
                            NextTerritory.owner = CurrentPlayer;
                            CurrentPlayer.territoriesowned += 1;
                            CurrentPlayer.score += 1;
                            bool won = true;
                            foreach(Player p in Players)
                            {
                                if(p.territoriesowned > 0) { won = false; }
                            }
                            if (won) { Win(); }
                            int lost = NextTerritory.temparmies - (int)Math.Ceiling(prob * NextTerritory.temparmies);
                            NextTerritory.temparmies -= lost;
                            Output(String.Format("You have captured this territory and lost {0} armies in battle.", lost));
                            UpdateState(GameState.Conquer);
                        }
                        else
                        {
                            NextTerritory.temparmies = 0;
                            int survived = (int)(Math.Ceiling(1 - prob) * NextTerritory.currentarmies);
                            int loss = NextTerritory.currentarmies - survived;
                            NextTerritory.currentarmies = survived; // can be simplified /\
                            Output(String.Format("You have lost this battle, the enemy suffered {0} casualties.", loss ));
                            NextTerritory.button.Content = NextTerritory.currentarmies;
                            ClearSelectionsUI();
                        }
                    }
                    else { Output("You must select the territories you wish to attack to/from first."); }
                    break;
                case GameState.Conquer:
                    Place_Reinforce(NextTerritory, NextTerritory.temparmies);
                    NextTerritory.temparmies = 0;
                    ConquerTerritoryUI();
                    NextAction();
                    break;
                case GameState.Move:
                    Place_Reinforce(NextTerritory, NextTerritory.temparmies);
                    NextTerritory.temparmies = 0;
                    NextAction();
                    break;
            }
        }
        private void Continue(object sender, RoutedEventArgs e) { CancelUnconfirmedActions(); if (gamestate != GameState.Conquer) { NextAction(); } }
        private void Cancel(object sender, RoutedEventArgs e) { CancelUnconfirmedActions(); }
        private void Increase(object sender, RoutedEventArgs e) { PlayerActions(true); }
        private void Decrease(object sender, RoutedEventArgs e) { PlayerActions(false); }

        ////  Player Actions ////
        private void Place_Reinforce(Territory T, int num)
        {
            if (T.owner != CurrentPlayer)
            {
                // Update UI to reflect ownership
                if (T.owner != null) { T.owner.territoriesowned -= 1; }
                T.owner = CurrentPlayer;
                T.button.Background = T.owner.Color;
                CurrentPlayer.territoriesowned += num;
                CurrentPlayer.score += 1;
            }
            // Sets up game-board, sets owner and places army into territory
            T.currentarmies += num;
            if(gamestate == GameState.InitialArmyPlace) { CurrentPlayer.army_undeployed -= num; }
            T.button.Content = T.currentarmies;
        }
        private void NextAction()
        {
            ClearSelectionsUI();
            switch (gamestate)
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
            if (SlctTerritory != null)
            {
                int i = -1;
                switch (gamestate)
                {
                    case GameState.PlacingArmy:
                        // if increase == false ensure no negatives
                        if (increase == true)
                        {
                            if (CurrentPlayer.army_undeployed > 0) { i = 1; }
                            else
                            {
                                Output("You have no armies left to place");
                                break;
                            }
                        }
                        SlctTerritory.temparmies += i;
                        CurrentPlayer.army_undeployed -= i;
                        UpdateNumOutput();
                        break;
                    case GameState.Attacking:
                        if ((SlctTerritory != null) && (NextTerritory != null))
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
            if(gamestate == GameState.InitialArmyPlace) { Output("You must finish setup before attempting to save."); }
            else if(gamestate == GameState.Conquer) { Output("You must finish conquering before saving."); }
            //else if(action == true) { Output("You must finish your current action before saving"); }
            else
            {
                try
                {
                    CancelUnconfirmedActions();
                    GameManager.SaveGame(game);
                    Output("Game saved successfully");
                }
                catch { Output("An error has occurred. The game may not have saved, please try again."); };
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Win();
        }









        // Media //
        private void Mediaplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaForward(sender, e);
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
        private void UpdateMediaText(object sender, RoutedEventArgs e)
        {
        }
    }
}
