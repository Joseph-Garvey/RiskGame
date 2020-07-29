using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace RiskGame.Game
{
    public abstract class Dice
    {
        /// <summary>
        /// Base class for all dice in game.
        /// Contains multithreading code for animating dice and generating the random number.
        /// </summary>
        #region Variables and Properties
        public BackgroundWorker workerthread = new BackgroundWorker() // The thread that generates random numbers and animates the die image.
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        private static readonly Uri[] sources = new Uri[] // List of file paths for the dice images.
        {
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice1.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice2.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice3.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice4.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice5.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice6.png"),
        };
        public int current; // Stores the current number the class has generated
        private Image dieimage; // Reference to the dice's image UI Element on-screen.
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor that instantiates the object, sets the image and thread events.
        /// </summary>
        /// <param name="_dieimage"></param>
        public Dice(Image _dieimage)
        {
            dieimage = _dieimage;
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            current = -1;
        }
        #endregion

        #region Methods
        public void StartRoll() // Method starts the worker thread animation.
        {
            workerthread.RunWorkerAsync();
        }
        #endregion

        #region Events
        private void Worker_DoWork(object sender, DoWorkEventArgs e) // Event called when the worker thread begins
        {
            for (int i = 0; i < 6; i++) // Repeat 6 times
            {
                if (workerthread.CancellationPending == true) // If the thread is pending cancellation, cancel the thread.
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    int progressPercentage = Convert.ToInt32(((double)i / 6) * 100); // Progress percentage calculation
                    current = ThreadSafeRandom.ThisThreadsRandom.Next(0, 5); // Generate a random number in a thread-safe fashion between 0 and 5
                    (sender as BackgroundWorker).ReportProgress(progressPercentage); // Update the UI
                    Thread.Sleep(250); // Wait .25 seconds before repeating again.
                }
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) // Event called to update the UI
        {
            dieimage.Source = new BitmapImage(sources[current]); // Set the die image to match the current random number
        }
        #endregion
    }

    // Derived classes used to distinguish between player and enemy dice.
    public class PlayerDice : Dice
    {
        public PlayerDice(Image _dieimage) : base(_dieimage)
        {
        }
    }
    public class EnemyDice : Dice
    {
        public EnemyDice(Image _dieimage) : base(_dieimage)
        {
        }
    }
}