using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    [Serializable]
    internal class TerritoryNotFoundException : Exception
    {
        /// <summary>
        /// Custom exception thrown when the game cannot find the requested territory in the list of territories.
        /// </summary>
        public TerritoryNotFoundException()
        {
        }
    }
}
