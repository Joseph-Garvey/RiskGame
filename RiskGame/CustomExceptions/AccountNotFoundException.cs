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
        public AccountNotFoundException(string message = "Account not found.") : base(message)
        {
        }
    }
}
