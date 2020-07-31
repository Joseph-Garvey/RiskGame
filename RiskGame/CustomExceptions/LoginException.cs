using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    /// <summary>
    /// Custom Exception thrown when the user inputs an incorrect password.
    /// </summary>
    [Serializable]
    public class LoginException : Exception
    {
        /// <summary>
        /// Creates default message "Incorrect Password"
        /// </summary>
        public LoginException() : base("Incorrect Password") { }
    }
}
