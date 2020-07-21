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
        public LoginException() : base("Incorrect Password") { }
    }
}
