using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RiskGame.CustomExceptions;

namespace RiskGame
{
    /// <summary>
    /// Class used for human players.
    /// </summary>
    [Serializable]
    public class Human : Player
    {
        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="username">Player's username</param>
        /// <param name="password">User's password</param>
        /// <param name="music_enabled">User's music preference.</param>
        /// <param name="hints_enabled">User's hint preference.</param>
        public Human(string username, string password, bool music_enabled, bool hints_enabled) : base(username)
        {
            Password = password ?? throw new ArgumentNullException(nameof(password));
            this.music_enabled = music_enabled;
            this.hints_enabled = hints_enabled;
        }
        #endregion

        #region Variables and Properties
        /// <summary>
        /// File-path for saved accounts.
        /// </summary>
        private static readonly String FileName = "Usersaves.txt";
        /// <summary>
        /// Stores user password.
        /// </summary>
        private String password;
        /// <summary>
        /// Write-only accessor for password variable.
        /// </summary>
        public String Password { set => password = value; }
        /// <summary>
        /// User's music preference.
        /// </summary>
        public bool music_enabled;
        /// <summary>
        /// User's hints preference.
        /// </summary>
        public bool hints_enabled;
        /// <summary>
        /// Allowed symbols for passwords.
        /// </summary>
        private static List<char> legalsymbols = new List<char> { '!', '£', '$', '€', '%', '^', '&', '*', '+', '-', '=', '_', '<', '>', '@','?','~', '#'};
        #endregion

        #region Account Management
        /// <summary>
        /// Attempts to find the supplied user details in the text file.
        /// Creates a new object and returns it if successful
        /// </summary>
        /// <returns></returns>
        public static Human SignIn(String username, String password) // Must use inside try/catch
        {
            if(username == "" || username == null || password == "" || password == null) { throw new ArgumentNullException(); } // if fields empty throw exception
            using (StreamReader sr = new StreamReader(FileName)) // using auto-cleans up on leave
            {
                string line;
                while ((line = sr.ReadLine()) != null) // while not at end of file.
                {
                    string tmpusername = line.Substring(0, 10); // split line into set length substrings, containing the username and password contained within that line.
                    string tmppassword = line.Substring(10, 15);
                    string m_enabled = line.Substring(25, 5);
                    string h_enabled = line.Substring(30, 5);
                    tmpusername = tmpusername.Trim(); // trim spaces and whitespace
                    tmppassword = tmppassword.Trim();
                    m_enabled = m_enabled.Trim();
                    h_enabled = h_enabled.Trim();
                    bool music_enabled = new bool();
                    bool.TryParse(m_enabled , out music_enabled); // convert strings to bool values
                    bool hints_enabled = new bool();
                    bool.TryParse(h_enabled, out hints_enabled);
                    if ((tmpusername == username) && (tmppassword == password)) // If matches given details return object.
                    {
                        Human player = new Human(username, password, music_enabled, hints_enabled);
                        return player;
                    }
                    else if(tmpusername == username) { throw new LoginException(); } // Otherwise throw exception indicating the account does not exist or cannot be found.
                }
                throw new AccountNotFoundException();
            }
        }
        /// <summary>
        /// Registers new accounts.
        /// </summary>
        public static void Register(String username, String password, bool music_enabled, bool hints_enabled) // must be used in try/catch
        {
            // Call Validation on the given details to ensure they are valid.
            Validation(username, password);
            // if pass create player
            Human newplayer = new Human(username, password, music_enabled, hints_enabled);
            Human.Add(newplayer); // Write the new account to file.
        }
        /// <summary>
        /// Adds the player object to the accounts file.
        /// </summary>
        /// <param name="newplayer">Account to be written to file.</param>
        private static void Add(Human newplayer)
        {
            using (StreamWriter sr = new StreamWriter(FileName, true)) // STREAM ONLY EXISTS FOR EXECUTION
                // Open file in append mode
            {
                String write = String.Format("{0}{1}{2}{3}", (newplayer.Username).PadRight(10), (newplayer.password).PadRight(15), (newplayer.music_enabled.ToString()).PadRight(5), (newplayer.hints_enabled.ToString()).PadRight(5));
                sr.WriteLine(write); /// Format the account as a string and write to file.
            }
        }
        /// <summary>
        /// Update an existing player's preferences on file.
        /// </summary>
        /// <param name="player">User account to write to file.</param>
        public static void Update(Human player)
        {
            List<String> lines = new List<string>();
            using (StreamReader sr = new StreamReader(FileName))
            {
                string tmpplayername = player.Username.PadRight(10);
                string tmpplayerpassword = player.password.PadRight(15);
                string line; // Format the players name to match that on file.
                while ((line = sr.ReadLine()) != null) // While it has not reached the end of the file.
                {
                    string tmpusername = line.Substring(0, 10); // Extract the username from the line.
                    string tmppassword = line.Substring(10, 15); // Extract the password from the line.
                    if ((tmpplayername == tmpusername) && (tmpplayerpassword == tmppassword)) // if this line matches the player
                    {
                        // create line to write to file
                        lines.Add(String.Format("{0}{1}{2}{3}", tmpplayername, tmpplayerpassword, player.music_enabled.ToString().PadRight(5), player.hints_enabled.ToString().PadRight(5)));
                    }
                    else { lines.Add(line); }
                }
            }
            using (StreamWriter sw = new StreamWriter(FileName, false))
            { // write the line in the same place on the file.
                foreach (String s in lines)
                {
                    sw.WriteLine(s);
                }
            }
        }
        /// <summary>
        /// Updates an existing player's preferences and password on file.
        /// </summary>
        /// <param name="player">User account to write to file.</param>
        /// <param name="newpassword">New user password.</param>
        public static void Update(Human player, String newpassword)
        {
            // Update an existing player's details on file.
            List<String> lines = new List<string>();
            using (StreamReader sr = new StreamReader(FileName))
            {
                string tmpplayername = player.Username.PadRight(10);
                string tmpplayerpassword = player.password.PadRight(15);
                string line; // Format the players name to match that on file.
                while ((line = sr.ReadLine()) != null) // While it has not reached the end of the file.
                {
                    string tmpusername = line.Substring(0, 10); // Extract the username from the line.
                    string tmppassword = line.Substring(10, 15); // Extract the password from the line.
                    if ((tmpplayername == tmpusername) && (tmpplayerpassword == tmppassword)) // if this line matches the player
                    {
                        lines.Add(String.Format("{0}{1}{2}{3}", tmpplayername, newpassword.PadRight(15), player.music_enabled.ToString().PadRight(5), player.music_enabled.ToString().PadRight(5)));
                    }
                    else { lines.Add(line); }
                }
            }
            using (StreamWriter sw = new StreamWriter(FileName, false))
            { // write the line in the same place on the file.
                foreach (String s in lines)
                {
                    sw.WriteLine(s);
                }
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validates if the user's name and password are valid by checking them against certain rules.
        /// Throws AccountCreationException if invalid.
        /// </summary>
        public static void Validation(String username, String password) // Must be used in try/catch
        {
            if (username == "" || username == null || password == "" || password == null) { throw new ArgumentNullException(); } // if fields empty throw argument null exception
            Char[] userchar = username.ToCharArray();
            Char[] passchar = password.ToCharArray();
            if ((!ValidUserLength(userchar)) || (!ValidCharUser(userchar)) || (!ValidNameTaken(username)) || (!ValidPassLength(passchar)) || (!ValidPassChar(passchar)) || (!ValidPassSecurity(passchar))) { throw new AccountCreationException(ValidUserLength(userchar), ValidCharUser(userchar), ValidNameTaken(username), ValidPassLength(passchar), ValidPassChar(passchar), ValidPassSecurity(passchar)); }
            // can be made more efficient as in current state checks Username at least twice on the first parameter that throws exception rather than storing result
        }
        /// <summary>
        /// Validates if the user's new password is valid by checking it against certain rules.
        /// Throws AccountCreationException if invalid
        /// </summary>
        public static void Validation(String password)
        {
            if (password == "" || password == null) { throw new ArgumentNullException(); } // if fields empty throw argumentnullexception
            Char[] passchar = password.ToCharArray();
            if ((!ValidPassLength(passchar)) || (!ValidPassChar(passchar)) || (!ValidPassSecurity(passchar))) { throw new AccountCreationException(true, true, true, ValidPassLength(passchar), ValidPassChar(passchar), ValidPassSecurity(passchar)); }
        }
        /// <summary>
        /// Checks if supplied username meets length requirements.
        /// </summary>
        /// <param name="chararray">Username as character array.</param>
        private static bool ValidUserLength(Char[] chararray)
        {
            if ((chararray.Length > 10) || (chararray.Length < 3)) { return false; }
            return true;
        }
        /// <summary>
        /// Checks the username consists of only letters digits and legal separators.
        /// </summary>
        /// <param name="chararray">Username as character array.</param>
        private static bool ValidCharUser(Char[] chararray) // cannot use if else on validation classes as then it will return earlier than expected.
        {
            List<char> legalseparators = new List<char> { '_', '-', '.', ',', '/' };
            foreach (Char c in chararray)
            {
                if (!((Char.IsLetterOrDigit(c)) || (legalseparators.Contains(c)))) { return false; }
            }
            return true;
        }
        /// <summary>
        /// Checks if username is in use.
        /// </summary>
        /// <returns>True if username not taken.</returns>
        private static bool ValidNameTaken(String username)
        {
            if (File.Exists(FileName))
            {
                using (StreamReader sr = new StreamReader(FileName))
                {
                    // Reads file looking for a match.
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string tmpusername = line.Substring(0, 10);
                        tmpusername = tmpusername.Trim();
                        if (tmpusername == username) { return false; } // if username matches return false.
                    }
                    return true;
                }
            } // If there isn't a match the username is valid.
            return true;
        }
        /// <summary>
        /// Checks supplied password meets length requirements.
        /// </summary>
        /// <param name="chararray">Password as character array.</param>
        private static bool ValidPassLength(Char[] chararray)
        {
            if ((chararray.Length > 15) || (chararray.Length < 8)) { return false; }
            return true;
        }
        /// <summary>
        /// Ensures the password only consists of letters, digits, symbols and punctuation.
        /// </summary>
        /// <param name="chararray">Password as character array.</param>
        private static bool ValidPassChar(Char[] chararray)
        {
            foreach (Char c in chararray)
            {
                if (!((Char.IsLetterOrDigit(c)) || legalsymbols.Contains(c) )) { return false; }
            }
            return true;
        }
        /// <summary>
        /// Checks that the password contains at least one uppercase letter, (allowed)symbol/punctuation and number.
        /// </summary>
        /// <param name="chararray">Password as character array.</param>
        private static bool ValidPassSecurity(Char[] chararray) // refine with sub-methods  in future so that a boolean doesn't have to be stored?
        {
            bool uppercase = false;
            bool symbol = false;
            bool digit = false;
            bool lowercase = false;
            foreach (Char c in chararray) // For every character, make the boolean true if it matches one of these conditions.
            {
                if (Char.IsUpper(c)) { uppercase = true; }
                else if(Char.IsLower(c)) { lowercase = true; }
                else if (legalsymbols.Contains(c)) { symbol = true; }
                else if (Char.IsDigit(c)) { digit = true; }
            }
            if (uppercase && symbol && digit && lowercase) { return true; } // If it matches all criteria, return true.
            return false;
        }
        #endregion
    }
}
