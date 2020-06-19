using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RiskGame.Locations
{
    [Serializable]
    public class Territory
    {
        // Variables //
        public readonly String name;
        public readonly List<String> links;
        public Player owner;
        public Button button;
        public int currentarmies = 0;
        public int temparmies = 0; // used for storing number to be placed, number attacking, number to be moved to, moved from
        public Territory(string name, List<string> links, Button button)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
            this.button = button ?? throw new ArgumentNullException(nameof(button));
        }
    }
}
