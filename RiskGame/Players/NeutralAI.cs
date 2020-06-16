using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.enemyAI
{
    [Serializable]
    public class NeutralAI : AI
    {
        public NeutralAI(string username) : base(username)
        {
        }
    }
}
