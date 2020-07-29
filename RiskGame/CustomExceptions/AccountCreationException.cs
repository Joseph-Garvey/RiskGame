using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    internal class AccountCreationException : Exception
    {
        /// <summary>
        /// This class is used when validating an account to ensure that each account meets the basic security requirements.
        /// It outputs a message telling the user what they requirements they failed to meet.
        /// </summary>

        // Static Variables //
        // Each of these strings contains the output text for each failed test //
        private static readonly String invalidUserLength = "Usernames must be between 3 and 10 characters.";
        private static readonly String invalidUserChar = "Only letters, numbers and seperator characters (Underscore, dash, period, comma, slash) allowed in usernames. ";
        private static readonly String invalidUserExists = "This username is taken. ";
        private static readonly String invalidPassLength = "Passwords must be between 8 and 15 characters. ";
        private static readonly String invalidPassChar = "Password must consist of letters, numbers and symbols. ";
        private static readonly String invalidPassSecurity = "Passwords must contain a minimum of one number, symbol, upper and lower case character. ";
        private static readonly String invalidSymbol = "Refer to the tutorial window for the list of allowed symbols.";
        public String error = ""; // This string holds the final error output.

        // Constructor
        // The constructor takes in a boolean indicating whether the user passed or failed a given test and appends the respective error message to the error output.
        public AccountCreationException(bool validlength, bool validcharuser, bool validuserexists, bool validpasslength, bool validpasschar, bool validpasssecurity)
        {
            if (!validlength) { error += invalidUserLength; }
            if (!validcharuser) { error += invalidUserChar; }
            if (!validuserexists) { error += invalidUserExists; }
            if (!validpasslength) { error += invalidPassLength; }
            if (!validpasschar) { error += invalidPassChar; }
            if (!validpasssecurity) { error += invalidPassSecurity; }
            if(!validpasschar || !validpasssecurity) { error += invalidSymbol; }
        }
    }
}
