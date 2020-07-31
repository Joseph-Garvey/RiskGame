using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.CustomExceptions
{
    /// <summary>
    /// Custom exception thrown when a game is not found in the gamesaves file.
    /// </summary>
    [Serializable]
    internal class GameNotFoundException : Exception
    {
        public GameNotFoundException()
        {
        }
    }
}
