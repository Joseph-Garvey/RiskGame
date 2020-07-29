using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    internal class AccountNotFoundException : Exception
    {
        /// <summary>
        /// This class is used when an account cannot be found in the usersaves file.
        /// It passes a default message to the base exception class on instantiation.
        /// </summary>
        public AccountNotFoundException() : base("Account not found.")
        {
        }
    }
}
