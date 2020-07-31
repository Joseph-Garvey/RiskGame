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
        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChangePassword()
        {
            Initialise(); // Calls method containing code shared between constructors
        }
        /// <summary>
        /// Takes username from previous window and auto-fills the username field.
        /// </summary>
        /// <param name="username"></param>
        public ChangePassword(String username)
        {
            Initialise(); // Calls method containing code shared between constructors
            txtName.Text = username; // Set the username field
        }
        #endregion
        #region Methods
        /// <summary>
        /// Contains code shared between constructors.
        /// Sets up UI, events and data-binding.
        /// </summary>
        private void Initialise()
        {
            InitializeComponent(); // UI Setup
            this.StateChanged += new EventHandler(((App)Application.Current).Window_StateChanged); // WIndow state changed event added
            this.DataContext = this; // Data-binding setup
        }
        /// <summary>
        /// Outputs error messages to the user
        /// </summary>
        /// <param name="Message">The output error message.</param>
        private void DispErrorMsg(String message)
        {
            txtError.Text = message; // Set text
            lblError.Visibility = Visibility.Visible; // Make visible
        }
        /// <summary>
        /// Outputs success messages to the user
        /// </summary>
        /// <param name="message">The output message</param>
        private void DispSuccessMsg(String message)
        {
            txtSuccess.Text = message; // Set text
            lblSuccess.Visibility = Visibility.Visible; // Make visible
        }
        /// <summary>
        /// Hides text outputs
        /// </summary>
        private void ClearMsg()
        {
            lblError.Visibility = Visibility.Collapsed; // Hide error output
            lblSuccess.Visibility = Visibility.Collapsed; // Hide success output
        }
        #endregion
        #region Events
        /// <summary>
        /// Toggles password text visibility.
        /// </summary>
        private void ShowHidePassword(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender; // The clicked checkbox
            PasswordBox passwordBox = new PasswordBox(); // The adjacent password-box
            TextBox textBox = new TextBox(); // The adjacent text-box
            switch (checkBox.Name)
            {
                // Match the clicked checkbox to its adjacent text-box and password-box
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
            if (checkBox.IsChecked == true) // If the checkbox is "Show"
            {
                textBox.Text = passwordBox.Password; // Set text
                passwordBox.Visibility = Visibility.Collapsed; // Hide password-box
                textBox.Visibility = Visibility.Visible; // Show text-box
            }
            else
            {
                passwordBox.Password = textBox.Text; // Set text
                passwordBox.Visibility = Visibility.Visible; // Hide password-box
                textBox.Visibility = Visibility.Collapsed; // Show text-box
            }
        }
        /// <summary>
        /// KeyDown Event.
        /// If enter is pressed call the changeuserpassword event.
        /// </summary>
        /// <param name="sender">The field typed in</param>
        /// <param name="e">The key pressed</param>
        private void ChangePasswordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) { ChangeUserPassword(sender, e); }
        }
        /// <summary>
        /// Verifies player details and changes password.
        /// </summary>
        private void ChangeUserPassword(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMsg(); // Clear output
                if (txtName.Text == null || txtName.Text == "" ||
                txtPass.Password == null || txtPass.Password == "" ||
                txtNewPass.Password == null || txtNewPass.Password == "" ||
                txtNewPassConf.Password == null || txtNewPassConf.Password == "")
                    { throw new ArgumentNullException(); } // If any fields are empty create Argument Null Exception
                if (txtNewPass.Password == txtNewPassConf.Password) // If passwords match
                {
                    Human.Validation(txtNewPass.Password); // Validate the new password against rules
                    Human player = Human.SignIn(txtName.Text, txtPass.Password); // Check user login is correct
                    Human.Update(player, txtNewPass.Password); // Update the password on file
                    DispSuccessMsg("Your password has been changed."); // Alert user
                }
                else { DispErrorMsg("The password(s) do not match"); } // Alert user
            }
            // Alert user //
            catch (ArgumentNullException) { DispErrorMsg("Please provide an input for every field."); }
            catch (IOException) { DispErrorMsg("An error reading or writing from the file has occurred. Please ensure you have created an account, try again, restart the application or delete the Usersaves.txt file in the game directory."); }
            catch (AccountCreationException k) { DispErrorMsg(k.error); }
            catch (AccountNotFoundException) { DispErrorMsg("Your account was not found. Please check your password."); }
            catch(LoginException k) { DispErrorMsg(k.Message); }
            catch (Exception) { DispErrorMsg("An unknown error has occurred."); }
        }
        /// <summary>
        /// TextChanged event.
        /// Sets password-box text equal to adjacent text-box text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender; // The text-box typed in
            PasswordBox passwordBox = new PasswordBox(); // The adjacent password-box
            switch (textBox.Name) // Match text-box to adjacent password-box
            {
                case "txtPassShow":
                    passwordBox = txtPass;
                    break;
                case "txtNewPassShow":
                    passwordBox = txtNewPass;
                    break;
                case "txtNewPassConfShow":
                    passwordBox = txtNewPassConf;
                    break;
            }
            passwordBox.Password = textBox.Text; // Set text
        }
        #endregion
    }
}
