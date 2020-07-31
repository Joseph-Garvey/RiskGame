using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.enemyAI
{
    /// <summary>
    /// AI Class type that prefers to fortify its own borders before attacking.
    /// </summary>
    [Serializable]
    public class DefensiveAI : AI
    {
        public DefensiveAI(string username) : base(username)
        {
        }
    }
}
