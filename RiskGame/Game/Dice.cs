using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Threading.Tasks;


namespace RiskGame.Game
{
    public class Dice
    {
        private BackgroundWorker workerthread = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        public static List<Uri> sources = new List<Uri>
        {
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_1.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_2.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_3.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_4.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_5.png"),
            new Uri("pack://siteoforigin:,,,/Images/Dice/dice_6.png"),
        };
        public int current;
        Random rng = new Random();

        public Dice()
        {
            workerthread.DoWork += Worker_DoWork;
            workerthread.ProgressChanged += Worker_ProgressChanged;
            workerthread.RunWorkerCompleted += Worker_RunWorkerCompleted;
            current = 6;
        }
        private void StartRoll()
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
                    current = rng.Next(0, 5);
                    (sender as BackgroundWorker).ReportProgress(progressPercentage);
                    Thread.Sleep(250);
                }
            }
        }
        void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // on completed
        }
    }
}
