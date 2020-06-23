using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.Game.Locations
{
    [Serializable]
    public class Continent
    {
        // Variables //
        public readonly string name;
        public readonly List<Territory> territories;
        public readonly int bonus;
        // Constructor //
        public Continent(string name, List<Territory> territories, int bonus)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.territories = territories ?? throw new ArgumentNullException(nameof(territories));
            this.bonus = bonus;
        }
    }
}
