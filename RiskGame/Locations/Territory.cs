using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace RiskGame.Locations
{
    [Serializable]
    public class Territory : INotifyPropertyChanged
    {
        // Variables //
        public readonly String name;
        public readonly List<String> links;
        public Player owner;
        // UI //
        public Button button;
        public Label current;
        public Label temp;
        private int currentarmies;
        public int Currentarmies
        {
            get { return this.currentarmies; }
            set
            {
                if(this.currentarmies != value)
                {
                    this.currentarmies = value;
                    this.NotifyPropertyChanged("Currentarmies");
                }
            }
        }

        public int Temparmies
        {
            get { return this.temparmies; }
            set
            {
                if(this.temparmies != value)
                {
                    this.currentarmies = value;
                    this.NotifyPropertyChanged("Temparmies");
                }
            }
        }

        private int temparmies = 0; // used for storing number to be placed, number attacking, number to be moved to, moved from

        public Territory(string name, List<string> links, Button button, Label current, Label temp)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.links = links ?? throw new ArgumentNullException(nameof(links));
            this.button = button ?? throw new ArgumentNullException(nameof(button));
            this.current = current ?? throw new ArgumentNullException(nameof(current));
            this.temp = temp ?? throw new ArgumentNullException(nameof(temp));
            BindLabel(ref current, "Currentarmies");
            BindLabel(ref temp, "Temparmies");
            Currentarmies = 0;
            Temparmies = 0;
        }

        public void NotifyPropertyChanged(string propName)
        {
            if(this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(propName)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void BindLabel(ref Label label, string bindingsource)
        {
            Binding b = new Binding(bindingsource);
            b.Source = this;
            label.SetBinding(Label.ContentProperty, b);
        }
    }
}
