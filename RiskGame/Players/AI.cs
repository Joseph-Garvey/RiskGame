﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.enemyAI
{
    /// <summary>
    /// Base AI class type.
    /// </summary>
    [Serializable]
    public abstract class AI : Player
    {
        // Constructor //
        public AI(String username) : base(username) // AI does not require username/password
        {
        }
    }
}
