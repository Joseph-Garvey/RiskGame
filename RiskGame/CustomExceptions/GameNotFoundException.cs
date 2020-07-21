using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    internal class GameNotFoundException : Exception
    {
        public GameNotFoundException()
        {
        }
    }
}
