using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame.Game.Locations
{
    /// <summary>
    /// Class used for map regions.
    /// </summary>
    [Serializable]
    public class Continent
    {
        #region Variables
        ///<summary>Name of continent</summary>
        public readonly string name;
        /// <summary> Territories in region. </summary>
        public readonly List<Territory> territories;
        /// <summary> Bonus received for conquering continent. </summary>
        public readonly int bonus;
        #endregion
        // Constructor //
        /// <summary>
        /// Default class constructor.
        /// </summary>
        /// <param name="name">Name of continent</param>
        /// <param name="territories">Territories in region.</param>
        /// <param name="bonus">Bonus armies received for conquering region.</param>
        public Continent(string name, List<Territory> territories, int bonus)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.territories = territories ?? throw new ArgumentNullException(nameof(territories));
            this.bonus = bonus;
        }
    }
}
