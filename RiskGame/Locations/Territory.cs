using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RiskGame.Game.Locations
{
    [Serializable]
    public class Territory : IComparable<Territory>
    {
        // Variables //
        public readonly String name;
        public readonly List<String> links;
        public Player owner;
        public int currentarmies = 0;
        public int temparmies = 0; // used for storing number to be placed, number attacking, number to be moved to, moved from
        [NonSerialized]
        public Button button;
        public Territory(string name, List<string> links, Button button)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
            this.button = button ?? throw new ArgumentNullException(nameof(button));
        }
        public int CompareTo(Territory other)
        {
            return string.Compare(this.name, other.name);
        }
    }
}
