using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RiskGame.CustomExceptions;

namespace RiskGame
{
    [Serializable]
    public class Human : Player
    {
        // Global Constants //
        private static readonly String FileName = "Usersaves.txt";
        // Constructors //
        public Human(string username, string password, bool music_enabled) : base(username)
        {
            Password = password ?? throw new ArgumentNullException(nameof(password));
            this.music_enabled = music_enabled;
        }
        // Variables //
        private String password;
        public String Password { set => password = value; } // Makes password write-only
        public bool music_enabled;

        // Account Management //
        public static Human SignIn(String username, String password) // Must use inside try/catch
        {
            /// Attempts to find the given details in the text file. Creates a new object and returns it if successful
            using (StreamReader sr = new StreamReader(FileName)) // using auto-cleans up on leave
            {
                string line;
                while ((line = sr.ReadLine()) != null) // while not at end of file.
                {
                    string tmpusername = line.Substring(0, 10); // split line into set length substrings, containing the username and password contained within that line.
                    string tmppassword = line.Substring(10, 15);
                    string m_enabled = line.Substring(25, 5);
                    tmpusername = tmpusername.Trim();
                    tmppassword = tmppassword.Trim();
                    m_enabled = m_enabled.Trim();
                    bool music_enabled = new bool();
                    bool.TryParse(m_enabled , out music_enabled);
                    if((tmpusername == username) && (tmppassword == password)) // If mathces given details return object.
                    {
                        Human player = new Human(username, password, music_enabled);
                        return player;
                    }
                    else if(tmpusername == username) { throw new LoginException(); } // Otherwise throw exception indicating the account does not exist or cannot be found.
                }
                throw new AccountNotFoundException();
            }
        }
        public static void Register(String username, String password, bool music_enabled) // must be used in try/catch
        {
            /// Registers new accounts.
            // Call Validation on the given details to ensure they are valid.
            Validation(username, password);
            Human newplayer = new Human(username, password, music_enabled);
            Human.Add(newplayer); // Write the new account to file.
        }
        // File Handling //
        private static void Add(Human newplayer) // This can be simplified by instead passing in just the username and password in future. left over from when player class was serialized.
        {
            using (StreamWriter sr = new StreamWriter(FileName, true)) // STREAM ONLY EXISTS FOR EXECUTION
            {
                String write = String.Format("{0}{1}{2}", (newplayer.Username).PadRight(10), (newplayer.password).PadRight(15), (newplayer.music_enabled.ToString()).PadRight(5));
                sr.WriteLine(write); /// Format the account as a string and write to file.
            }
        }
        ///
        public static void Update(Human player)
        {
            // Update an existing player's details on file.
            using (StreamReader sr = new StreamReader(FileName))
            {
                string tmpplayername = player.Username.PadRight(10);
                string tmpplayerpassword = player.password.PadRight(15);
                string line; // Format the players name to match that on file.
                List<String> lines = new List<string>();
                while ((line = sr.ReadLine()) != null) // While it has not reached the end of the file.
                {
                    string tmpusername = line.Substring(0, 10); // Extract the username from the line.
                    string tmppassword = line.Substring(10, 15); // Extract the password from the line.
                    if ((tmpplayername == tmpusername) && (tmpplayerpassword == tmppassword)) // if this line matches the player
                    {
                        lines.Add(String.Format("{0}{1}{2}", tmpplayername, tmpplayerpassword, player.music_enabled.ToString()));
                    }
                    else { lines.Add(line); }
                }
                using(StreamWriter sw = new StreamWriter(FileName))
                { // write the line in the same place on the file.
                    foreach(String s in lines)
                    {
                        sw.WriteLine(s);
                    }
                }
            }
        }

        // Validation
        public static void Validation(String username, String password) // Must be used in try/catch
        {
            // Validates if the user's name and password are valid by checking them against certain rules.
            Char[] userchar = username.ToCharArray();
            Char[] passchar = password.ToCharArray();
            if ((!ValidUserLength(userchar)) || (!ValidCharUser(userchar)) || (!ValidNameTaken(username)) || (!ValidPassLength(passchar)) || (!ValidPassChar(passchar)) || (!ValidPassSecurity(passchar))) { throw new AccountCreationException(ValidUserLength(userchar), ValidCharUser(userchar), ValidNameTaken(username), ValidPassLength(passchar), ValidPassChar(passchar), ValidPassSecurity(passchar)); }
            // can be made more efficient as in current state checks Username at least twice on the first parameter that throws exception rather than storing result
        }
        private static bool ValidUserLength(Char[] chararray)
        {
            // Checks the username is the correct length.
            if ((chararray.Length > 10) || (chararray.Length < 3)) { return false; }
            return true;
        }
        private static bool ValidCharUser(Char[] chararray) // cannot use if else on validation classes as then it will return earlier than expected.
        {
            // Checks the username consists of only letters digits and seperators.
            foreach (Char c in chararray)
            {
                if (!((Char.IsLetterOrDigit(c)) || (Char.IsSeparator(c)))) { return false; }
            }
            return true;
        }
        private static bool ValidNameTaken(String username)
        {
            // Checks the username is not already taken.
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
                        if (tmpusername == username) { return false; }
                    }
                    return true;
                }
            } // If there isn't a match the username is valid.
            return true;
        }
        private static bool ValidPassLength(Char[] chararray)
        {
            // Ensures the password is the correct length.
            if ((chararray.Length > 15) || (chararray.Length < 8)) { return false; }
            return true;
        }
        private static bool ValidPassChar(Char[] chararray)
        {
            // Ensures the password only consists of letters, digits, symbols and punctuation.
            foreach (Char c in chararray)
            {
                if (!((Char.IsLetterOrDigit(c)) || (Char.IsSymbol(c)) || (Char.IsPunctuation(c)))) { return false; }
            }
            return true;
        }
        private static bool ValidPassSecurity(Char[] chararray) // refine with sub-methods  in future so that a boolean doesn't have to be stored?
        {
            // Checks that the password contains at least one uppercase letter, symbol/punctuation and number. This keeps passwords secure.
            bool uppercase = false;
            bool symbol = false;
            bool digit = false;
            foreach (Char c in chararray) // For every character, make the boolean true if it matches one of these conditions.
            {
                if (Char.IsUpper(c)) { uppercase = true; }
                else if (Char.IsSymbol(c)) { symbol = true; }
                else if (Char.IsDigit(c)) { digit = true; }
                else if (Char.IsPunctuation(c)) { symbol = true; } // as some Unicode 'symbols' are considered punctuation e.g @
                if (uppercase && symbol && digit) { return true; } // If it matches all criteria, return true.
            }
            return false;
        }
    }
}
