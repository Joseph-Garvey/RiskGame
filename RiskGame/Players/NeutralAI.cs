using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.enemyAI
{
    /// <summary>
    /// Neutral AI object that can own and defend territories but cannot attack, move or receive reinforcements.
    /// </summary>
    [Serializable]
    public class NeutralAI : AI
    {
        public NeutralAI(string username) : base(username)
        {
        }
    }
}
