using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RiskGame.Game.Locations
{
    /// <summary>
    /// Class for map areas.
    /// </summary>
    [Serializable]
    public class Territory : IComparable<Territory>
    {
        #region
        /// <summary>
        /// Name of area.
        /// </summary>
        public readonly String name;
        /// <summary>
        /// Names of territories adjacent or linked to this object.
        /// </summary>
        public readonly List<String> links;
        public Player owner;
        /// <summary>
        /// Number of armies currently placed by owner in the territory,
        /// </summary>
        public int currentarmies = 0;
        /// <summary>
        /// Used for storing number to be placed, number attacking, number to be moved to, moved from
        /// </summary>
        public int temparmies = 0;
        /// <summary>
        /// UI button representing territory.
        /// </summary>
        [NonSerialized]
        public Button button;
        #endregion

        public Territory(string name, List<string> links, Button button)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
            this.button = button ?? throw new ArgumentNullException(nameof(button));
        }

        /// <summary>
        /// Compares territory names alphabetically.
        /// Implementation of IComparable interface.
        /// </summary>
        /// <param name="other">Territory to be compared to</param>
        public int CompareTo(Territory other)
        {
            return string.Compare(this.name, other.name);
        }
    }
}
