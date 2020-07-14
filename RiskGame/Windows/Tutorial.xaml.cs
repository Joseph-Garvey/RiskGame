using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RiskGame.Windows
{
    /// <summary>
    /// Interaction logic for Tutorial.xaml
    /// </summary>
    public partial class Tutorial : Window
    {
        private Window lastwindow;
        public Tutorial(Window _sender)
        {
            InitializeComponent();
            lastwindow = _sender;
        }
        // not needed anymore
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RestoreFocus();
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            RestoreFocus();
        }
        private void RestoreFocus()
        {
            try
            {
                App.Current.MainWindow = lastwindow;
            }
            catch (Exception) { }
        }
    }
}
