﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    internal class AccountCreationException : Exception
    {
        // custom parameter for exception or create string, con-cat then pass into message if exception created, or Exception with message = prefab as parameter as constructor. String from individual methods passed up chain?
        // or use exception method trace (targetsite exception) and a series of try finally
        private static readonly String invalidUserLength = "Usernames must be between 5 and 10 characters including whitespace. ";
        private static readonly String invalidUserChar = "Only letters, numbers and whitespace/seperator characters allowed in usernames. ";
        private static readonly String invalidUserExists = "This username is taken. ";
        private static readonly String invalidPassLength = "Passwords must be between 8 and 20 characters. ";
        private static readonly String invalidPassChar = "Password must consist of letters, numbers and symbols. ";
        private static readonly String invalidPassSecurity = "Passwords must contain a minimum of one number, symbol and uppercase character.";
        public String error = "";
        public AccountCreationException(bool validlength, bool validcharuser, bool validuserexists, bool validpasslength, bool validpasschar, bool validpasssecurity)
        {
            if (!validlength) { error += invalidUserLength; }
            if (!validcharuser) { error += invalidUserChar; }
            if (!validuserexists) { error += invalidUserExists; }
            if (!validpasslength) { error += invalidPassLength; }
            if (!validpasschar) { error += invalidPassChar; }
            if (!validpasssecurity) { error += invalidPassSecurity; }
        }
    }
}
