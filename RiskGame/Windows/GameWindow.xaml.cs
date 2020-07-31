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
        #region Variables and Properties
        /// <summary>
        /// Current game object
        /// </summary>
        private GameManager game;
        /// <summary>
        /// Accessor for list of game players
        /// </summary>
        private List<Player> Players
        {
            get { return game.players; }
            set { game.players = value; }
        }
        /// <summary>
        /// Accessor for list of game territories
        /// </summary>
        private List<Territory> Territories
        {
            get { return game.territories; }
            set { game.territories = value; }
        }
        /// <summary>
        /// Accessor for list of game continents.
        /// </summary>
        private List<Continent> Continents
        {
            get { return game.continents; }
            set { game.continents = value; }
        }
        /// <summary>
        /// Accessor for current selected territory.
        /// </summary>
        private Territory SlctTerritory
        {
            get { return game.slctTerritory; }
            set { game.slctTerritory = value; }
        }
        /// <summary>
        /// Accessor for second selected territory.
        /// </summary>
        private Territory NextTerritory
        {
            get { return game.nextTerritory; }
            set { game.nextTerritory = value; }
        }
        /// <summary>
        /// Accessor for current player
        /// </summary>
        private Player CurrentPlayer
        {
            get { return game.currentplayer; }
            set { game.currentplayer = value; }
        }
        /// <summary>
        /// Accessor for current game-state.
        /// </summary>
        private GameState Gamestate
        {
            get { return game.gameState; }
            set { game.gameState = value; }
        }
        /// <summary>
        /// Accessor for defense bias.
        /// </summary>
        private double DefenseBias
        {
            get { return game.defenderbias; }
            set { game.defenderbias = value; }
        }
        /// <summary>
        /// Accessor for maximum turn time.
        /// </summary>
        private int Time
        {
            get { return game.time; }
            set { game.time = value; }
        }
        /// <summary>
        /// Random number generator.
        /// </summary>
        private static Random rng = new Random();
        /// <summary>
        /// Accessor for number of turns players have taken.
        /// </summary>
        private int Turn
        {
            get { return game.turn; }
            set { game.turn = value; }
        }
        /// <summary>
        /// LIst of territories scanned by the possiblemoves method.
        /// Prevents code loops.
        /// </summary>
        private static List<Territory> scanterritories = new List<Territory>();
        /// <summary>
        /// Accessor for game's map.
        /// </summary>
        private GameMap Map
        {
            get { return game.map; }
            set { game.map = value; }
        }
        /// <summary>
        /// Accessor for game's game-mode.
        /// </summary>
        private GameMode Gamemode
        {
            get { return game.gamemode; }
            set { game.gamemode = value; }
        }
        /// <summary>
        /// Stores the user's music preference
        /// </summary>
        private bool music_enabled;
        /// <summary>
        /// Accessor for the music enabled variable.
        /// </summary>
        public bool Music_enabled
        {
            get => music_enabled;
            set
            {
                if (Players.Count != 0) // if players are logged in
                {
                    try
                    {
                        ((Human)Players[0]).music_enabled = value; // save the user's preferences to the file
                        Human.Update(Players[0] as Human);
                    }
                    catch {  } // Output an error if the preference did not save
                }
                if (value == true) { mediaplayer.Play(); } // If music is enabled, start playback.
                else if (value == false) { mediaplayer.Pause(); } // If music is disabled, stop playback.
                music_enabled = value; // Set the value
            }
        }
        /// <summary>
        /// Stores the user's hints preference.
        /// </summary>
        private bool hints_enabled;
        /// <summary>
        /// Accessor for the hints enabled variable.
        /// </summary>
        public bool Hints_enabled
        {
            get => hints_enabled;
            set
            {
                if (Players.Count != 0) // If players are logged in
                {
                    try
                    {
                        ((Human)Players[0]).hints_enabled = value; // Save the preference
                        Human.Update(Players[0] as Human);
                    }
                    catch {  }
                }
                hints_enabled = value; // Set the value
            }
        }
        /// <summary>
        /// Thread that runs the timer.
        /// </summary>
        private BackgroundWorker workerthread = null;
        /// <summary>
        /// Stores whether game timer is paused.
        /// </summary>
        private bool paused;
        #endregion

        #region Constructors
        /// <summary>
        /// COnstructor used when loading a game from file.
        /// </summary>
        /// <param name="_game">Loaded game from file</param>
        public GameWindow(GameManager _game)
        {
            game = _game; // Set game object
            SetupWindow(); // Setup the window
            game.territories.Sort(); // Sort the list of territories.
            if (Time > 0) // If timer enabled
            {
                TimerSetup(); // Setup the timer
                StartTimer(); // Start the timer
            }
            LoadPlayerUI(); // Setup UI + Map
            Output("The game has loaded."); // Alert user
        }
        /// <summary>
        /// Constructor used when creating a new game.
        /// </summary>
        /// <param name="_players">List of players playing game.</param>
        /// <param name="randomise_initial">Is initial setup of territories randomised?</param>
        /// <param name="_map">Desired Game Map</param>
        /// <param name="mode">Desired Game-Mode</param>
        /// <param name="timerduration">Length of time allowed for a turn, 0 if disabled.</param>
        /// <param name="defensebias">New Risk bias towards defending players.</param>
        public GameWindow(List<Player> _players, bool randomise_initial, GameMap _map, GameMode mode, int timerduration, double defensebias)
        {
            GameManager.ClearEmptyFile(); // Clear empty save files.
            game = new GameManager(); // Create new game object.
            // Set properties
            Time = timerduration * 100;
            Players = _players;
            Gamemode = mode;
            DefenseBias = defensebias;
            Map = _map;
            SetupWindow(); // Setup the window
            Turn = 0;
            try
            {
                MapSetup(false); // Setup map with new territory and continent objects. If not default map, setup UI.
            }
            catch (Exception) { // Exit if error occurs loading map.
                MessageBox.Show("An error occurred loading the map.");
                this.Close();
                return;
            }
            if (Time > 0)
            {
                TimerSetup(); // If timer enabled setup the timer.
            }
            SetupGame(randomise_initial); // Setup the initial game board.
        }
        #endregion

        #region Setup Methods
        /// <summary>
        /// Contains code shared between constructors.
        /// Sets up UI, events, Data-binding and properties.
        /// </summary>
        private void SetupWindow()
        {
            InitializeComponent();
            paused = false;
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            DataContext = this;
            chkFullscreen.IsChecked = true;
            music_enabled = ((Human)Players[0]).music_enabled;
            mediaplayer.Source = Music.sources[Music.MusicIndex];
            if (music_enabled) { mediaplayer.Play(); }
            if (Gamemode == GameMode.Classic)
            {
                playerdie1 = new PlayerDice(imgPlayerDie1);
                playerdie2 = new PlayerDice(imgPlayerDie2);
                playerdie3 = new PlayerDice(imgPlayerDie3);
                enemydie1 = new EnemyDice(imgEnemyDie1);
                enemydie2 = new EnemyDice(imgEnemyDie2);
            }
        }
        /// <summary>
        /// Sets up map UI, territories and continents.
        /// </summary>
        /// <param name="load">Is the game loaded from a file or a new game?</param>
        private void MapSetup(bool load)
        {
            List<Button> mapbuttons = new List<Button>();
            if (Map == GameMap.Default && !load) // If the map is default and the game is a new game
            {
                // Setup each territory.
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
                // Setup each continent
                Continent North_America = new Continent("North America", (new List<Territory> { Alaska, Alberta, Central_America, Eastern_US, Greenland, Northwest_Canada, Ontario, Quebec, Western_US }), 5);
                Continent South_America = new Continent("South America", (new List<Territory> { Argentina, Brazil, Peru, Venezuela }), 2);
                Continent Europe = new Continent("Europe", (new List<Territory> { UK_Ireland, Iceland, Northern_Europe, Scandinavia, Southern_Europe, Soviet_Bloc, Western_Europe }), 5);
                Continent Africa = new Continent("Africa", (new List<Territory> { Central_Africa, East_Africa, Egypt, Madagascar, North_Africa, South_Africa }), 3);
                Continent Asia = new Continent("Asia", (new List<Territory> { Afghanistan, China, India, Irkutsk, Japan, Kamchatka, Middle_East, Mongolia, Southeast_Asia, Siberia, Ural, Yakutsk }), 7);
                Continent Australia = new Continent("Australia", (new List<Territory> { Eastern_Australia, Indonesia, New_Guinea, Western_Australia }), 2);
                Continents = new List<Continent> { North_America, South_America, Europe, Africa, Asia, Australia };
            }
            else if(Map != GameMap.Default) // If map is not default
            {
                GameGrid.Children.Clear(); // Clear gamegrid UI elements.
                if (Map == GameMap.NewYork)
                {
                    img_Map.ImageSource = new BitmapImage(new Uri("pack://siteoforigin:,,,/Images/Maps/NewYork.jpg")); // Set map image
                    img_Map.Stretch = Stretch.Uniform;
                    // Setup UI buttons
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
                    if (!load) // If new game, setup territories
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
            if (load) // If loading from file, reconnect each territory to its button.
            {
                string buttonprefix = "btn";
                if(Map == GameMap.Default)
                {
                    foreach (Territory t in Territories)
                    {
                        String concat = buttonprefix + t.name;
                        t.button = (Button)GameGrid.FindName(concat); // find the button whose name matches the name of the territory on-screen.
                    }
                }
                else
                {
                    foreach(Territory t in Territories)
                    {
                        String concat = buttonprefix + t.name;
                        t.button = mapbuttons.Find(Button => Button.Name == concat); // set the button to the button in the list whose name matches the name of the territory.
                    }
                }
            }
        }
        /// <summary>
        /// Instantiates each territory UI button with the desired properties.
        /// </summary>
        /// <param name="name">Name of button</param>
        /// <param name="margin">Margin of territory from edge of game grid.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Sets up the game turn timer thread.
        /// </summary>
        private void TimerSetup()
        {
            workerthread = new BackgroundWorker // instantiate thread
            {
                WorkerReportsProgress = true, // report progress to UI
                WorkerSupportsCancellation = true // timer can be cancelled.
            };
            // setup events
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            workerthread.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }
        /// <summary>
        /// Setup the game when creating a new game.
        /// </summary>
        /// <param name="randomise_initial">Are armies going to be placed randomly around the map?</param>
        private void SetupGame(bool randomise_initial)
        {
            // Counts the number of players that are not NeutralAI
            int playercount = 0;
            foreach (Player p in Players)
            {
                if (!(p is NeutralAI))
                {
                    playercount += 1;
                }
            }
            int initialarmies = (50 - (5 * playercount)); // Calculate number of initial armies
            CurrentPlayer = Players[0]; // Set the current player
            // Setup Board and initial armies //
            UISetup(); // Setup UI
            foreach (Player p in Players) { p.army_undeployed = initialarmies; } // Assign each player their initial armies
            UpdateState(GameState.InitialArmyPlace); // Set game state and update UI
            if (randomise_initial == true) // If random placement enabled, randomise initial placement
            {
                SetupRandom();
                Territories.Sort();
                StartGame();
            }
            else
            {
                Territories.Sort();
                if (CurrentPlayer is Human)
                {
                    if (((Human)CurrentPlayer).hints_enabled) // If hints enabled give player hints in text output.
                    {
                        Output("Place armies around the map using left click.");
                        Output("You can capture any territory not already taken by another player.");
                    }
                }
            }
        }
        /// <summary>
        /// Starts the game.
        /// Alerts user and sets gamestate.
        /// </summary>
        private void StartGame()
        {
            Output("The Game is beginning.");
            Gamestate = GameState.PlacingArmy;
            NextTurnThreaded(); // Start the next turn
        }
        /// <summary>
        /// Distributes armies at random around the game board, ensuring each territory is owned by a player and that the distribution is pseudo-random but fair.
        /// </summary>
        private void SetupRandom()
        {
            // originally tried various methods of randomising player and territory assignments to varying degrees of success.
            // However many would get stuck in loops, or to avoid loops would become overly complex.
            // I realised I could instead randomise the order of territories in the list and then assign each territory to a player methodically.
            // This achieves the same effect with far less complexity.
            Territories.Shuffle(); // Shuffle to randomise order in which territories are assigned to players.
            // Assigned initial placements of armies.
            foreach (Territory t in Territories)
            {
                // ensures each territory is owned by a player.
                bool assigned = false;
                do
                {
                    CyclePlayers(); // select the next player
                    if (CurrentPlayer.army_undeployed > 0) // if they have armies left to deploy
                    {
                        Place_Reinforce(t, 1); // conquer this territory
                        assigned = true;
                    }
                } while (assigned == false); // repeat until territory owned by a player.
            }
            // Places remaining armies around map in friendly territory until there are none left //
            foreach (Player p in Players)
            {
                CurrentPlayer = p;
                while (p.army_undeployed > 0) // while the player still has armies left to deploy
                {
                    foreach (Territory t in Territories) // cycle through each territory
                    {
                        if (p.army_undeployed > 0) // while the player still has armies left to deploy
                        {
                            if (t.owner == p) // if the player owns the territory
                            {
                                Place_Reinforce(t, rng.Next(1, Math.Min(p.army_undeployed, 4))); // place a random number of armies on the territory
                                // between the number the player has left to deploy and 4.
                            }
                        }
                        else { break; } // exit if no armies left to deploy
                    }
                }
            }
        }
        /// <summary>
        /// Distributes the neutral AI armies among unowned territories.
        /// </summary>
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
                    CycleNeutralAI(); // cycle to the next neutral AI
                    if (CurrentPlayer.army_undeployed > 0) // if they have armies left to deploy
                    {
                        alltaken = false;
                        Place_Reinforce(t, 1); // conquer the territory.
                        assigned = true;
                    }
                } while (assigned == false); // repeat until territory owned.
            }
            if (alltaken == false) // if all territories are not owned by players; some territories are owned by neutral AI
            {
                foreach (Player p in Players) // cycle each neutral AI
                {
                    if (p is NeutralAI)
                    {
                        CurrentPlayer = p;
                        while (p.army_undeployed > 0) // while neutral AI has armies left to deploy
                        {
                            foreach (Territory t in Territories)
                            {
                                if (p.army_undeployed > 0) // while neutral AI has armies left to deploy
                                {
                                    if (t.owner == p) // if AI owns territory
                                    {
                                        Place_Reinforce(t, rng.Next(1, Math.Min(p.army_undeployed, 4))); // place a random number of armies on the territory
                                        // between the number the player has left to deploy and 4.
                                    }
                                }
                                else { break; } // else exit
                            }
                        }
                    }
                }
            }
            // Places remaining armies around map in friendly territory until there are none left //
        }
        /// <summary>
        /// Cycles the current player through each neutral AI in the game.
        /// </summary>
        private void CycleNeutralAI()
        {
            CyclePlayers();
            if (!(CurrentPlayer is NeutralAI))
            {
                CycleNeutralAI();
            }
        }
        #endregion
        #region Player-Panel UI Setup
        /// <summary>
        /// Sets up the player panel UI for each player
        /// </summary>
        private void UISetup()
        {
            // foreach player set their username display,
            // bind their army strength and number of territories owned to the onscreen labels
            // set their player colour indicator
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
            ///<remarks> Make this more efficient in future </remarks>
            if (Players.Count >= 4)
            {
                lblPlayerName4.Content = Players[3].Username;
                rectPlayerColor4.Fill = (SolidColorBrush)Players[3].Color;
                brd_Player4.Visibility = Visibility.Visible;
                Players[3].Disp_ArmyStrength = lblPlayer4Strength;
                Players[3].Disp_Owned = lblPlayer4Territories;
                if (Players.Count >= 5)
                { // If there are more than 5 players UI elements must be resized to fit all players on-screen.
                    int fs;
                    int rect_height = 17; // player colour indicator height
                    Thickness th = new Thickness(10, 3, 0, 3); // margin around player colour indicators
                    int gap = 2; // margin around player panel entries
                    lblPlayerName5.Content = Players[4].Username;
                    rectPlayerColor5.Fill = (SolidColorBrush)Players[4].Color;
                    brd_Player5.Visibility = Visibility.Visible;
                    Players[4].Disp_ArmyStrength = lblPlayer5Strength;
                    Players[4].Disp_Owned = lblPlayer5Territories;
                    if (Players.Count >= 6)
                    {
                        fs = 11; // reduce fontsize to 11
                        gap = 0; // reduce margin around player panel entries to 0
                        rectPlayerColor6.Fill = (SolidColorBrush)Players[5].Color;
                        lblPlayerName6.Content = Players[5].Username;
                        brd_Player6.Visibility = Visibility.Visible;
                        Players[5].Disp_ArmyStrength = lblPlayer6Strength;
                        Players[5].Disp_Owned = lblPlayer6Territories;
                        SetFontSize(fs); // set the new font-size
                    }
                    SetGap(gap); // set the margin around player panel entries.
                    SetRect(rect_height, th); // set the dimensions and margin around the player colour indicators.
                }
            }
        }
        /// <summary>
        /// Sets the font-size of labels within the player panel
        /// </summary>
        /// <param name="i">Desired font-size</param>
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
        /// <summary>
        /// Sets the players' colour indicators dimensions and margin
        /// </summary>
        /// <param name="i">Desired height</param>
        /// <param name="margin">Desired margin</param>
        private void SetRect(int i, Thickness margin)
        {
            // set height
            rectPlayerColor1.Height = i;
            rectPlayerColor2.Height = i;
            rectPlayerColor3.Height = i;
            rectPlayerColor4.Height = i;
            rectPlayerColor5.Height = i;
            rectPlayerColor6.Height = i;
            // set margin
            rectPlayerColor1.Margin = margin;
            rectPlayerColor2.Margin = margin;
            rectPlayerColor3.Margin = margin;
            rectPlayerColor4.Margin = margin;
            rectPlayerColor5.Margin = margin;
            rectPlayerColor6.Margin = margin;
        }
        /// <summary>
        /// Sets the margin around player panel entries.
        /// </summary>
        /// <param name="i">Desired margin</param>
        private void SetGap(int i)
        {
            Thickness gap;
            if (i == 0) // if margin is 0
            {
                gap = new Thickness(0, 0, 0, 0);
                brd_Player1.Margin = gap; // set the first player entry's margin to 0
                brd_Players.BorderThickness = new Thickness(0);
            }
            else
            {
                gap = new Thickness(5, 0, 5, i);
                brd_Player1.Margin = new Thickness(5, i, 5, i); // set the first player entry's margin to the same as others but with an additional top margin
            }
            // set margins
            brd_Player2.Margin = gap;
            brd_Player3.Margin = gap;
            brd_Player4.Margin = gap;
            brd_Player5.Margin = gap;
            brd_Player6.Margin = gap;
        }
        #endregion

        #region Game Methods
        /// <summary>
        /// Sets current player to next player in list and updates UI
        /// </summary>
        private void CyclePlayers()
        { // Cycles through the list of players, for a new turn or placing armies.
            if ((Players.IndexOf(CurrentPlayer) + 1) == (Players.Count))
            { CurrentPlayer = Players[0];
                if (Gamestate != GameState.InitialArmyPlace) { Turn += 1; }
            } // if at end of list loop back to start.
            else { CurrentPlayer = Players[(Players.IndexOf(CurrentPlayer) + 1)]; } // else set to next player in list.
            UpdatePlayerPanelUI(); // update UI
        }
        /// <summary>
        /// returns a boolean indicating if every player has placed their initial armies
        /// </summary>
        /// <returns>Have all players (except NeutralAI) placed their initial armies</returns>
        private bool AllPlaced()
        {
            bool allplaced = true;
            foreach(Player p in Players)
            {
                if(p.army_undeployed > 0 && !(p is NeutralAI)) // if player has undeployed armies and is not a neutral AI
                {
                    allplaced = false;
                    break;
                }
            }
            return allplaced;
        }
        /// <summary>
        /// Cancels timer before continuing to next turn
        /// </summary>
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
        /// <summary>
        /// Starts the next player's turn in the game.
        /// </summary>
        private void NextTurnThreaded()
        {
            ClearSelectionsUI(); // Clear previous player's selection(s)
            if (Gamestate == GameState.InitialArmyPlace) // During setup
            {
                if ((!(CurrentPlayer is NeutralAI)) && CurrentPlayer.army_undeployed > 0) // If next player has armies to deploy and is not a neutral AI
                {
                    Output(String.Format("It is now {0}'s turn.", CurrentPlayer.Username)); // Alert the player their turn is starting
                }
                else
                {
                    if (CurrentPlayer is NeutralAI) { CyclePlayers(); NextTurn(); } // If the player is a neutral AI skip their turn
                    if (AllPlaced()) // If all human players have placed their initial armies
                    {
                        foreach(Player p in Players) // Setup the Neutral AI placement
                        {
                            if(p is NeutralAI)
                            {
                                NeutralAISetup();
                                break;
                            }
                        }
                        Territories.Sort(); // After shuffling during setup, must be sorted for binary search to work.
                        CurrentPlayer = Players[Players.Count - 1]; // Set current player to end of list
                        StartGame();
                    }
                }
            }
            else // If game has started
            {
                CyclePlayers(); // Cycle to next player
                if(CurrentPlayer is NeutralAI) // If neutral AI skip turn
                {
                    NextTurn();
                }
                else
                {
                    List<String> ownedContinents = new List<string>();
                    int bonus = 0;
                    foreach (Continent c in Continents) // If continent owned by player give them bonus armies
                    {
                        if (ContinentOwned(c))
                        {
                            ownedContinents.Add(c.name);
                            bonus += c.bonus;
                        }
                    }
                    CurrentPlayer.army_undeployed += ((CurrentPlayer.Territoriesowned / 3) + bonus); // Give player reinforcements
                    UpdateState(GameState.PlacingArmy);
                    /// <remarks>simplify this code?</remarks>
                    switch (ownedContinents.Count) // alert user that they have received reinforcements
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
                    if (Time > 0) { StartTimer(); } // If timer enabled start the timer
                }
            }
        }
        /// <summary>
        ///Called when a player wins the game.
        /// </summary>
        ///Opens a new high-scores window and closes this window.
        private void Win()
        {
            CurrentPlayer.score += CurrentPlayer.Army_strength / 3; // Calculate player's final score
            int finalscore = CurrentPlayer.score / Turn;
            GameDetails gamedetails = new GameDetails(DateTime.Now.ToString(), CurrentPlayer.Username, Players.Count.ToString(), finalscore.ToString(),Turn.ToString(), Map.ToString(), Gamemode.ToString());
            GameDetails.Save(gamedetails); // Save their score details to file.
            GameManager.DeleteGame(game.GameID); // Delete their game save
            Highscores finish = new Highscores(gamedetails, Players); // Open new highscores window
            App.Current.MainWindow = finish;
            this.Close(); // Close this window
            finish.Show();
        }
        #endregion

        #region UI Update Methods
        /// <summary>
        /// Instantiates UI when game is loaded.
        /// </summary>
        private void LoadPlayerUI()
        {
            /// <remarks>use data binding in future now that it is working correctly</remarks>
            UISetup(); // Setup UI
            UpdatePlayerPanelUI(); // set player panel to indicate current player
            try
            {
                MapSetup(true); // Setup map in load game mode
            }
            catch (Exception) // exit if failure
            {
                MessageBox.Show("An error occurred loading the map.");
                this.Close();
                return;
            }
            UpdateState(game.gameState); // Update state
            if (Gamestate == GameState.PlacingArmy) // Output number of armies player has to place
            {
                UpdatePlayerUndeployed();
            }
            foreach (Territory t in Territories) // Update territory UI
            {
                t.button.Background = t.owner.Color;
                t.button.Content = t.currentarmies;
            }
        }
        /// <summary>
        /// Updates nextTerritory UI when conquering
        /// </summary>
        private void ConquerTerritoryUI()
        {
            NextTerritory.button.Background = NextTerritory.owner.Color; // set colour
            NextTerritory.button.Content = NextTerritory.temparmies; // set content
        }
        /// <summary>
        /// Updates selected territories' armies
        /// </summary>
        private void AttackMoveTerritoryUI()
        {
            SlctTerritory.button.Content = SlctTerritory.currentarmies; // set content
        }
        /// <summary>
        /// Updates number output to the relevant value
        /// </summary>
        private void UpdateNumOutput()
        {
            switch (Gamestate)
            {
                case GameState.PlacingArmy: // During placing it shows how many armies you are placing on the selected territory.
                    btnNumber.Content = SlctTerritory.temparmies;
                    break;
                case GameState.Attacking: // During attack it shows how many armies you are attacking with.
                case GameState.Conquer: // During conquer it shows how many armies you are moving into the new territory.
                case GameState.Move: // During move it shows how many armies you are moving to the next territory.
                    btnNumber.Content = NextTerritory.temparmies;
                    break;
            }
        }
        /// <summary>
        /// Updates currently highlighted player in player panel to current player
        /// </summary>
        private void UpdatePlayerPanelUI()
        {
            int i = Players.IndexOf(CurrentPlayer); // i = index of player in list
            foreach (Border b in panel_Players.Children) { // reset every other player's background
                b.Background = panel_Players.Background;
            }
            panel_UI.Background = CurrentPlayer.Color; // update UI background to current player's colour.
            switch (i) // Highlight player matching index.
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
        }
        /// <summary>
        /// Outputs number of armies player has not yet deployed.
        /// </summary>
        private void UpdatePlayerUndeployed()
        {
            Output(String.Format("You have {0} armies to place.", CurrentPlayer.army_undeployed));
        }
        /// <summary>
        /// Updates the UI to reflect the current game state.
        /// Outputs hints for player if they have enabled hints.
        /// </summary>
        private void UpdateStateUI()
        {
            switch (Gamestate)
            {
                /// <remarks>
                /// In each state, the state display button's content and colour is changed.
                /// The confirmation button's (on the number display) content is also changed.
                /// If the current player has hints enabled a contextual text hint is output.
                /// </remarks>
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
        }
        /// <summary>
        /// Clears all player territory selections and clears UI
        /// </summary>
        private void ClearSelectionsUI()
        {
            foreach(Territory t in Territories)
            {
                t.button.BorderBrush = Brushes.Black; // reset border
            }
            btnNumber.Content = 0; // reset indicator
            ClearSelections(); // clear player's selections
        }
        #endregion

        #region Back-end Methods
        /// <summary>
        /// Retrieves the territory from the list matching the territory name.
        /// </summary>
        /// <param name="territoryname">Name of desired territory</param>
        /// <returns Territory found from list></returns>
        private Territory RetrieveTerritory(String territoryname)
        {
            territoryname = territoryname.Replace(' ', '_'); // ensures c# string parameter does not cause error (can sometimes remove spaces)
            try
            {
                // Binary Search //
                int start = 0;
                int end = Territories.Count - 1;
                while (start <= end) // repeat until found
                {
                    int mid = Decimal.ToInt32(Math.Floor((decimal)(start + end) / 2)); // find midpoint
                    if (String.Compare(territoryname, Territories[mid].name) < 0) // compare order alphabetically
                    {
                        end = mid - 1; // move endpoint
                    }
                    else if(String.Compare(territoryname, Territories[mid].name) > 0) // compare order alphabetically
                    {
                        start = mid + 1; // move start-point
                    }
                    else
                    {
                        return Territories[mid]; // return midpoint
                    }
                }
                throw new TerritoryNotFoundException(); // if not found (old bugs in binary search meant that it sometimes failed)
            }
            catch (TerritoryNotFoundException) // if not found
            {
                foreach(Territory t in Territories) // perform linear search
                {
                    if(t.name == territoryname)
                    {
                        return t;
                    }
                }
                throw new Exception();
            }
        }
        /// <summary>
        /// Selects and highlights the given territory in the UI
        /// </summary>
        /// <param name="t">The desired territory</param>
        /// <param name="b">The button of the territory</param>
        /// <param name="color">colour button should be highlighted with</param>
        /// <param name="next">select next territory?</param>
        /// <remarks>button parameter can be removed as it is redundant, t.button can be used instead</remarks>
        private void SelectTerritory(Territory t, Button b, Brush color, bool next)
        {
            if (next) { NextTerritory = t; } // if next territory set next
            else { SlctTerritory = t; } // otherwise set primary selection
            b.BorderBrush = color;
        }
        /// <summary>
        /// Provide text output to user
        /// </summary>
        /// <param name="s">Output message</param>
        private void Output(String s)
        {
            if (txtOutput.Text == null || txtOutput.Text == "") { txtOutput.Text = s; } // if first line added, text = string.
            else { txtOutput.Text += ("\n" + s); } // every line after has a new line character appended.
            scrlOutput.ScrollToEnd(); // scrolls to end of text-output.
        }
        /// <summary>
        /// Clears the selected Territories.
        /// </summary>
        private void ClearSelections()
        {
            SlctTerritory = null;
            NextTerritory = null;
        }
        /// <summary>
        /// Updates UI and game state to parameter.
        /// </summary>
        /// <param name="g">Next game-state</param>
        private void UpdateState(GameState g)
        {
            Gamestate = g; // set gamestate
            if(Gamestate == GameState.Conquer) { ConquerTerritoryUI(); } // update territory UI during conquer
            UpdateStateUI(); // update main UI panel
        }
        /// <summary>
        /// Highlights all territories player could possibly attack from the selected territory.
        /// </summary>
        /// <remarks> Merge with move for efficiency at some point</remarks>
        private void ShowAttack()
        {
            bool canmove = false; // player cannot attack unless a possible move is found
            foreach(String s in SlctTerritory.links) // iterate through each link
            {
                Territory t = RetrieveTerritory(s); // retrieve the object matching the link name
                if(t.owner != CurrentPlayer) // If the owner is not the current player
                {
                    canmove = true; // player can move
                    t.button.BorderBrush = Brushes.Aqua; // highlight territory
                }
            }
            if(canmove == false) // if player cannot attack,
            {
                Output("There is nowhere to attack from here."); // alert player
            }
        }
        /// <summary>
        /// Performs branching search to highlight all territories player
        /// could move to from territory.
        /// </summary>
        /// <param name="t">Territory to scan links</param>
        /// <returns>can player move from supplied territory</returns>
        /// <remarks>this is very much a bodge. please fix with a proper solution later.
        /// fix the twice add to list too</remarks>
        private bool ShowMoves(Territory t)
        { // merge with show attack for efficiency
            //
            scanterritories.Add(t); // this line is inefficient as added twice
            bool canmove = false; // player cannot move unless move is found
            foreach(String s in t.links) // iterate through each link
            {
                Territory y = RetrieveTerritory(s);
                if (!scanterritories.Contains(y)) // if territory already scanned, skip that path
                {
                    scanterritories.Add(y); // add to list to prevent loops
                    if (y.owner == CurrentPlayer) // if owned by player,
                    {
                        canmove = true; // player can move
                        y.button.BorderBrush = Brushes.Aqua; // highlight territory
                        ShowMoves(y); // scan the path leading from that territory.
                    }
                }
            }
            return canmove;
        }
        /// <summary>
        /// Increments or decrements the number of armies the player is using to attack or move.
        /// </summary>
        /// <param name="i">number to increment or decrement by</param>
        private void AdjustAttackMoves(int i)
        {
            if(i >= 1) // moving armies out of slct territory
            {
                if (SlctTerritory.currentarmies < 2) // ensure territory always has army
                {
                    Output("At least one army must remain in a friendly territory.");
                    return;
                }
                else if(Gamemode == GameMode.Classic && Gamestate == GameState.Attacking && NextTerritory.temparmies >= 3)
                {
                    Output("You can attack with a maximum of 3 armies at once.");
                    return; // players can only roll 3 die during attack in classic risk
                }
            }
            if(i <= -1) // moving armies back to slct territory
            {
                if(NextTerritory.temparmies <= 1) // player cannot perform an action with less than one
                {
                    switch (Gamestate)
                    {
                        // alert user
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
                else if(Gamemode == GameMode.Classic && Gamestate == GameState.Conquer) // during conquer in classic risk
                {
                    int count = 0;
                    foreach(Dice d in dices) { if (d is PlayerDice) { count += 1; } } // count number of dice player rolled.
                    if(NextTerritory.temparmies <= count) // player must move at least as many as used to attack, even if player loses an army they cannot decrease
                    {
                        Output("You must move at least as many armies used to attack into the new territory.");
                        return;
                    }
                }
            }
            SlctTerritory.currentarmies -= i; // adjust numbers
            NextTerritory.temparmies += i;
            UpdateNumOutput(); // update the number output
            AttackMoveTerritoryUI(); // update territory UI
            if(Gamestate == GameState.Conquer) { ConquerTerritoryUI(); } // update next territory UI
        }
        /// <summary>
        /// Cancels player actions that have not been confirmed
        /// </summary>
        private void CancelUnconfirmedActions()
        {
            switch (Gamestate) // determine game-state
            {
                case GameState.PlacingArmy:
                    foreach (Territory t in Territories)
                    {
                        if (t.owner == CurrentPlayer)
                        {
                            CurrentPlayer.army_undeployed += t.temparmies; // return unconfirmed armies to undeployed
                            t.temparmies = 0;
                        }
                    }
                    ClearSelectionsUI(); // clear selections and UI
                    break;
                case GameState.Attacking:
                    if(SlctTerritory != null) // if player has selected a territory
                    {
                        if(NextTerritory != null) // if player has selected both territories
                        {
                            SlctTerritory.currentarmies += NextTerritory.temparmies; // move armies "home"
                            NextTerritory.temparmies = 0;
                            AttackMoveTerritoryUI(); // update UI
                            NextTerritory.button.Content = NextTerritory.currentarmies;
                        }
                        ClearSelectionsUI(); // clear selections and UI
                    }
                    break;
                case GameState.Conquer:
                    Output("You must move armies into the newly captured territory."); // cannot cancel confirmed
                    break;
                case GameState.Move:
                    if(SlctTerritory != null)
                    {
                        if(NextTerritory != null)
                        {
                            SlctTerritory.currentarmies += NextTerritory.temparmies; // move armies to original location
                            AttackMoveTerritoryUI(); // update UI
                            NextTerritory.temparmies = 0;
                        }
                        ClearSelectionsUI(); // clear selections and UI
                    }
                    break;
            }
        }
        /// <summary>
        /// Determines if continent is owned by player
        /// </summary>
        /// <param name="c">Continent being tested.</param>
        /// <returns>Player owns continent?</returns>
        private bool ContinentOwned(Continent c)
        {
            bool owned = true; // player owns continent unless someone else owns a territory within
            foreach(Territory t in c.territories) // search each territory
            {
                if(t.owner != CurrentPlayer) { owned = false; break; } // if another player owns a territory within the player does not own the continent
            }
            return owned;
        }
        /// <summary>
        /// Determines if die menu is open and outputs error text
        /// </summary>
        /// <returns>Is die menu open?</returns>
        private bool DieOpen()
        {
            if(panel_Die.Visibility == Visibility.Visible) // if open
            {
                if((String)btnDieStatus.Content == "Continue to Attack")
                {
                    Output("Click \"continue to attack\" to proceed."); // output error
                }
                else if((String)btnDieStatus.Content == "Continue to Conquer")
                {
                    Output("You must conquer the territory."); // output error
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Timer Control Methods and Events
        /// <summary>
        /// Starts turn timer
        /// </summary>
        private void StartTimer()
        {
            pb_Timer.Value = 0; // reset value
            workerthread.CancelAsync(); // cancel previous timer
            workerthread.RunWorkerAsync(); // start timer
        }
        /// <summary>
        /// Timer start event. Runs timer and sends updates to progress bar.
        /// </summary>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for(int i = 0; i < Time; i++) // while timer has time left
            {
                if (workerthread.CancellationPending == true) // if cancellation pending,
                {
                    e.Cancel = true; // cancel timer
                    return;
                }
                else
                {
                    while(paused == true) { Thread.Sleep(100); } // while game paused sleep timer thread.
                    int progressPercentage = Convert.ToInt32(((double)i / Time) * 100); // calculate timer complete percentage
                    (sender as BackgroundWorker).ReportProgress(progressPercentage); // send progress report to UI
                    Thread.Sleep(10); // wait before updating again
                }
            }
        }
        /// <summary>
        /// Updates timer progress bar.
        /// </summary>
        /// <param name="sender">Background worker that sent update</param>
        /// <param name="e">Percentage timer complete.</param>
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb_Timer.Value = e.ProgressPercentage; // update progress bar to value
        }
        /// <summary>
        /// Called when timer ends, cancels actions and ends turn if allowed.
        /// </summary>
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Gamestate == GameState.Conquer) { Output("Move your armies to end your turn."); } // turn cannot end during conquer
            else if (!DieOpen()) // if die menu not open
            {
                CancelUnconfirmedActions();
                NextTurnThreaded(); // start next turn
            }
        }
        #endregion

        #region Button Click Events
        /// <summary>
        /// Territory left click event. Performs contextual action.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        private void Click(object sender, RoutedEventArgs e)
        {   // Called when a territory is clicked on and performs an action based on the
            // context in which it was clicked.
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' })); // Retrieve territory based on name of button clicked
            Button btnTerritory = t.button;
            switch (Gamestate)
            {
                case GameState.InitialArmyPlace: // During setup
                    SlctTerritory = t; // select territory
                    if (SlctTerritory.owner == null || SlctTerritory.owner == CurrentPlayer) // if available,
                    {
                        Place_Reinforce(SlctTerritory, 1); // place an army on territory
                        CyclePlayers(); // next player
                        NextTurn(); // start next turn
                    }
                    else { Output("You cannot capture this territory."); ClearSelectionsUI(); } // output error and clear selection
                    break;
                case GameState.PlacingArmy:
                    if(t == SlctTerritory) { PlayerActions(true); break; } // if selected attempt to deploy another army
                    else
                    {
                        ClearSelectionsUI(); // clear selections
                        if (t.owner == null || t.owner == CurrentPlayer) // if available to place,
                        {
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false); // select clicked territory
                            PlayerActions(true); // add one army
                            UpdateNumOutput(); // update number output
                        }
                        else { Output("This is not your territory."); ClearSelectionsUI(); } // output error and clear selections
                        break;
                    }
                case GameState.Attacking:
                    if (DieOpen()) { return; } // if die open player cannot adjust
                    if (t.owner == CurrentPlayer) // if own territory clicked
                    {
                        if(t.currentarmies > 1) // if enough armies to attack
                        {
                            CancelUnconfirmedActions(); // cancel previous attacks
                            SelectTerritory(t, btnTerritory, Brushes.Lime, false);
                            ShowAttack();
                        }
                        else { Output("You do not have enough armies to attack from here."); break; } // output error
                    }
                    else if (SlctTerritory != null) // if a territory is selected
                    {
                        if(NextTerritory == null) // if the next territory is not selected.
                        {
                            if (t.owner != null) // if territory owned (but not by player, checked in previous line)
                            {
                                if (btnTerritory.BorderBrush == Brushes.Aqua) // if marked as available to attack
                                {
                                    SelectTerritory(t, btnTerritory, Brushes.Red, true); // select the territory
                                    // set default attack number
                                    if (Gamemode == GameMode.NewRisk) { AdjustAttackMoves((SlctTerritory.currentarmies - 1)); }
                                    else if (Gamemode == GameMode.Classic) { AdjustAttackMoves(Math.Min(SlctTerritory.currentarmies - 1, 3)); }
                                    if (CurrentPlayer is Human) // output hint if the player has hints enabled
                                    {
                                        if (((Human)CurrentPlayer).hints_enabled)
                                        {
                                            Output("Select the number of armies you wish to attack with.");
                                        }
                                    }
                                }
                                else { Output("You cannot attack this territory from here"); break; } // error
                            }
                            else { Output("There is nothing here to attack."); break; } // error output
                        }
                        else if(NextTerritory == t && btnTerritory.BorderBrush == Brushes.Red) // if next territory clicked
                        {
                            PlayerActions(true); // adjust number attacking up
                        }
                        else { Output("You must cancel your previous selection before attacking a different territory."); break; }
                    }
                    else { Output("You do not own this territory");
                        Output("Select where you wish to attack from"); }
                    break;
                case GameState.Move:
                    if(t.owner != CurrentPlayer) { Output("You do not own this territory."); break; } // can only select territories owned by player
                    else if(SlctTerritory == null) // if territory not selected
                    {
                        ClearSelectionsUI(); // ckear selections + UI
                        SelectTerritory(t, btnTerritory, Brushes.Lime, false); // select the clicked territory
                        if (ShowMoves(SlctTerritory)) // show possible moves, if there are moves
                        {
                            if (CurrentPlayer is Human) // give player hint if enabled
                            {
                                if (((Human)CurrentPlayer).hints_enabled)
                                {
                                    Output("You can move armies to the highlighted territories.");
                                }
                            }
                        }
                        else { Output("There are no friendly territories to move to from here."); ClearSelectionsUI(); } // if no moves possible error output + clear selections
                        List<Territory> blank = new List<Territory>();
                        scanterritories = blank; // clears scanned territories
                    }
                    else if(btnTerritory.BorderBrush == Brushes.Green) // if next territory selected and clicked
                    {
                        PlayerActions(true); // increase number moved
                        return;
                    }
                    else if (btnTerritory.BorderBrush == Brushes.Aqua) // if possible move clicked
                    {
                        if(NextTerritory != null) // if next territory already selected
                        {
                            // error
                            Output("You must finish or cancel your current move before selecting another territory.");
                            return;
                        }
                        SelectTerritory(t, btnTerritory, Brushes.Green, true); // if next territory not selected select this territory
                        AdjustAttackMoves(1); // move one army into selected territory.
                    }
                    else { Output("You cannot move armies to here from your selected territory."); } // if not movable, error
                    break;
                case GameState.Conquer:
                    try
                    {
                        if (t == NextTerritory) { PlayerActions(true); } // increase number moved into new territory.
                        else if (t == SlctTerritory) { PlayerActions(false); } // decrease number moved into new territory.
                        break;
                    }
                    catch (NullReferenceException) { break; }
            }
        }
        /// <summary>
        /// Territory right click event. Performs contextual action.
        /// </summary>
        /// <param name="sender">Button that was clicked</param>
        private void RightClick(object sender, MouseEventArgs e)
        {
            Territory t = RetrieveTerritory(((Button)sender).Name.TrimStart(new char[] { 'b', 't', 'n' })); // retrieve clicked territory
            Button btnTerritory = t.button;
            switch (Gamestate)
            {
                case GameState.PlacingArmy:
                    if (t == SlctTerritory) { PlayerActions(false); } // if selected territory right clicked, decrease number placed
                    // error will be output if temp = 0 from player actions script as is shared by decrease button.
                    break;
                case GameState.Attacking:
                    // error is output by adjust attack moves if invalid
                    try
                    {
                        if (!DieOpen()) // if die menu not open
                        {
                        // if nextterritory null break otherwise continue
                            if (t == NextTerritory) { PlayerActions(false); } // decrease number attacking if attacked territory right clicked
                            else if (t == SlctTerritory) // increase number attacking if slct right clicked
                            {
                                if (Gamemode == GameMode.Classic && (NextTerritory.temparmies == 3)) { Output("You cannot attack with more than 3 armies at a time."); return; }
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
                        if(t == NextTerritory) { PlayerActions(false); } // decrease number moving if right click next territory
                        else if (t == SlctTerritory) { PlayerActions(true); }  // increase number moving if right click sclt territory
                        break;
                    }
                    catch (NullReferenceException) { break; }
                case GameState.Move:
                    if((SlctTerritory != null) && (NextTerritory != null))
                    {
                        if(t == SlctTerritory) { PlayerActions(true); } // increase number moving if right click sclt territory
                        else if(t == NextTerritory) { PlayerActions(false); } // decrease number moving if right click next territory
                    }
                    break;
            }
        }
        /// <summary>
        /// Confirms current player action
        /// </summary>
        private void Confirm(object sender, RoutedEventArgs e)
        { // Confirms the current action(s)
            switch (Gamestate)
            {
                case GameState.PlacingArmy:
                    foreach(Territory t in Territories)
                    {
                        if(t.owner == CurrentPlayer)
                        {
                            Place_Reinforce(t, t.temparmies); // place assigned armies on territory
                            t.temparmies = 0;
                        }
                    }
                    if(CurrentPlayer.army_undeployed == 0) { // if player does not have armies left to deploy proceed to attack phase
                        NextAction(); }
                    else { ClearSelectionsUI(); } // clear selections
                    break;
                case GameState.Attacking:
                    if((SlctTerritory != null) && (NextTerritory != null)) // if both territories selected
                    {
                        if(Gamemode == GameMode.NewRisk) // New Risk mode
                        {
                            double num = rng.NextDouble(); // generate random number
                            // calculate probability of player winning battle
                            double prob = (double)1 / (1 + Math.Exp(-3 * (((double)(NextTerritory.temparmies - NextTerritory.currentarmies)/NextTerritory.currentarmies) - DefenseBias)));
                            if (num <= prob) // player wins
                            {
                                // change values
                                NextTerritory.owner.Territoriesowned -= 1;
                                NextTerritory.owner.Army_strength -= NextTerritory.currentarmies;
                                NextTerritory.currentarmies = 0; // enemy loses all armies
                                NextTerritory.owner = CurrentPlayer;
                                CurrentPlayer.Territoriesowned += 1;
                                CurrentPlayer.score += 1;
                                // check if player has won
                                bool won = true;
                                foreach (Player p in Players)
                                {
                                    if (p != CurrentPlayer && p.Territoriesowned > 0) { won = false; break; } // if any player other than the current player owns a territory the player has not won.
                                }
                                if (won) { Win(); } // if the player has won end the game
                                int lost = (int)Math.Floor((1-prob) * (Double)NextTerritory.temparmies); // calculate the number of casualties the player will suffer
                                NextTerritory.temparmies -= lost;
                                Output(String.Format("You have captured this territory and lost {0} armies in battle.", lost)); // alert user
                                UpdateState(GameState.Conquer); // update to conquer defeated territory
                            }
                            else
                            {
                                NextTerritory.temparmies = 0; // player loses all armies
                                int survived = (int)(Math.Ceiling((1 - prob) * (Double)NextTerritory.currentarmies)); // calculate number of surviving enemy armies
                                int loss = NextTerritory.currentarmies - survived;
                                Output(String.Format("You have lost this battle, the enemy suffered {0} casualties.", loss));
                                NextTerritory.currentarmies = survived; /// <remarks> this should be simplified </remarks>
                                NextTerritory.button.Content = NextTerritory.currentarmies; // update UI
                                ClearSelectionsUI(); // clear selections
                            }
                        }
                        // Classic Mode
                        else if(Gamemode == GameMode.Classic)
                        {
                            if (DieOpen()) { return; } // if die menu open ignore
                            // show die menu
                            panel_NumberSelection.Visibility = Visibility.Collapsed;
                            panel_Die.Visibility = Visibility.Visible;
                            dices.Clear();
                            // add player and enemy dice, showing UI images of dice when created.
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
                            btnPlayerDie.Content = CurrentPlayer.Username; // update UI
                            btnEnemyDie.Content = NextTerritory.owner.Username; // update UI
                            // style setup //
                            // sets background colour of elements to match player/enemy colour
                            Style diestyle = panel_Die.Resources["btnDie"] as Style;
                            Style playerdiestyle = new Style(typeof(Button) ,diestyle);
                            playerdiestyle.Setters.Add(new Setter(Border.BackgroundProperty, CurrentPlayer.Color));
                            panel_Die.Resources["btnPlayerDie"] = playerdiestyle;
                            Style enemydiestyle = new Style(typeof(Button), diestyle);
                            enemydiestyle.Setters.Add(new Setter(Border.BackgroundProperty, NextTerritory.owner.Color));
                            panel_Die.Resources["btnEnemyDie"] = enemydiestyle;
                            ToRoll = dices.Count; // number of dice which need to be rolled.
                            Rolled = 0; // number rolled thus far
                            paused = true; // pause timer
                            foreach (Dice d in dices) // start each dice roll
                            {
                                d.workerthread.RunWorkerCompleted += DieRollComplete;
                                d.StartRoll();
                            }
                        }
                    }
                    else { Output("You must select the territories you wish to attack to/from first."); } // if not selected output error
                    break;
                case GameState.Conquer:
                    Place_Reinforce(NextTerritory, NextTerritory.temparmies); // move armies into new territory
                    NextTerritory.temparmies = 0;
                    NextTerritory.button.Background = NextTerritory.owner.Color; // update UI
                    NextTerritory.button.Content = NextTerritory.currentarmies;
                    if (Time > 0) // proceed to next turn if timer expired
                    {
                        if (workerthread.IsBusy == false) { NextTurnThreaded(); return; }
                    }
                    NextAction(); // proceed to move if timer not expired
                    break;
                case GameState.Move:
                    // output error if both territories not selected
                    if(SlctTerritory == null) { Output("You must select a territory to move armies from."); }
                    else if(NextTerritory == null) { Output("You must select a territory to move armies to."); }
                    else
                    {
                        Place_Reinforce(NextTerritory, NextTerritory.temparmies); // else move the armies
                        NextTerritory.temparmies = 0;
                        NextAction(); // end turn
                    }
                    break;
            }
        }
        /// <summary>
        /// Cancels current action and skips to next game phase if possible.
        /// </summary>
        private void Continue(object sender, RoutedEventArgs e)
        {
            if (DieOpen()) { return; } // if die menu visible cancel
            else if (Gamestate != GameState.Conquer) { CancelUnconfirmedActions(); NextAction(); } // if not conquer, cancel and proceed to next action
            else { Output("You must finish conquering the territory."); } // error as cannot cancel conquer
        }
        /// <summary>
        /// Cancels unconfirmed actions if die panel not open.
        /// </summary>
        private void Cancel(object sender, RoutedEventArgs e)
        {
            if (DieOpen()) { return; }
            CancelUnconfirmedActions();
        }
        /// <summary>
        /// Increases player contextual number (Player Actions method)
        /// </summary>
        private void Increase(object sender, RoutedEventArgs e) { PlayerActions(true); }
        /// <summary>
        /// Decreases player contextual number (Player Actions method)
        /// </summary>
        private void Decrease(object sender, RoutedEventArgs e) { PlayerActions(false); }
        /// <summary>
        /// Pauses game and opens settings menu.
        /// </summary>
        private void Settings(object sender, RoutedEventArgs e)
        {
            paused = true;
            panel_MainUI.Visibility = Visibility.Collapsed;
            panel_Settings.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Un-pauses game and closes settings menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Return(object sender, RoutedEventArgs e)
        {
            paused = false;
            panel_MainUI.Visibility = Visibility.Visible;
            panel_Settings.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// Checks player is not in middle of action that cannot be cancelled,
        /// before cancelling action and saving game.
        /// </summary>
        private void SaveGame(object sender, RoutedEventArgs e)
        { // if cannot cancel, output error
            if (Gamestate == GameState.InitialArmyPlace) { Output("You must finish setup before attempting to save."); }
            else if (Gamestate == GameState.Conquer) { Output("You must finish conquering before saving."); }
            else if ((Gamestate == GameState.Attacking) && (Gamemode == GameMode.Classic)) { Output("You must finish your attack before saving."); }
            else
            {
                try
                {
                    CancelUnconfirmedActions(); // cancel actions
                    GameManager.SaveGame(game); // save
                    Output("Game saved successfully"); // alert user
                }
                catch { Output("An error has occurred. The game may not have saved, please try again."); };
            }
        }
        /// <summary>
        /// Closes window.
        /// </summary>
        private void Quit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Player Actions
        /// <summary>
        /// Makes player owner of territory, adds army to territory and updates UI.
        /// </summary>
        /// <param name="T">Territory to place/conquer/reinforce</param>
        /// <param name="num">Number of armies to place.</param>
        private void Place_Reinforce(Territory T, int num)
        {
            if (T.owner != CurrentPlayer) // if player does not own territory
            {
                if (T.owner != null) { T.owner.Territoriesowned -= 1; } // take territory from previous owner
                T.owner = CurrentPlayer; // make player owner
                T.button.Background = T.owner.Color; // update UI to reflect
                CurrentPlayer.Territoriesowned += num;
                CurrentPlayer.score += 1; // increase player score
            }
            // if placing new armies, increase army strength
            if ((Gamestate == GameState.InitialArmyPlace) || (Gamestate == GameState.PlacingArmy)) { CurrentPlayer.Army_strength += num; }
            T.currentarmies += num;
            // remove from un-deployed
            if(Gamestate == GameState.InitialArmyPlace) { CurrentPlayer.army_undeployed -= num; }
            T.button.Content = T.currentarmies; // update UI
        }
        /// <summary>
        /// Proceeds to next game phase, ending turn after move phase.
        /// </summary>
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
        /// <summary>
        /// Sets up player actions to be confirmed later.
        /// </summary>
        /// <param name="increase">Is player increasing context number?</param>
        private void PlayerActions(bool increase)
        {
            if (SlctTerritory != null) // if player has selected territory
            {
                int i = -1; // default to decrease
                switch (Gamestate)
                {
                    case GameState.PlacingArmy:
                        if (increase == true)
                        {
                            if (CurrentPlayer.army_undeployed > 0) { i = 1; } // check player has armies left to place, if they do set i to increase
                            else
                            {
                                Output("You do not have any armies left to place");
                                break;
                            }
                        }
                        else
                        {
                            if(SlctTerritory.temparmies <= 1) // player must place at least one army
                            {
                                Output("You must place at least one army");
                                break;
                            }
                        }
                        SlctTerritory.temparmies += i; // increment/decrement by value
                        CurrentPlayer.army_undeployed -= i; // increment/decrement by value
                        UpdateNumOutput();
                        break;
                    case GameState.Attacking:
                        if (NextTerritory != null) // if player has selected a territory to attack
                        {
                            if (increase == true) { i = 1; } // increment if increase
                            AdjustAttackMoves(i);
                        }
                        else { Output("You must select the territories you wish to attack to/from."); } // error output
                        break;
                    case GameState.Conquer: // merge these with above for efficiency
                    case GameState.Move:
                        if (increase == true) { i = 1; }  // increment if increase
                        AdjustAttackMoves(i);
                        break;
                }
            }
            else { Output("Please select a territory."); }
        }
        #endregion

        #region Dice Methods, Events and Properties
        // Die objects used in classic risk for battles.
        PlayerDice playerdie1;
        PlayerDice playerdie2;
        PlayerDice playerdie3;
        EnemyDice enemydie1;
        EnemyDice enemydie2;
        private int rolled; // stores number of dice that have rolled
        public int Rolled // Accessor for rolled variable. When rolled = toRoll AllRollsCompleted is called.
        {
            get { return rolled; }
            set
            {
                rolled = value;
                if (rolled == toRoll)
                {
                    AllRollsCompleted();
                }
            }
        }
        private int toRoll; // stores number of dice that must be rolled.
        public int ToRoll
        {
            get { return toRoll; }
            set { toRoll = value; }
        } // Accessor for toRoll variable.
        List<Dice> dices = new List<Dice>(); // list of dice currently in use
        /// <summary>
        /// Increase rolled by one. Called each time a dice object finishes a roll.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DieRollComplete(object sender, RunWorkerCompletedEventArgs e) { Rolled += 1; }
        /// <summary>
        /// Carries out logic after all dice have rolled in a battle in classic Risk.
        /// Determines player's and enemy's highest rolls,
        /// updates UI and territory properties.
        /// </summary>
        private void AllRollsCompleted()
        {
            // determine player and enemy's highest die roll(s)
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
            // determine which player won each battle
            if((playerNextHighest != -1) && (enemyNextHighest != -1))
            {
                if(ClassicBattle(playerNextHighest, enemyNextHighest)) { enemyloss += 1; CurrentPlayer.score += 1; }
                else { playerloss += 1; }
            }
            // carry out result of battle
            if(ClassicBattle(playerHighestRoll, enemyHighestRoll)) { enemyloss += 1; CurrentPlayer.score += 1; }
            else { playerloss += 1; }
            CurrentPlayer.Army_strength -= playerloss;
            NextTerritory.temparmies -= playerloss;
            NextTerritory.owner.Army_strength -= enemyloss;
            NextTerritory.currentarmies -= enemyloss;
            NextTerritory.button.Content = NextTerritory.currentarmies;
            // Output to user result
            Output(String.Format("You lost {0} armies in battle. The enemy lost {1}", playerloss, enemyloss));
            if (NextTerritory.currentarmies == 0)
            {
                Output("You have successfully captured this territory.");
                btnDieStatus.Content = "Continue to Conquer";
            }
            else { btnDieStatus.Content = "Continue to Attack"; }
            btnDieStatus.Visibility = Visibility.Visible;
            // un-pause game
            paused = false;
        }
        /// <summary>
        /// Returns to attack if the player did not eliminate all enemy armies,
        /// ends turn if the timer expired,
        /// conquers territory if timer expires.
        /// Determines if player has won game.
        /// </summary>
        private void Attack_DieContinue(object sender, RoutedEventArgs e)
        {
            if (((String)((Button)sender).Content) == "Dice Rolling...") // if dice still rolling exit
            {
                return;
            }
            btnDieStatus.Visibility = Visibility.Collapsed; // redundant?
            panel_Die.Visibility = Visibility.Collapsed;
            panel_NumberSelection.Visibility = Visibility.Visible;
            if (((String)((Button)sender).Content) == "Continue to Attack") // if not eliminated all
            {
                CancelUnconfirmedActions(); // cancel
                if (Time > 0) // if timer expired > next turn
                {
                    if (workerthread.IsBusy == false) { NextTurnThreaded(); }
                }
            }
            else // player must conquer territory
            {
                // adjust properties
                NextTerritory.owner.Territoriesowned -= 1;
                //NextTerritory.owner.Army_strength -= NextTerritory.currentarmies;
                NextTerritory.owner = CurrentPlayer;
                CurrentPlayer.Territoriesowned += 1;
                // check if player has won game
                bool won = true;
                foreach (Player p in Players)
                {
                    if (p != CurrentPlayer && p.Territoriesowned > 0) { won = false; }
                }
                if (won) { Win(); } // if won end game
                UpdateState(GameState.Conquer); // proceed to conquer
            }
            btnDieStatus.Content = "Outcome";
        }
        /// <summary>
        /// Determines the highest and second highest number out of 2 sorted values and a new value
        /// </summary>
        /// <param name="greatest">Largest number</param>
        /// <param name="secondgreatest">Second largest number</param>
        /// <param name="newnum">New number to compare</param>
        private void DetermineHigherRoll(ref int greatest, ref int secondgreatest, int newnum)
        {
            if(newnum > secondgreatest)
            {
                if(newnum > greatest)
                {
                    secondgreatest = greatest; // move greatest to second-greatest
                    greatest = newnum; // as new-number is the largest
                }
                else { secondgreatest = newnum; } //new-number is second-largest
            }
        }
        /// <summary>
        /// Determines which player wins based on their die roll in classic risk
        /// </summary>
        /// <param name="player">Number player rolled</param>
        /// <param name="enemy">Number enemy rolled</param>
        /// <returns></returns>
        private bool ClassicBattle(int player, int enemy)
        {
            if(enemy >= player) { return false; } // player only wins if they roll greater than enemy
            else { return true; }
        }
        #endregion
        // testing method
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Win();
        }
    }
}
