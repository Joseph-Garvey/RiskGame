using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    /// <summary>
    /// This class is used when an account cannot be found in the user-saves file.
    /// </summary>
    [Serializable]
    internal class AccountNotFoundException : Exception
    {
        /// <summary>
        /// Default constructor passes "Account not found" message to base class.
        /// </summary>
        public AccountNotFoundException() : base("Account not found.")
        {
        }
    }
}
