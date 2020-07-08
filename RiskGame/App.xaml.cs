using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RiskGame.Windows;

namespace RiskGame
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Start Splash Screen and Show
            SplashScreenWindow splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();
            // Show Progress Bar on a new thread
            Task.Factory.StartNew(() =>
            {
                //we need to do the work in batches so that we can report progress
                for (int i = 1; i <= 110; i++)
                {
                    //simulate a part of work being done
                    System.Threading.Thread.Sleep(10);
                    splashScreen.Dispatcher.Invoke(() => splashScreen.Progress = i);
                }
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the Welcome Screen window, set as main app window.
                    //and close the splash screen
                    WelcomeScreen mainWindow = new WelcomeScreen();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    splashScreen.Close();
                });
            });
        }

        public void Tutorial_Window(object sender, RoutedEventArgs e)
        {
            Tutorial tutorial = new Tutorial();
            App.Current.MainWindow = tutorial;
            tutorial.Show();
        }
    }
}
