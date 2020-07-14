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
using RiskGame.CustomExceptions;
using System.IO;

namespace RiskGame.Windows
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            Initialise();
        }
        public ChangePassword(String username)
        {
            Initialise();
            txtName.Text = username;
        }
        private void Initialise()
        {
            InitializeComponent();
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged);
            this.DataContext = this;
        }
        private void ShowHidePassword(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            PasswordBox passwordBox = new PasswordBox();
            TextBox textBox = new TextBox();
            switch (checkBox.Name)
            {
                case "chkPass":
                    passwordBox = txtPass;
                    textBox = txtPassShow;
                    break;
                case "chkNewPass":
                    passwordBox = txtNewPass;
                    textBox = txtNewPassShow;
                    break;
                case "chkNewPassConf":
                    passwordBox = txtNewPassConf;
                    textBox = txtNewPassConfShow;
                    break;
            }
            if (checkBox.IsChecked == true)
            {
                textBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Visible;
            }
            else
            {
                passwordBox.Password = textBox.Text;
                passwordBox.Visibility = Visibility.Visible;
                textBox.Visibility = Visibility.Collapsed;

            }
        }
        private void RegisterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) { ChangeUserPassword(sender, e); }
        }
        private void ClearPwdText(object sender, KeyboardFocusChangedEventArgs e)
        {
            PasswordBox P = (PasswordBox)sender;
            P.Password = "";
        }
        private void ChangeUserPassword(object sender, RoutedEventArgs e)
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed;
                lblSuccess.Visibility = Visibility.Collapsed;
                if (txtNewPass.Password == txtNewPassConf.Password)
                {
                    Human.Validation(txtNewPass.Password);
                    Human player = Human.SignIn(txtName.Text, txtPass.Password);
                    Human.Update(player, txtNewPass.Password);
                    DispSuccessMsg("Your password has been changed.");
                }
                else { DispErrorMsg("The password(s) do not match"); }
            }
            catch (IOException) { DispErrorMsg("An error reading or writing from the file has occurred. Please try again or delete the Usersaves.txt file in the game directory."); }
            catch (AccountCreationException k) { DispErrorMsg(k.error); }
            catch (AccountNotFoundException) { DispErrorMsg("Your account was not found. Please check your password."); }
            catch(LoginException k) { DispErrorMsg(k.Message); }
            catch (Exception) { DispErrorMsg("An unknown error has occurred."); }
        }
        private void DispErrorMsg(String message)
        {
            // Shows error box with message //
            txtError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
        private void DispSuccessMsg(String message)
        {
            // Shows message on successful registration //
            txtSuccess.Text = message;
            lblSuccess.Visibility = Visibility.Visible;
        }
    }
}
