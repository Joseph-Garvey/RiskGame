using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.Locations
{
    [Serializable]
    public class Territory
    {
        // Variables //
        public readonly String name;
        public readonly List<String> links;
        public Player owner;
        public int currentarmies = 0;
        public int temparmies = 0; // used for storing number to be placed, number attacking, number to be moved to, moved from
        public Territory(string territoryname, List<String> links)
        {
            this.name = territoryname ?? throw new ArgumentNullException(nameof(territoryname));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
        }
    }
}
