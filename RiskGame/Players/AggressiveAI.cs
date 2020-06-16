using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.enemyAI
{
    [Serializable]
    public class AggressiveAI : AI
    {
        public AggressiveAI(string username) : base(username)
        {
        }
    }
}
