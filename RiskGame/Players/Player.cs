using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;

namespace RiskGame
{
    [Serializable]
    public class Player
    {
       /// <summary>
       /// The basic player class which contains the basic variables for human and ai players.
       /// Used for polymorphism ///
       /// </summary>
       /// <param name="username"></param>
       ///
        // Constructors //
        public Player(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
        }
        // Variables //
        public String Username { get; set; }
        public int army_undeployed;
        public int territoriesowned = 0;
        public int army_strength;
        public int score;
        private byte r;
        private byte g;
        private byte b;
        [NonSerialized]
        private SolidColorBrush color;
        public SolidColorBrush Color
        {
            get => color;
            set
            {
                color = value;
                r = color.Color.R;
                g = color.Color.G;
                b = color.Color.B;
            }
        }

        // Methods //
        public void RetrieveColor()
        {
            Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }
    }
}
