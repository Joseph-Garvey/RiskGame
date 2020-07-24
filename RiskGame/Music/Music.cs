using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame
{
    internal static class Music
    {
        public static List<Uri> sources = new List<Uri>
        {
            new Uri("pack://siteoforigin:,,,/Music/FallenSoldier.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/AirToTheThrone.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/EpicBattleSpeech.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/RideOfTheValkyries.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/SavingTheWorld.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/1812Overture.mp3"),
            new Uri("pack://siteoforigin:,,,/Music/TheRising.mp3"),
        };
        private static int musicIndex;
        public static int MusicIndex
        {
            get { return musicIndex; }
            set
            {
                if (value >= Music.sources.Count)
                {
                    musicIndex = 0;
                }
                else if (value < 0) { musicIndex = Music.sources.Count - 1; }
                else { musicIndex = value; }
            }
        }
    }
}
