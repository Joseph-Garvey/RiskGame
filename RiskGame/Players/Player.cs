using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;

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
        private int army_strength;
        public int Army_strength
        {
            get { return this.army_strength; }
            set
            {
                if (this.army_strength != value)
                {
                    this.army_strength = value;
                    this.NotifyPropertyChanged("Army_strength");
                }
            }
        }
        private Label disp_ArmyStrength;
        public Label Disp_ArmyStrength
        {
            get => disp_ArmyStrength;
            set
            {
                disp_ArmyStrength = value;
                BindLabel(ref disp_ArmyStrength, "Army_strength");
            }
        }
        public int score = 0;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void BindLabel(ref Label label, string bindingsource)
        {
            Binding b = new Binding(bindingsource);
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Source = this;
            label.SetBinding(Label.ContentProperty, b);
        }
    }
}
