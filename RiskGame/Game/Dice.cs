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
        public BackgroundWorker workerthread = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        private static readonly Uri[] sources = new Uri[]
        {
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice1.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice2.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice3.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice4.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice5.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/imgDice6.png"),
        };
        public int current;
        private Image dieimage;

        public Dice(Image _dieimage)
        {
            dieimage = _dieimage;
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            current = -1;
        }
        public void StartRoll()
        {
            workerthread.RunWorkerAsync();
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 6; i++)
            {
                if (workerthread.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    int progressPercentage = Convert.ToInt32(((double)i / 6) * 100);
                    current = ThreadSafeRandom.ThisThreadsRandom.Next(0, 5);
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                    Thread.Sleep(250);
                }
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            dieimage.Source = new BitmapImage(sources[current]);
        }
    }

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