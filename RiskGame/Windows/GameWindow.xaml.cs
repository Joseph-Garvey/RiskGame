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
using RiskGame.CustomExceptions;
using System.Threading;
using System.ComponentModel;
using RiskGame.Windows;

namespace RiskGame
{

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
        public String CurrentPlayerUsername
        {
            get { return CurrentPlayer.Username; }
        } // redundant property never used.
        private GameState Gamestate
        {
            get { return game.gameState; }
            set { game.gameState = value; }
        }
        private double DefenseBias
        {
            get { return game.defenderbias; }
            set { game.defenderbias = value; }
        }
        private int Time
        {
            get { return game.time; }
            set { game.time = value; }
        }
        private static Random rng = new Random();
        private int Turn
        {
            get { return game.turn; }
            set { game.turn = value; }
        }
        private static List<Territory> scanterritories = new List<Territory>();
        private GameMap Map
        {
            get { return game.map; }
            set { game.map = value; }
        }
        private GameMode Gamemode
        {
            get { return game.gamemode; }
            set { game.gamemode = value; }
        }
        private bool music_enabled;
        public bool Music_enabled
        {
            get => music_enabled;
            set
            {

                if (Players.Count != 0)
                {
                    try
                    {
                        ((Human)Players[0]).music_enabled = value;
                        Human.Update(Players[0] as Human);
                    }
                    catch {  }
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

                if (Players.Count != 0)
                {
                    try
                    {
                        ((Human)Players[0]).hints_enabled = value;
                        Human.Update(Players[0] as Human);
                    }
                    catch {  }
                }
                hints_enabled = value;
            }
        }
        private BackgroundWorker workerthread = null;
        private bool paused;
        #region Dice
        PlayerDice playerdie1;
        PlayerDice playerdie2;
        PlayerDice playerdie3;
        EnemyDice enemydie1;
        EnemyDice enemydie2;
        List<Dice> dices = new List<Dice>();
        #endregion

        //// Constructors ////
        // Load Game //
        public GameWindow(GameManager _game)
        {
            InitializeComponent();
            game = _game;
            paused = false;
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            chkFullscreen.IsChecked = true;
            game.territories.Sort();
            if (Time > 0)
            {
                TimerSetup();
                StartTimer();
            }
            LoadPlayerUI();
            music_enabled = ((Human)Players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            Output("The game has loaded.");
            if (Gamemode == GameMode.Classic)
            {
                playerdie1 = new PlayerDice(imgPlayerDie1);
                playerdie2 = new PlayerDice(imgPlayerDie2);
                playerdie3 = new PlayerDice(imgPlayerDie3);
                enemydie1 = new EnemyDice(imgEnemyDie1);
                enemydie2 = new EnemyDice(imgEnemyDie2);
            }
        }
        // New Game //
        public GameWindow(List<Player> _players, bool randomise_initial, GameMap _map, GameMode mode, int timerduration, double defensebias)
        {
            InitializeComponent();
            DataContext = this;
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            chkFullscreen.IsChecked = true;
            GameManager.ClearEmptyFile();
            game = new GameManager();
            Time = timerduration * 100;
            Players = _players;
            Music_enabled = ((Human)Players[0]).music_enabled;
            Hints_enabled = ((Human)Players[0]).hints_enabled;
            Turn = 0;
            DefenseBias = defensebias;
            music_enabled = ((Human)Players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            paused = false;
            // Creation of Territories and Map Setup //
            Map = _map;
            try
            {
                MapSetup(false);
            }
            catch (Exception) {
                MessageBox.Show("An error occurred loading the map.");
                this.Close();
                return;
            }
            Gamemode = mode;
            if (Gamemode == GameMode.Classic)
            {
                playerdie1 = new PlayerDice(imgPlayerDie1);
                playerdie2 = new PlayerDice(imgPlayerDie2);
                playerdie3 = new PlayerDice(imgPlayerDie3);
                enemydie1 = new EnemyDice(imgEnemyDie1);
                enemydie2 = new EnemyDice(imgEnemyDie2);
            }
            if (Time > 0)
            {
                TimerSetup();
            }
            else { pb_Timer.Visibility = Visibility.Collapsed; }
            SetupGame(randomise_initial);
        }
        private void MapSetup(bool load)
        {
            List<Button> mapbuttons = new List<Button>();
            if (Map == GameMap.Default && !load)
            {
                Territory Alaska = new Territory("Alaska", new List<string> { "Kamchatka", "Alberta", "Northwest_Canada" }, btnAlaska);
                Territory Northwest_Canada = new Territory("Northwest_Canada", new List<string> { "Alaska", "Alberta", "Greenland", "Ontario" }, btnNorthwest_Canada);
                Territory Greenland = new Territory("Greenland", new List<string> { "Northwest_Canada", "Quebec", "Ontario", "Iceland" }, btnGreenland);
                Territory Alberta = new Territory("Alberta", new List<string> { "Alaska", "Northwest_Canada", "Ontario", "Western_US" }, btnAlberta);
                Territory Quebec = new Territory("Quebec", new List<string> { "Ontario", "Greenland", "Eastern_US" }, btnQuebec);
                Territory Ontario = new Territory("Ontario", new List<string> { "Greenland", "Quebec", "Eastern_US", "Western_US", "Northwest_Canada", "Alberta" }, btnOntario);
                Territory Western_US = new Territory("Western_US", new List<string> { "Quebec", "Ontario", "Eastern_US", "Central_America", "Alberta"}, btnWestern_US);
                Territory Eastern_US = new Territory("Eastern_US", new List<string> { "Western_US", "Ontario", "Central America", "Quebec" }, btnEastern_US);
                Territory Central_America = new Territory("Central_America", new List<string> { "Western_US", "Eastern_US", "Venezuela" }, btnCentral_America);
                Territory Venezuela = new Territory("Venezuela", new List<string> { "Central_America", "Peru", "Brazil" }, btnVenezuela);
                Territory Peru = new Territory("Peru", new List<string> { "Venezuela", "Brazil", "Argentina" }, btnPeru);
                Territory Brazil = new Territory("Brazil", new List<string> { "Venezuela", "Peru", "Argentina", "North_Africa" }, btnBrazil);
                Territory Argentina = new Territory("Argentina", new List<string> { "Peru", "Brazil" }, btnArgentina);
                Territory Iceland = new Territory("Iceland", new List<string> { "Greenland", "Scandinavia", "UK_Ireland" }, btnIceland);
                Territory UK_Ireland = new Territory("UK_Ireland", new List<string> { "Iceland", "Western_Europe", "Northern_Europe", "Scandinavia" }, btnUK_Ireland);
                Territory Scandinavia = new Territory("Scandinavia", new List<string> { "Iceland", "UK_Ireland", "Northern_Europe", "Soviet_Bloc" }, btnScandinavia);
                Territory Soviet_Bloc = new Territory("Soviet_Bloc", new List<string> { "Scandinavia", "Northern_Europe", "Southern_Europe", "Ural", "Afghanistan", "Middle_East" }, btnSoviet_Bloc);
                Territory Southern_Europe = new Territory("Southern_Europe", new List<string> { "Western_Europe", "Northern_Europe", "Soviet_Bloc", "Middle_East", "Egypt" }, btnSouthern_Europe);
                Territory Northern_Europe = new Territory("Northern_Europe", new List<string> { "UK_Ireland", "Scandinavia", "Soviet_Bloc", "Southern_Europe", "Western_Europe" }, btnNorthern_Europe);
                Territory Western_Europe = new Territory("Western_Europe", new List<string> { "UK_Ireland", "Northern_Europe", "Southern_Europe", "North_Africa" }, btnWestern_Europe);
                Territory North_Africa = new Territory("North_Africa", new List<string> { "Brazil", "Egypt", "East_Africa", "Central_Africa", "Western_Europe", "Southern_Europe" }, btnNorth_Africa);
                Territory Egypt = new Territory("Egypt", new List<string> { "North_Africa", "Southern_Europe", "Middle_East", "East_Africa" }, btnEgypt);
                Territory Central_Africa = new Territory("Central_Africa", new List<string> { "North_Africa", "East_Africa", "South_Africa" }, btnCentral_Africa);
                Territory East_Africa = new Territory("East_Africa", new List<string> { "Egypt", "Middle_East", "Madagascar", "South_Africa", "Central_Africa", "North_Africa" }, btnEast_Africa);
                Territory South_Africa = new Territory("South_Africa", new List<string> { "Central_Africa", "East_Africa", "Madagascar" }, btnSouth_Africa);
                Territory Madagascar = new Territory("Madagascar", new List<string> { "South_Africa", "East_Africa" }, btnMadagascar);
                Territory Middle_East = new Territory("Middle_East", new List<string> { "Southern_Europe", "Soviet_Bloc", "Afghanistan", "India", "East_Africa", "Egypt" }, btnMiddle_East);
                Territory Afghanistan = new Territory("Afghanistan", new List<string> { "Middle_East", "Soviet_Bloc", "Ural", "China", "India" }, btnAfghanistan);
                Territory India = new Territory("India", new List<string> { "Middle_East", "Afghanistan", "China", "Southeast_Asia" }, btnIndia);
                Territory Southeast_Asia = new Territory("Southeast_Asia", new List<string> { "India", "China", "Indonesia" }, btnSoutheast_Asia);
                Territory China = new Territory("China", new List<string> { "Afghanistan", "Ural", "Siberia", "Mongolia", "Southeast_Asia", "India" }, btnChina);
                Territory Ural = new Territory("Ural", new List<string> { "Soviet_Bloc", "Siberia", "China", "Afghanistan" }, btnUral);
                Territory Siberia = new Territory("Siberia", new List<string> { "Ural", "Yakutsk", "Irkutsk", "Mongolia", "China" }, btnSiberia);
                Territory Mongolia = new Territory("Mongolia", new List<string> { "China", "Siberia", "Irkutsk", "Kamchatka", "Japan" }, btnMongolia);
                Territory Japan = new Territory("Japan", new List<string> { "Mongolia", "Kamchatka" }, btnJapan);
                Territory Irkutsk = new Territory("Irkutsk", new List<string> { "Siberia", "Yakutsk", "Kamchatka", "Mongolia" }, btnIrkutsk);
                Territory Yakutsk = new Territory("Yakutsk", new List<string> { "Siberia", "Kamchatka", "Irkutsk" }, btnYakutsk);
                Territory Kamchatka = new Territory("Kamchatka", new List<string> { "Yakutsk", "Alaska", "Japan", "Mongolia", "Irkutsk" }, btnKamchatka);
                Territory Indonesia = new Territory("Indonesia", new List<string> { "New_Guinea", "Southeast_Asia", "Western_Australia" }, btnIndonesia);
                Territory New_Guinea = new Territory("New_Guinea", new List<string> { "Indonesia", "Eastern_Australia", "Western_Australia" }, btnNew_Guinea);
                Territory Western_Australia = new Territory("Western_Australia", new List<string> { "Eastern_Australia", "New_Guinea", "Indonesia" }, btnWestern_Australia);
                Territory Eastern_Australia = new Territory("Eastern_Australia", new List<string> { "Western_Australia", "New_Guinea" }, btnEastern_Australia);
                Territories = new List<Territory>
                {
                Alaska, Northwest_Canada,Greenland,Alberta,Quebec,Ontario,Western_US,Eastern_US,Central_America,
                Venezuela,Peru,Brazil,Argentina,
                Iceland,Scandinavia,UK_Ireland,Soviet_Bloc,Northern_Europe,Western_Europe,Southern_Europe,
                North_Africa,Egypt,East_Africa,Central_Africa,South_Africa,Madagascar,
                Indonesia,New_Guinea,Eastern_Australia,Western_Australia,
                Middle_East,Afghanistan,India,Ural,Siberia,China,Southeast_Asia,Mongolia,Irkutsk,Yakutsk,Kamchatka,Japan
                };
                Continent North_America = new Continent("North America", (new List<Territory> { Alaska, Alberta, Central_America, Eastern_US, Greenland, Northwest_Canada, Ontario, Quebec, Western_US }), 5);
                Continent South_America = new Continent("South America", (new List<Territory> { Argentina, Brazil, Peru, Venezuela }), 2);
                Continent Europe = new Continent("Europe", (new List<Territory> { UK_Ireland, Iceland, Northern_Europe, Scandinavia, Southern_Europe, Soviet_Bloc, Western_Europe }), 5);
                Continent Africa = new Continent("Africa", (new List<Territory> { Central_Africa, East_Africa, Egypt, Madagascar, North_Africa, South_Africa }), 3);
                Continent Asia = new Continent("Asia", (new List<Territory> { Afghanistan, China, India, Irkutsk, Japan, Kamchatka, Middle_East, Mongolia, Southeast_Asia, Siberia, Ural, Yakutsk }), 7);
                Continent Australia = new Continent("Australia", (new List<Territory> { Eastern_Australia, Indonesia, New_Guinea, Western_Australia }), 2);
                Continents = new List<Continent> { North_America, South_America, Europe, Africa, Asia, Australia };
            }
            else if(Map != GameMap.Default)
            {
                GameGrid.Children.Clear();
                if (Map == GameMap.NewYork)
                {
                    img_Map.ImageSource = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/Maps/NewYork.jpg"));
                    img_Map.Stretch = Stretch.Uniform;
                    Button btnRockaway = SetupButton("btnRockaway", new Thickness(376, 395, 667, 98));
                    Button btnSaint_Albans = SetupButton("btnSaint_Albans", new Thickness(521, 458, 488, 36));
                    Button btnSouth_Queens = SetupButton("btnSouth_Queens", new Thickness(518, 384, 514, 110));
                    Button btnBayside = SetupButton("btnBayside", new Thickness(606, 445, 413, 49));
                    Button btnFlushing = SetupButton("btnFlushing", new Thickness(618, 386, 377, 108));
                    Button btnMiddle_Village = SetupButton("btnMiddle_Village", new Thickness(565, 340, 478, 154));
                    Button btnAstoria = SetupButton("btnAstoria", new Thickness(614, 305, 418, 189));
                    Button btnEast_New_York = SetupButton("btnEast_New_York", new Thickness(506, 339, 541, 155));
                    Button btnCanarsie = SetupButton("btnCanarsie", new Thickness(457, 325, 596, 171));
                    Button btnFlatlands = SetupButton("btnFlatlands", new Thickness(430, 299, 622, 194));
                    Button btnSheepshead_Bay = SetupButton("btnSheepshead_Bay", new Thickness(390, 276, 658, 217));
                    Button btnConey_Island = SetupButton("btnConey_Island", new Thickness(358, 252, 694, 242));
                    Button btnBay_Ridge = SetupButton("btnBay_Ridge", new Thickness(411, 208, 632, 286));
                    Button btnFlatbush = SetupButton("btnFlatbush", new Thickness(469, 283, 585, 211));
                    Button btnPark_Slope = SetupButton("btnPark_Slope", new Thickness(494, 244, 564, 254));
                    Button btnBorough_Park = SetupButton("btnBorough_Park", new Thickness(441, 243, 613, 251));
                    Button btnBedford_Stuyvesant = SetupButton("btnBedford_Stuyvesant", new Thickness(522, 282, 528, 212));
                    Button btnRed_Hook = SetupButton("btnRed_Hook", new Thickness(520, 230, 534, 268));
                    Button btnWilliamsburg = SetupButton("btnWilliamsburg", new Thickness(562, 266, 486, 228));
                    Button btnWoodrow = SetupButton("btnWoodrow", new Thickness(274, 55, 747, 439));
                    Button btnDongan_Hills = SetupButton("btnDongan_Hills", new Thickness(367, 163, 665, 331));
                    Button btnPort_Richmond = SetupButton("btnPort_Richmond", new Thickness(413, 129, 625, 365));
                    Button btnEmerson_Hill = SetupButton("btnEmerson_Hill", new Thickness(350, 99, 683, 395));
                    Button btnBayonne = SetupButton("btnBayonne", new Thickness(474, 116, 578, 382));
                    Button btnJersey_City = SetupButton("btnJersey_City", new Thickness(544, 147, 488, 347));
                    Button btnNorth_Bergen = SetupButton("btnNorth_Bergen", new Thickness(637, 165, 392, 329));
                    Button btnHoboken = SetupButton("btnHoboken", new Thickness(586, 186, 460, 312));
                    Button btnUnion_City = SetupButton("btnUnion_City", new Thickness(653, 199, 394, 300));
                    Button btnPalisades_Park = SetupButton("btnPalisades_Park", new Thickness(742, 211, 293, 283));
                    Button btnEnglewood = SetupButton("btnEnglewood", new Thickness(802, 224, 239, 270));
                    Button btnRiverdale = SetupButton("btnRiverdale", new Thickness(824, 293, 223, 201));
                    Button btnLaconia = SetupButton("btnLaconia", new Thickness(787, 337, 241, 157));
                    Button btnWashington_Heights = SetupButton("btnWashington_Heights", new Thickness(764, 266, 275, 232));
                    Button btnUnionport = SetupButton("btnUnionport", new Thickness(741, 356, 307, 138));
                    Button btnConcourse = SetupButton("btnConcourse", new Thickness(723, 293, 292, 201));
                    Button btnHarlem = SetupButton("btnHarlem", new Thickness(707, 256, 347, 244));
                    Button btnFinancial_District = SetupButton("btnFinancial_District", new Thickness(569, 210, 486, 288));
                    Button btnLower_East_Side = SetupButton("btnLower_East_Side", new Thickness(566, 240, 488, 259));
                    Button btnHells_Kitchen = SetupButton("btnHells_Kitchen", new Thickness(612, 210, 434, 291));
                    Button btnMidtown_East = SetupButton("btnMidtown_East", new Thickness(610, 234, 442, 266));
                    Button btnUpper_West_Side = SetupButton("btnUpper_West_Side", new Thickness(651, 228, 399, 273));
                    Button btnCentral_Park = SetupButton("btnCentral_Park", new Thickness(668, 247, 382, 253));
                    Button btnUpper_East_Side = SetupButton("btnUpper_East_Side", new Thickness(639, 257, 412, 244));
                    mapbuttons = new List<Button>
                    {
                        btnRockaway, btnSaint_Albans, btnBayside, btnSouth_Queens, btnFlushing, btnMiddle_Village, btnAstoria,
                        btnWoodrow, btnEmerson_Hill, btnDongan_Hills, btnPort_Richmond,
                        btnUnionport, btnLaconia, btnConcourse, btnRiverdale,
                        btnBayonne, btnJersey_City, btnHoboken, btnNorth_Bergen, btnUnion_City, btnPalisades_Park, btnEnglewood,
                        btnEast_New_York, btnCanarsie, btnBedford_Stuyvesant, btnFlatbush, btnFlatlands, btnSheepshead_Bay, btnBay_Ridge, btnConey_Island, btnPark_Slope, btnRed_Hook, btnWilliamsburg, btnBorough_Park,
                        btnLower_East_Side, btnFinancial_District, btnMidtown_East, btnHells_Kitchen, btnUpper_East_Side, btnUpper_West_Side, btnCentral_Park, btnHarlem, btnWashington_Heights
                    };
                    foreach(Button b in mapbuttons)
                    {
                        GameGrid.Children.Add(b);
                    }
                    GameGrid.UpdateLayout();
                    if (!load)
                    {
                        Territory Rockaway = new Territory("Rockaway", new List<String> { "Sheepshead_Bay", "South_Queens" }, btnRockaway);
                        Territory Saint_Albans = new Territory("Saint_Albans", new List<string> { "South_Queens", "Bayside" }, btnSaint_Albans);
                        Territory Bayside = new Territory("Bayside", new List<string> { "Saint_Albans", "Flushing", "South_Queens" }, btnBayside);
                        Territory South_Queens = new Territory("South_Queens", new List<string> { "East_New_York", "Rockaway", "Middle_Village", "Saint_Albans", "Bayside" }, btnSouth_Queens);
                        Territory Flushing = new Territory("Flushing", new List<string> { "Bayside", "Middle_Village", "Unionport" }, btnFlushing);
                        Territory Middle_Village = new Territory("Middle_Village", new List<string> { "Flushing", "Bayside", "South_Queens", "East_New_York", "Bedford_Stuyvesant", "Williamsburg", "Astoria" }, btnMiddle_Village);
                        Territory Astoria = new Territory("Astoria", new List<string> { "Middle_Village", "Upper_East_Side", "Concourse" }, btnAstoria);
                        Territory Woodrow = new Territory("Woodrow", new List<string> { "Emerson_Hill", "Dongan_Hills" }, btnWoodrow);
                        Territory Emerson_Hill = new Territory("Emerson_Hill", new List<string> { "Dongan_Hills", "Woodrow", "Port_Richmond" }, btnEmerson_Hill);
                        Territory Dongan_Hills = new Territory("Dongan_Hills", new List<string> { "Woodrow", "Emerson_Hill", "Port_Richmond", "Bay_Ridge" }, btnDongan_Hills);
                        Territory Port_Richmond = new Territory("Port_Richmond", new List<string> { "Bayonne", "Dongan_Hills", "Emerson_Hill" }, btnPort_Richmond);
                        Territory Unionport = new Territory("Unionport", new List<string> { "Flushing", "Laconia", "Concourse" }, btnUnionport);
                        Territory Laconia = new Territory("Laconia", new List<string> { "Unionport", "Concourse", "Riverdale" }, btnLaconia);
                        Territory Concourse = new Territory("Concourse", new List<string> { "Unionport", "Astoria", "Washington_Heights", "Riverdale", "Laconia" }, btnConcourse);
                        Territory Riverdale = new Territory("Riverdale", new List<string> { "Laconia", "Concourse" }, btnRiverdale);
                        Territory Bayonne = new Territory("Bayonne", new List<string> { "Port_Richmond", "Jersey_City" }, btnBayonne);
                        Territory Jersey_City = new Territory("Jersey_City", new List<string> { "Bayonne", "Hoboken", "North_Bergen" }, btnJersey_City);
                        Territory Hoboken = new Territory("Hoboken", new List<string> { "Jersey_City", "Union_City", "North_Bergen" }, btnHoboken);
                        Territory Union_City = new Territory("Union_City", new List<string> { "Hoboken", "North_Bergen" }, btnUnion_City);
                        Territory North_Bergen = new Territory("North_Bergen", new List<string> { "Hoboken", "Union_City", "Palisades_Park", "Jersey_City" }, btnNorth_Bergen);
                        Territory Palisades_Park = new Territory("Palisades_Park", new List<string> { "North_Bergen", "Washington_Heights", "Englewood" }, btnPalisades_Park);
                        Territory Englewood = new Territory("Englewood", new List<string> { "Palisades_Park" }, btnEnglewood);
                        Territory East_New_York = new Territory("East_New_York", new List<string> { "Canarsie", "Bedford_Stuyvesant", "Middle_Village", "South_Queens" }, btnEast_New_York);
                        Territory Canarsie = new Territory("Canarsie", new List<string> { "East_New_York", "Flatbush", "Bedford_Stuyvesant", "Flatlands" }, btnCanarsie);
                        Territory Bedford_Stuyvesant = new Territory("Bedford_Stuyvesant", new List<string> { "Canarsie", "East_New_York", "Flatbush", "Park_Slope", "Williamsburg", "Red_Hook", "Middle_Village" }, btnBedford_Stuyvesant);
                        Territory Flatbush = new Territory("Flatbush", new List<string> { "Canarsie", "Bedford_Stuyvesant", "Flatlands", "Borough_Park", "Park_Slope" }, btnFlatbush);
                        Territory Borough_Park = new Territory("Borough_Park", new List<string> { "Park_Slope", "Flatbush", "Flatlands", "Sheepshead_Bay", "Bay_Ridge" }, btnBorough_Park);
                        Territory Flatlands = new Territory("Flatlands", new List<string> { "Canarsie", "Flatbush", "Borough_Park", "Sheepshead_Bay" }, btnFlatlands);
                        Territory Sheepshead_Bay = new Territory("Sheepshead_Bay", new List<string> { "Bay_Ridge", "Borough_Park", "Flatlands", "Coney_Island", "Rockaway" }, btnSheepshead_Bay);
                        Territory Bay_Ridge = new Territory("Bay_Ridge", new List<string> { "Dongan_Hills", "Borough_Park", "Park_Slope", "Sheepshead_Bay" }, btnBay_Ridge);
                        Territory Coney_Island = new Territory("Coney_Island", new List<string> { "Sheepshead_Bay" }, btnConey_Island);
                        Territory Park_Slope = new Territory("Park_Slope", new List<string> { "Red_Hook", "Bedford_Stuyvesant", "Flatbush", "Borough_Park", "Bay_Ridge" }, btnPark_Slope);
                        Territory Red_Hook = new Territory("Red_Hook", new List<string> { "Park_Slope", "Bedford_Stuyvesant" }, btnRed_Hook);
                        Territory Williamsburg = new Territory("Williamsburg", new List<string> { "Bedford_Stuyvesant", "Lower_East_Side", "Middle_Village" }, btnWilliamsburg);
                        Territory Lower_East_Side = new Territory("Lower_East_Side", new List<string> { "Williamsburg", "Financial_District", "Midtown_East" }, btnLower_East_Side);
                        Territory Financial_District = new Territory("Financial_District", new List<string> { "Midtown_East", "Lower_East_Side", "Hells_Kitchen" }, btnFinancial_District);
                        Territory Midtown_East = new Territory("Midtown_East", new List<string> { "Financial_District", "Lower_East_Side", "Hells_Kitchen", "Upper_West_Side", "Central_Park", "Upper_East_Side" }, btnMidtown_East);
                        Territory Hells_Kitchen = new Territory("Hells_Kitchen", new List<string> { "Financial_District", "Midtown_East", "Upper_West_Side" }, btnHells_Kitchen);
                        Territory Upper_West_Side = new Territory("Upper_West_Side", new List<string> { "Midtown_East", "Hells_Kitchen", "Central_Park", "Harlem" }, btnUpper_West_Side);
                        Territory Central_Park = new Territory("Central_Park", new List<string> { "Midtown_East", "Harlem", "Upper_West_Side", "Upper_East_Side" }, btnCentral_Park);
                        Territory Upper_East_Side = new Territory("Upper_East_Side", new List<string> { "Midtown_East", "Harlem", "Central_Park", "Astoria" }, btnUpper_East_Side);
                        Territory Harlem = new Territory("Harlem", new List<string> { "Upper_West_Side", "Central_Park", "Upper_East_Side", "Washington_Heights" }, btnHarlem);
                        Territory Washington_Heights = new Territory("Washington_Heights", new List<string> { "Harlem", "Concourse", "Palisades_Park" }, btnWashington_Heights);
                        Territories = new List<Territory>
                        {
                        Rockaway, Saint_Albans, Bayside, South_Queens, Flushing, Middle_Village, Astoria,
                        Woodrow, Emerson_Hill, Dongan_Hills, Port_Richmond,
                        Unionport, Laconia, Concourse, Riverdale,
                        Bayonne, Jersey_City, Hoboken, North_Bergen, Union_City, Palisades_Park, Englewood,
                        East_New_York, Canarsie, Bedford_Stuyvesant, Flatbush, Flatlands, Sheepshead_Bay, Bay_Ridge, Coney_Island, Park_Slope, Red_Hook, Williamsburg, Borough_Park,
                        Lower_East_Side, Financial_District, Midtown_East, Hells_Kitchen, Upper_East_Side, Upper_West_Side, Central_Park, Harlem, Washington_Heights
                        };
                        Continent Queens = new Continent("Queens", new List<Territory> { Rockaway, Saint_Albans, Bayside, South_Queens, Flushing, Middle_Village, Astoria }, 5);
                        Continent Staten_Island = new Continent("Staten_Island", new List<Territory> { Woodrow, Emerson_Hill, Dongan_Hills, Port_Richmond }, 2);
                        Continent Bronx = new Continent("Bronx", new List<Territory> { Unionport, Laconia, Concourse, Riverdale }, 2);
                        Continent New_Jersey = new Continent("New_Jersey", new List<Territory> { Bayonne, Jersey_City, Hoboken, North_Bergen, Union_City, Palisades_Park, Englewood }, 3);
                        Continent Manhattan = new Continent("Manhattan", new List<Territory> { Lower_East_Side, Financial_District, Midtown_East, Hells_Kitchen, Upper_East_Side, Upper_West_Side, Central_Park, Harlem, Washington_Heights }, 5);
                        Continent Brooklyn = new Continent("Brooklyn", new List<Territory> { East_New_York, Canarsie, Bedford_Stuyvesant, Flatbush, Flatlands, Sheepshead_Bay, Bay_Ridge, Coney_Island, Park_Slope, Red_Hook, Williamsburg }, 7);
                        Continents = new List<Continent> { Queens, Staten_Island, Bronx, New_Jersey, Manhattan, Brooklyn };
                    }
                }
                else { throw new Exception("An error has occured"); }
            }
            if (load)
            {
                string buttonprefix = "btn";
                if(Map == GameMap.Default)
                {
                    foreach (Territory t in Territories)
                    {
                        String concat = buttonprefix + t.name;
                        t.button = (Button)GameGrid.FindName(concat);
                    }
                }
                else
                {
                    foreach(Territory t in Territories)
                    {
                        String concat = buttonprefix + t.name;
                        t.button = mapbuttons.Find(Button => Button.Name == concat);
                    }
                }
            }
        }
        private Button SetupButton(String name, Thickness margin)
        {
            Button b = new Button()
            {
                Name = name,
                Margin = margin,
                Content = "0",
                ToolTip = new ToolTip() { Content = name.Replace("_", " ").TrimStart(new char[] { 'b', 't', 'n' }) }
            };
            return b;
        }
        private void TimerSetup()
        {
            workerthread = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            workerthread.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
        // Game Setup and UI Management //
        private void SetupGame(bool randomise_initial)
        {
            // Determines how many armies players have.
            int playercount = 0;
            foreach(Player p in Players)
            {
                if(!(p is NeutralAI))
                {
                    playercount += 1;
                }
            }
            int initialarmies = (50 - (5 * playercount));
            CurrentPlayer = Players[0];
            // Setup Board and initial armies //
            UISetup();
            foreach (Player p in Players) { p.army_undeployed = initialarmies; }
            UpdateState(GameState.InitialArmyPlace);
            if (randomise_initial == true)
            {
                SetupRandom();
                Territories.Sort();
                StartGame();
            }
            else
            {
                Territories.Sort();
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
            Gamestate = GameState.PlacingArmy;
            NextTurnThreaded();
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
                        if(p.army_undeployed > 0)
                        {
                            if(t.owner == p)
                            {
                                Place_Reinforce(t, rng.Next(1, Math.Min(p.army_undeployed,4)));
                            }
                        }
                        else { break; }
                    }
                }
            }
        }
        private void NeutralAISetup()
        {
            bool alltaken = true;
            foreach (Territory t in Territories)
            {
                // ensures each territory is owned by a player.
                bool assigned = false;
                do
                {
                    if (t.owner != null) { assigned = true; break; }
                    CycleNeutralAI();
                    if (CurrentPlayer.army_undeployed > 0)
                    {
                        alltaken = false;
                        Place_Reinforce(t, 1);
                        assigned = true;
                    }
                } while (assigned == false);
            }
            if (alltaken == false)
            {
                foreach (Player p in Players)
                {
                    if (p is NeutralAI)
                    {
                        CurrentPlayer = p;
                        while (p.army_undeployed > 0)
                        {
                            foreach (Territory t in Territories)
                            {
                                if (p.army_undeployed > 0)
                                {
                                    if (t.owner == p)
                                    {
                                        Place_Reinforce(t, rng.Next(1, Math.Min(p.army_undeployed, 4)));
                                    }
                                }
                                else { break; }
                            }
                        }
                    }
                }
            }
            // Places remaining armies around map in friendly territory until there are none left //
        }
        private void CycleNeutralAI()
        {
            CyclePlayers();
            if (!(CurrentPlayer is NeutralAI))
            {
                CycleNeutralAI();
            }
        }
        // Game Start UI setup //
        private void UISetup()
        {
            // This code sets up the "player panel" with the players details, resizing certain elements to avoid white borders.
            lblPlayerName1.Content = Players[0].Username;
            Players[0].Disp_ArmyStrength = lblPlayer1Strength;
            Players[0].Disp_Owned = lblPlayer1Territories;
            lblPlayerName2.Content = Players[1].Username;
            Players[1].Disp_ArmyStrength = lblPlayer2Strength;
            Players[1].Disp_Owned = lblPlayer2Territories;
            rectPlayerColor1.Fill = (SolidColorBrush)Players[0].Color;
            rectPlayerColor2.Fill = (SolidColorBrush)Players[1].Color;
            lblPlayerName3.Content = Players[2].Username;
            rectPlayerColor3.Fill = (SolidColorBrush)Players[2].Color;
            Players[2].Disp_ArmyStrength = lblPlayer3Strength;
            Players[2].Disp_Owned = lblPlayer3Territories;
            // make this more efficient
            if (Players.Count >= 4)
            {
                lblPlayerName4.Content = Players[3].Username;
                rectPlayerColor4.Fill = (SolidColorBrush)Players[3].Color;
                brd_Player4.Visibility = Visibility.Visible;
                Players[3].Disp_ArmyStrength = lblPlayer4Strength;
                Players[3].Disp_Owned = lblPlayer4Territories;
                if (Players.Count >= 5)
                {
                    int fs;
                    int rect_height = 17;
                    Thickness th = new Thickness(10, 3, 0, 3);
                    int gap = 2;
                    lblPlayerName5.Content = Players[4].Username;
                    rectPlayerColor5.Fill = (SolidColorBrush)Players[4].Color;
                    brd_Player5.Visibility = Visibility.Visible;
                    Players[4].Disp_ArmyStrength = lblPlayer5Strength;
                    Players[4].Disp_Owned = lblPlayer5Territories;
                    if (Players.Count >= 6)
                    {
                        fs = 11;
                        gap = 0;
                        rectPlayerColor6.Fill = (SolidColorBrush)Players[5].Color;
                        lblPlayerName6.Content = Players[5].Username;
                        brd_Player6.Visibility = Visibility.Visible;
                        Players[5].Disp_ArmyStrength = lblPlayer6Strength;
                        Players[5].Disp_Owned = lblPlayer6Territories;
                        SetFontSize(fs);
                    }
                    SetGap(gap);
                    SetRect(rect_height, th);
                }
            }
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
        private void SetRect(int i, Thickness margin)
        {
            rectPlayerColor1.Height = i;
            rectPlayerColor2.Height = i;
            rectPlayerColor3.Height = i;
            rectPlayerColor4.Height = i;
            rectPlayerColor5.Height = i;
            rectPlayerColor6.Height = i;
            rectPlayerColor1.Margin = margin;
            rectPlayerColor2.Margin = margin;
            rectPlayerColor3.Margin = margin;
            rectPlayerColor4.Margin = margin;
            rectPlayerColor5.Margin = margin;
            rectPlayerColor6.Margin = margin;
        }
        private void SetGap(int i)
        {
            Thickness gap;
            if (i == 0)
            {
                gap = new Thickness(0, 0, 0, 0);
                brd_Player1.Margin = gap;
                brd_Players.BorderThickness = new Thickness(0);
            }
            else
            {
                gap = new Thickness(5, 0, 5, i);
                brd_Player1.Margin = new Thickness(5, i, 5, i);
            }
            brd_Player2.Margin = gap;
            brd_Player3.Margin = gap;
            brd_Player4.Margin = gap;
            brd_Player5.Margin = gap;
            brd_Player6.Margin = gap;
        }

        //// Game Methods ////
        private void CyclePlayers()
        { // Cycles through the list of players, for a new turn or placing armies.
            if ((Players.IndexOf(CurrentPlayer) + 1) == (Players.Count)) { CurrentPlayer = Players[0]; }
            else { CurrentPlayer = Players[(Players.IndexOf(CurrentPlayer) + 1)]; }
            UpdatePlayerPanelUI();
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
            if(Time > 0)
            {
                if (Gamestate == GameState.InitialArmyPlace) { NextTurnThreaded(); return; }
                if (workerthread != null && workerthread.IsBusy == true)
                {
                    workerthread.CancelAsync();
                }
                else
                {
                    NextTurnThreaded();
                }
            }
            else
            {
                NextTurnThreaded();
            }
        }
        private void NextTurnThreaded()
        {
            ClearSelectionsUI();
            if (Gamestate == GameState.InitialArmyPlace)
            {
                if ((!(CurrentPlayer is NeutralAI)) && CurrentPlayer.army_undeployed > 0)
                {
                    Output(String.Format("It is now {0}'s turn.", CurrentPlayer.Username));
                }
                else
                {
                    if (CurrentPlayer is NeutralAI) { CyclePlayers(); NextTurn(); }
                    if (AllPlaced())
                    {
                        foreach(Player p in Players)
                        {
                            if(p is NeutralAI)
                            {
                                NeutralAISetup();
                                break;
                            }
                        }
                        Territories.Sort();
                        CurrentPlayer = Players[Players.Count - 1];
                        StartGame();
                    }
                }
            }
            else
            {
                Turn += 1;
                CyclePlayers();
                if(CurrentPlayer is NeutralAI)
                {
                    NextTurn();
                }
                else
                {
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
                    CurrentPlayer.army_undeployed += ((CurrentPlayer.Territoriesowned / 3) + bonus);
                    UpdateState(GameState.PlacingArmy);
                    // simplify
                    switch (ownedContinents.Count)
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
                    if (Time > 0) { StartTimer(); }
                }
            }
        }
        private void Win()
        {
            CurrentPlayer.score += CurrentPlayer.Army_strength / 3;
            int finalscore = CurrentPlayer.score / Turn;
            GameDetails gamedetails = new GameDetails(DateTime.Now.ToString(), CurrentPlayer.Username, Players.Count.ToString(), finalscore.ToString(),Turn.ToString(), Map.ToString(), Gamemode.ToString());
            GameDetails.Save(gamedetails);
            GameManager.DeleteGame(game.GameID);
            Highscores finish = new Highscores(gamedetails, Players);
            App.Current.MainWindow = finish;
            this.Close();
            finish.Show();
        }

        //// UI /////
        private void LoadPlayerUI()
        {  // use binding in future
            UISetup();
            UpdatePlayerPanelUI();
            try
            {
                MapSetup(true);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred loading the map.");
                this.Close();
                return;
            }
            UpdateState(game.gameState);
            if (Gamestate == GameState.PlacingArmy)
            {
                UpdatePlayerUndeployed();
            }
            foreach (Territory t in Territories)
            {
                t.button.Background = t.owner.Color;
                t.button.Content = t.currentarmies;
            }
        }
        private void ConquerTerritoryUI()
        {
            NextTerritory.button.Background = NextTerritory.owner.Color;
            NextTerritory.button.Content = NextTerritory.temparmies;
        } // Updates nextTerritory UI for Conquer
        private void AttackMoveTerritoryUI()
        {
            SlctTerritory.button.Content = SlctTerritory.currentarmies;
        } // Updates selected territory's armies
        private void UpdateNumOutput()
        {
            switch (Gamestate)
            {
                case GameState.PlacingArmy:
                    btnNumber.Content = SlctTerritory.temparmies;
                    break;
                case GameState.Attacking:
                    btnNumber.Content = NextTerritory.temparmies;
                    break;
                case GameState.Conquer:
                    btnNumber.Content = NextTerritory.temparmies;
                    break;
                case GameState.Move:
                    btnNumber.Content = NextTerritory.temparmies;
                    break;
            }
        } // Updates UI Label Number
        private void UpdatePlayerPanelUI()
        {
            int i = Players.IndexOf(CurrentPlayer);
            foreach (Border b in panel_Players.Children) {
                b.Background = panel_Players.Background;
            }
            panel_UI.Background = CurrentPlayer.Color;
            switch (i)
            {
                case 0:
                    brd_Player1.Background = Brushes.LightBlue;
                    break;
                case 1:
                    brd_Player2.Background = Brushes.LightBlue;
                    break;
                case 2:
                    brd_Player3.Background = Brushes.LightBlue;
                    break;
                case 3:
                    brd_Player4.Background = Brushes.LightBlue;
                    break;
                case 4:
                    brd_Player5.Background = Brushes.LightBlue;
                    break;
                case 5:
                    brd_Player6.Background = Brushes.LightBlue;
                    break;
            }
        }  // Updates currently highlighted player in UI stack
        private void UpdatePlayerUndeployed()
        { // Updates the UI to show the player's undeployed armies.
            Output(String.Format("You have {0} armies to place.", CurrentPlayer.army_undeployed));
        } // Use to update undeployed on place
        private void UpdateStateUI()
        {
            switch (Gamestate)
            {
                case GameState.Attacking:
                    btnStateDisp.Content = "Attack";
                    btnStateDisp.Background = new SolidColorBrush(Color.FromRgb(235, 64, 45));
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
                    btnStateDisp.Content = "Setup Game Board";
                    btnStateDisp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B9FF"));
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
                    btnStateDisp.Content = "Place Armies";
                    btnStateDisp.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00B9FF"));
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
                    btnStateDisp.Content = "Move";
                    btnStateDisp.Background = new SolidColorBrush(Color.FromRgb(245, 245, 78));
                    btnState.Content = "Confirm Fortify";
                    if (CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output("Click to select a territory");
                            Output("to move armies from.");
                            Output("Click again to select where you");
                            Output("wish to move them to.");
                            Output("Use +- and L/R click");
                            Output("to adjust the number moved.");
                        }
                    }
                    break;
                case GameState.Conquer:
                    btnStateDisp.Content = "Conquer";
                    btnStateDisp.Background = new SolidColorBrush(Color.FromRgb(50, 250, 93));
                    btnState.Content = "Confirm Conquer";
                    if (CurrentPlayer is Human)
                    {
                        if (((Human)CurrentPlayer).hints_enabled)
                        {
                            Output("Use Click, Right-Click, + and -");
                            Output("to move armies to or from");
                            Output("your new territory.");
                        }
                    }
                    break;
            }
        } // Start of turn instructions and UI State change
        private void ClearSelectionsUI()
        {
            foreach(Territory t in Territories)
            {
                t.button.BorderBrush = Brushes.Black;
            }
            btnNumber.Content = 0;
            ClearSelections();
        } // Clears Selections and UI

        //// Backend Methods ////
        private Territory RetrieveTerritory(String territoryname)
        {
            territoryname = territoryname.Replace(' ', '_');
            try
            {
                // Binary Search //
                int start = 0;
                int end = Territories.Count - 1;
                while (start <= end)
                {
                    int mid = Decimal.ToInt32(Math.Floor((decimal)(start + end) / 2));
                    if (String.Compare(territoryname, Territories[mid].name) < 0)
                    {
                        end = mid - 1;
                    }
                    else if(String.Compare(territoryname, Territories[mid].name) > 0)
                    {
                        start = mid + 1;
                    }
                    else
                    {
                        return Territories[mid];
                    }
                }
                throw new TerritoryNotFoundException();
            }
            catch (TerritoryNotFoundException)
            {
                foreach(Territory t in Territories)
                {
                    if(t.name == territoryname)
                    {
                        return t;
                    }
                }
                throw new Exception();
            }
        }
        private void SelectTerritory(Territory t, Button b, Brush color, bool next)
        {
            if (next) { NextTerritory = t; }
            else { SlctTerritory = t; }
            b.BorderBrush = color;
        }
        private void Output(String s)
        {
            if ((txtOutput.Text == "") || (txtOutput.Text == null)) { txtOutput.Text = s; }
            else
            {
                String[] tmp = txtOutput.Text.Split('\n');
                if (tmp.Length >= 6)
                {
                    tmp[0] = tmp[1];
                    for (int i = 1; i < (tmp.Length - 1); i++)
                    {
                        tmp[i] = "\n" + tmp[i + 1];
                    }
                    tmp[5] = ("\n" + s);
                    txtOutput.Text = tmp[0] + tmp[1] + tmp[2] + tmp[3] + tmp[4] + tmp[5];
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
            Gamestate = g;
            if(Gamestate == GameState.Conquer) { ConquerTerritoryUI(); }
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
            if(i >= 1) // moving out of slct
            {
                if (SlctTerritory.currentarmies < 2)
                {
                    Output("At least one army must remain in a friendly territory.");
                    return;
                }
                else if(Gamemode == GameMode.Classic && Gamestate == GameState.Attacking && NextTerritory.temparmies >= 3)
                {
                    Output("You can attack with a maximum of 3 armies at once.");
                    return;
                }
            }
            if(i <= -1) // moving back to slct
            {
                if(NextTerritory.temparmies <= 1)
                {
                    switch (Gamestate)
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
                else if(Gamemode == GameMode.Classic && Gamestate == GameState.Conquer)
                {
                    int count = 0;
                    foreach(Dice d in dices) { if (d is PlayerDice) { count += 1; } }
                    if(NextTerritory.temparmies <= count)
                    {
                        Output("You must move at least as many armies used to attack into the new territory.");
                        return;
                    }
                }
            }
            SlctTerritory.currentarmies -= i;
            NextTerritory.temparmies += i;
            UpdateNumOutput();
            AttackMoveTerritoryUI();
            if(Gamestate == GameState.Conquer) { ConquerTerritoryUI(); }
        }
        private void CancelUnconfirmedActions()
        {
            switch (Gamestate)
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
                    ClearSelectionsUI();
                    break;
                case GameState.Attacking:
                    if(SlctTerritory != null)
                    {
                        if(NextTerritory != null)
                        {
                            SlctTerritory.currentarmies += NextTerritory.temparmies;
                            NextTerritory.temparmies = 0;
                            SlctTerritory.button.Content = SlctTerritory.currentarmies;
                            NextTerritory.button.Content = NextTerritory.currentarmies;
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
                            AttackMoveTerritoryUI();
                            NextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI();
                    }
                    break;
            }
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
        private bool DieOpen()
        {
            if(panel_Die.Visibility == Visibility.Visible)
            {
                if((String)btnDieStatus.Content == "Continue to Attack")
                {
                    Output("Click \"continue to attack\" to proceed.");
                }
                else if((String)btnDieStatus.Content == "Continue to Conquer")
                {
                    Output("You must conquer the territory.");
                }
                return true;
            }
            return false;
        }
        // Timer Control //
        private void StartTimer()
        {
            pb_Timer.Value = 0;
            workerthread.CancelAsync();
            workerthread.RunWorkerAsync();
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i < Time; i++)
            {
                if (workerthread.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    while(paused == true) { Thread.Sleep(100); }
                    int progressPercentage = Convert.ToInt32(((double)i / Time) * 100);
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                    Thread.Sleep(10);
                }
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb_Timer.Value = e.ProgressPercentage;
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Gamestate == GameState.Conquer) { Output("Move your armies to end your turn."); }
            else if (!DieOpen())
            {
                CancelUnconfirmedActions();
                NextTurnThreaded();
            }
        }

        ////  Button Events  ////
        private void Click(object sender, RoutedEventArgs e)
        {   // Called when a territory is clicked on and performs an action based on the
            // context in which it was clicked.
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' }));
            Button btnTerritory = t.button;
            switch (Gamestate)
            {
                case GameState.InitialArmyPlace:
                    SlctTerritory = t;
                    if (SlctTerritory.owner == null || SlctTerritory.owner == CurrentPlayer)
                    {
                        Place_Reinforce(SlctTerritory, 1);
                        CyclePlayers();
                        NextTurn();
                    }
                    else { Output("You cannot capture this territory."); SlctTerritory = null; } // use clear selections to prevent bugs
                    break;
                case GameState.PlacingArmy:
                    if(t == SlctTerritory) { PlayerActions(true); break; }
                    else
                    {
                        ClearSelectionsUI();
                        if (t.owner == null || t.owner == CurrentPlayer)
                        {
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                            PlayerActions(true);
                            UpdateNumOutput();
                        }
                        else { Output("This is not your territory."); SlctTerritory = null; }
                        break;
                    }
                case GameState.Attacking:
                    if (DieOpen()) { return; }
                    if (t.owner == CurrentPlayer)
                    {
                        if(t.currentarmies > 1)
                        {
                            CancelUnconfirmedActions();
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                            ShowAttack();
                        }
                        else { Output("You do not have enough armies to attack from here."); break; }
                    }
                    else if (SlctTerritory != null) // if a territory is selected
                    {
                        if(t.owner != null)
                        {
                            if(btnTerritory.BorderBrush == Brushes.Aqua)
                            {
                                SelectTerritory(t, btnTerritory, Brushes.Red, true);
                                if(Gamemode == GameMode.NewRisk) { AdjustAttackMoves((SlctTerritory.currentarmies - 1)); }
                                else if(Gamemode == GameMode.Classic) { AdjustAttackMoves(Math.Min(SlctTerritory.currentarmies - 1, 3)); }
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
                        List<Territory> blank = new List<Territory>(); // redundant?
                        scanterritories = blank;
                    }
                    else if(btnTerritory.BorderBrush == Brushes.Green)
                    {
                        PlayerActions(true);
                        return;
                    }
                    else if (btnTerritory.BorderBrush == Brushes.Aqua)
                    {
                        if(NextTerritory != null)
                        {
                            Output("You must finish or cancel your current move");
                            Output("before selecting another territory");
                            return;
                        }
                        SelectTerritory(t, btnTerritory, Brushes.Green, true);
                        AdjustAttackMoves(1);
                    }
                    else { Output("You cannot move armies to here from your selected territory."); }
                    break;
                case GameState.Conquer:
                    try
                    {
                        if (t == NextTerritory) { PlayerActions(true); }
                        else if (t == SlctTerritory) { PlayerActions(false); }
                        break;
                    }
                    catch (NullReferenceException) { break; }
            }
        }
        private void RightClick(object sender, MouseEventArgs e)
        {
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' }));
            Button btnTerritory = t.button;
            switch (Gamestate)
            {
                case GameState.PlacingArmy:
                    if (t == SlctTerritory) { PlayerActions(false); }
                    // error will be output if temp = 0 from player actions script as is shared by decrease button.
                    break;
                case GameState.Attacking:
                    // error is output by adjust attack moves if invalid
                    try
                    {
                        if (!DieOpen())
                        {
                        // if nextterritory null break otherwise continue
                            if (t == NextTerritory) { PlayerActions(false); }
                            else if (t == SlctTerritory)
                            {
                                if (Gamemode == GameMode.NewRisk && (NextTerritory.temparmies == 3)) { Output("You cannot attack with more than 3 armies at a time."); return; } // fix this should be classic risk
                                else { PlayerActions(true); }
                            }
                            break;
                        }
                    }
                    catch (NullReferenceException) { break; }
                    break;
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
            switch (Gamestate)
            {
                case GameState.PlacingArmy:
                    foreach(Territory t in Territories)
                    {
                        if(t.owner == CurrentPlayer)
                        {
                            Place_Reinforce(t, t.temparmies);
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
                        if(Gamemode == GameMode.NewRisk)
                        {
                            double num = rng.NextDouble();
                            double prob = (double)1 / (1 + Math.Exp(-3 * (((double)(NextTerritory.temparmies - NextTerritory.currentarmies)/NextTerritory.currentarmies) - DefenseBias)));
                            if (num <= prob)
                            {
                                NextTerritory.owner.Territoriesowned -= 1;
                                NextTerritory.owner.score -= 1;
                                NextTerritory.owner.Army_strength -= NextTerritory.currentarmies;
                                NextTerritory.currentarmies = 0;
                                NextTerritory.owner = CurrentPlayer;
                                CurrentPlayer.Territoriesowned += 1;
                                CurrentPlayer.score += 1;
                                bool won = true;
                                foreach (Player p in Players)
                                {
                                    if (p != CurrentPlayer && p.Territoriesowned > 0) { won = false; break; }
                                }
                                if (won) { Win(); }
                                int lost = (int)Math.Floor((1-prob) * (Double)NextTerritory.temparmies);
                                NextTerritory.temparmies -= lost;
                                Output(String.Format("You have captured this territory and lost {0} armies in battle.", lost));
                                UpdateState(GameState.Conquer);
                            }
                            else
                            {
                                NextTerritory.temparmies = 0;
                                int survived = (int)(Math.Ceiling((1 - prob) * (Double)NextTerritory.currentarmies));
                                int loss = NextTerritory.currentarmies - survived;
                                Output(String.Format("You have lost this battle, the enemy suffered {0} casualties.", loss));
                                NextTerritory.currentarmies = survived; // can be simplified /\
                                NextTerritory.button.Content = NextTerritory.currentarmies;
                                ClearSelectionsUI();
                            }
                        }
                        // Classic Mode
                        else if(Gamemode == GameMode.Classic)
                        {
                            if (DieOpen()) { return; }
                            panel_NumberSelection.Visibility = Visibility.Collapsed;
                            panel_Die.Visibility = Visibility.Visible;
                            dices.Clear();
                            dices = new List<Dice>{ playerdie1, enemydie1 };
                            if(NextTerritory.currentarmies > 1)
                            {
                                imgEnemyDie2.Visibility = Visibility.Visible;
                                dices.Add(enemydie2);
                            }
                            else { imgEnemyDie2.Visibility = Visibility.Collapsed; }
                            if(NextTerritory.temparmies > 1) { imgPlayerDie2.Visibility = Visibility.Visible; dices.Add(playerdie2); }
                            else { imgPlayerDie2.Visibility = Visibility.Collapsed; }
                            if(NextTerritory.temparmies > 2) { imgPlayerDie3.Visibility = Visibility.Visible; dices.Add(playerdie3); }
                            else { imgPlayerDie3.Visibility = Visibility.Collapsed; }
                            btnPlayerDie.Content = CurrentPlayer.Username;
                            btnEnemyDie.Content = NextTerritory.owner.Username;
                            // style setup //
                            Style diestyle = panel_Die.Resources["btnDie"] as Style;
                            Style playerdiestyle = new Style(typeof(Button) ,diestyle);
                            playerdiestyle.Setters.Add(new Setter(Border.BackgroundProperty, CurrentPlayer.Color));
                            panel_Die.Resources["btnPlayerDie"] = playerdiestyle;
                            Style enemydiestyle = new Style(typeof(Button), diestyle);
                            enemydiestyle.Setters.Add(new Setter(Border.BackgroundProperty, NextTerritory.owner.Color));
                            panel_Die.Resources["btnEnemyDie"] = enemydiestyle;
                            ToRoll = dices.Count;
                            Rolled = 0;
                            paused = true;
                            foreach (Dice d in dices)
                            {
                                d.workerthread.RunWorkerCompleted += DieRollComplete;
                                d.StartRoll();
                            }
                            // make invisible at end of roll
                        }
                    }
                    else { Output("You must select the territories you wish to attack to/from first."); }
                    break;
                case GameState.Conquer:
                    Place_Reinforce(NextTerritory, NextTerritory.temparmies);
                    NextTerritory.temparmies = 0;
                    NextTerritory.button.Background = NextTerritory.owner.Color;
                    NextTerritory.button.Content = NextTerritory.currentarmies;
                    if (Time > 0)
                    {
                        if (workerthread.IsBusy == false) { NextTurnThreaded(); return; }
                    }
                    NextAction();
                    break;
                case GameState.Move:
                    if(SlctTerritory == null) { Output("You must select a territory to move armies from."); }
                    else if(NextTerritory == null) { Output("You must select a territory to move armies to."); }
                    else
                    {
                        Place_Reinforce(NextTerritory, NextTerritory.temparmies);
                        NextTerritory.temparmies = 0;
                        NextAction();
                    }
                    break;
            }
        }
        private void Continue(object sender, RoutedEventArgs e)
        {
            if(panel_Die.Visibility == Visibility.Visible)
            {
                if ((String)btnDieStatus.Content == "Continue to Attack")
                {
                    btnDieStatus.Visibility = Visibility.Collapsed; // redundant?
                    panel_Die.Visibility = Visibility.Collapsed;
                    panel_NumberSelection.Visibility = Visibility.Visible;
                    CancelUnconfirmedActions();
                    btnDieStatus.Content = "Outcome";
                    if (Time > 0)
                    {
                        if (workerthread.IsBusy == false) { NextTurnThreaded(); return; }
                    }
                }
                else if ((String)btnDieStatus.Content == "Continue to Conquer")
                {
                    Output("You must continue to conquer.");
                    return;
                }
            }
            else if (Gamestate != GameState.Conquer) { CancelUnconfirmedActions(); NextAction(); }
            else { Output("You must finish conquering the territory."); }
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            if (DieOpen()) { return; }
            CancelUnconfirmedActions();
        }
        private void Increase(object sender, RoutedEventArgs e) { PlayerActions(true); }
        private void Decrease(object sender, RoutedEventArgs e) { PlayerActions(false); }
        private void Settings(object sender, RoutedEventArgs e)
        {
            paused = true;
            panel_MainUI.Visibility = Visibility.Collapsed;
            panel_Settings.Visibility = Visibility.Visible;
        }
        private void Return(object sender, RoutedEventArgs e)
        {
            paused = false;
            panel_MainUI.Visibility = Visibility.Visible;
            panel_Settings.Visibility = Visibility.Collapsed;
        }

        ////  Player Actions ////
        private void Place_Reinforce(Territory T, int num)
        {
            if (T.owner != CurrentPlayer)
            {
                // Update UI to reflect ownership
                if (T.owner != null) { T.owner.Territoriesowned -= 1; }
                T.owner = CurrentPlayer;
                T.button.Background = T.owner.Color;
                CurrentPlayer.Territoriesowned += num;
                CurrentPlayer.score += 1;
            }
            // Sets up game-board, sets owner and places army into territory
            if ((Gamestate == GameState.InitialArmyPlace) || (Gamestate == GameState.PlacingArmy)) { CurrentPlayer.Army_strength += num; }
            T.currentarmies += num;
            if(Gamestate == GameState.InitialArmyPlace) { CurrentPlayer.army_undeployed -= num; }
            T.button.Content = T.currentarmies;
        }
        private void NextAction()
        {
            ClearSelectionsUI();
            switch (Gamestate)
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
                switch (Gamestate)
                {
                    case GameState.PlacingArmy:
                        // if increase == false ensure no negatives
                        if (increase == true)
                        {
                            if (CurrentPlayer.army_undeployed > 0) { i = 1; }
                            else
                            {
                                Output("You do not have any armies left to place");
                                break;
                            }
                        }
                        else
                        {
                            if(SlctTerritory.temparmies <= 1)
                            {
                                Output("You must place at least one army");
                                break;
                            }
                        }
                        SlctTerritory.temparmies += i;
                        CurrentPlayer.army_undeployed -= i;
                        UpdateNumOutput();
                        break;
                    case GameState.Attacking:
                        if (NextTerritory != null)
                        {
                            if (increase == true) { i = 1; }
                            AdjustAttackMoves(i);
                        }
                        else { Output("You must select the territories you wish to attack to/from."); }
                        break;
                    case GameState.Conquer:
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
            if(Gamestate == GameState.InitialArmyPlace) { Output("You must finish setup before attempting to save."); }
            else if(Gamestate == GameState.Conquer) { Output("You must finish conquering before saving."); }
            else if ((Gamestate == GameState.Attacking) && (Gamemode == GameMode.Classic)) { Output("You must finish your attack before saving."); }
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


        // die rolling

        private void DieRollComplete(object sender, RunWorkerCompletedEventArgs e) { Rolled += 1; }

        private int rolled;
        public int Rolled
        {
            get { return rolled; }
            set
            {
                rolled = value;
                if(rolled == toRoll)
                {
                    AllRollsCompleted();
                }
            }
        }

        private int toRoll;
        public int ToRoll
        {
            get { return toRoll; }
            set { toRoll = value; }
        }
        private void AllRollsCompleted()
        {
            int playerHighestRoll = playerdie1.current;
            int enemyHighestRoll = enemydie1.current;
            int playerNextHighest = -1;
            int enemyNextHighest = -1;
            if(playerdie2.current != -1)
            {
                DetermineHigherRoll(ref playerHighestRoll, ref playerNextHighest, playerdie2.current);
                if (playerdie3.current != -1) { DetermineHigherRoll(ref playerHighestRoll, ref playerNextHighest, playerdie3.current); }
                if (enemydie2.current != -1) { DetermineHigherRoll(ref enemyHighestRoll, ref enemyNextHighest, enemydie2.current); }
            }
            foreach(Dice d in dices)
            {
                d.current = -1;
            }
            int playerloss = 0;
            int enemyloss = 0;
            if((playerNextHighest != -1) && (enemyNextHighest != -1))
            {
                if(ClassicBattle(playerNextHighest, enemyNextHighest)) { enemyloss += 1; }
                else { playerloss += 1; }
            }
            if(ClassicBattle(playerHighestRoll, enemyHighestRoll)) { enemyloss += 1; }
            else { playerloss += 1; }
            CurrentPlayer.Army_strength -= playerloss;
            NextTerritory.temparmies -= playerloss;
            NextTerritory.owner.Army_strength -= enemyloss;
            NextTerritory.currentarmies -= enemyloss;
            NextTerritory.button.Content = NextTerritory.currentarmies;
            Output(String.Format("You lost {0} armies in battle. The enemy lost {1}", playerloss, enemyloss));
            if (NextTerritory.currentarmies == 0)
            {
                Output("You have successfully captured this territory.");
                btnDieStatus.Content = "Continue to Conquer";
            }
            else { btnDieStatus.Content = "Continue to Attack"; }
            btnDieStatus.Visibility = Visibility.Visible;
            paused = false;
        }
        private void Attack_DieContinue(object sender, RoutedEventArgs e)
        {
            if (((String)((Button)sender).Content) == "Dice Rolling...")
            {
                return;
            }
            btnDieStatus.Visibility = Visibility.Collapsed; // redundant?
            panel_Die.Visibility = Visibility.Collapsed;
            panel_NumberSelection.Visibility = Visibility.Visible;
            if (((String)((Button)sender).Content) == "Continue to Attack")
            {
                CancelUnconfirmedActions();
                if (Time > 0)
                {
                    if (workerthread.IsBusy == false) { NextTurnThreaded(); return; }
                }
            }
            else
            {
                NextTerritory.owner.Territoriesowned -= 1;
                NextTerritory.owner.score -= 1;
                NextTerritory.owner.Army_strength -= NextTerritory.currentarmies;
                NextTerritory.owner = CurrentPlayer;
                CurrentPlayer.Territoriesowned += 1;
                CurrentPlayer.score += 1;
                bool won = true;
                foreach (Player p in Players)
                {
                    if (p != CurrentPlayer && p.Territoriesowned > 0) { won = false; }
                }
                if (won) { Win(); }
                UpdateState(GameState.Conquer);
            }
            btnDieStatus.Content = "Outcome";
        }
        private void DetermineHigherRoll(ref int greatest, ref int secondgreatest, int newnum)
        {
            if(newnum > secondgreatest)
            {
                if(newnum > greatest)
                {
                    secondgreatest = greatest;
                    greatest = newnum;
                }
                else { secondgreatest = newnum; }
            }
        }
        private bool ClassicBattle(int player, int enemy)
        {
            if(enemy >= player) { return false; }
            else { return true; }
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
