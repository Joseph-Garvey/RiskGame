using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace RiskGame
{
    /// <summary>
    /// The basic player class which acts as a template for human and AI players.
    /// Used for polymorphism.
    /// </summary>
    [Serializable]
    public abstract class Player : INotifyPropertyChanged
    {
        // Constructors //
        public Player(string username)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            army_strength = 0;
            Territoriesowned = 0;
        }

        #region Variables
        public String Username { get; set; }
        /// <summary>
        /// Number of armies not yet deployed.
        /// </summary>
        public int army_undeployed;
        /// <summary>
        /// Number of territories owned by player.
        /// </summary>
        private int territoriesowned;
        /// <summary>
        /// Accessor for territoriesowned. Notifies UI when value changed, used for data-binding.
        /// </summary>
        public int Territoriesowned
        {
            get { return this.territoriesowned; }
            set
            {
                if (this.territoriesowned != value)
                {
                    this.territoriesowned= value;
                    this.NotifyPropertyChanged("Territoriesowned");
                }
            }
        }
        /// <summary>
        /// Stores number of armies deployed by player.
        /// </summary>
        private int army_strength;
        /// <summary>
        /// Accessor for territoriesowned. Notifies UI when value changed, used for data-binding.
        /// </summary>
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
        public int score = 0;
        /// <summary>
        /// Red value of player colour.
        /// </summary>
        private byte r;
        /// <summary>
        /// Green value of player colour.
        /// </summary>
        private byte g;
        /// <summary>
        /// Blue value of player colour.
        /// </summary>
        private byte b;
        /// <summary>
        /// Player's selected colour.
        /// </summary>
        [NonSerialized]
        private SolidColorBrush color;
        /// <summary>
        /// Accessor for color. Sets value and splits colour into its RGB values, setting r, g and b values if class.
        /// </summary>
        public SolidColorBrush Color
        {
            get => color;
            set
            {
                color = value;
                if (color == null)
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
                else
                {
                    // split colour into its component values.
                    r = color.Color.R;
                    g = color.Color.G;
                    b = color.Color.B;
                }
            }
        }
        /// <summary>
        /// UI element displaying the player's army strength.
        /// </summary>
        [NonSerialized]
        private Label disp_ArmyStrength;
        /// <summary>
        /// UI element displaying the player's number of owned territories.
        /// </summary>
        [NonSerialized]
        private Label disp_Owned;
        /// <summary>
        /// Accessor for disp_ArmyStrength, binds the label content to the player's property.
        /// </summary>
        public Label Disp_ArmyStrength
        {
            get => disp_ArmyStrength;
            set
            {
                disp_ArmyStrength = value;
                BindLabel(ref disp_ArmyStrength, "Army_strength");
            }
        }
        /// <summary>
        /// Accessor for disp_Owned, binds the label content to the player's property.
        /// </summary>
        public Label Disp_Owned
        {
            get => disp_Owned;
            set
            {
                disp_Owned = value;
                BindLabel(ref disp_Owned, "Territoriesowned");
            }
        }
        #endregion

        // Methods //
        /// <summary>
        /// Reconstructs the solid color brush from the saved R,G,B values.
        /// </summary>
        public void RetrieveColor()
        {
            Color = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }

        // Binding //
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies UI that property has changed.
        /// </summary>
        /// <param name="propName"></param>
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        /// <summary>
        /// Binds label to supplied property.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="bindingsource"></param>
        private void BindLabel(ref Label label, string bindingsource)
        {
            Binding b = new Binding(bindingsource);
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Source = this;
            label.SetBinding(Label.ContentProperty, b);
        }
    }
}
