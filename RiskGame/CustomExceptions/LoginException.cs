using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    public class LoginException : Exception
    {
        // Custom Exception thrown when the user inputs an incorrect password.
        public LoginException() : base("Incorrect Password") { }
    }
}
