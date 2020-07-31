using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskGame
{
    /// <summary>
    /// Class that manages music sources within the application.
    /// </summary>
    internal static class Music
    {
        /// <summary>
        /// File-paths of game's music tracks.
        /// </summary>
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
        /// <summary>
        /// Index of current track in list of sources.
        /// </summary>
        private static int musicIndex;
        /// <summary>
        /// Accessor for music index property.
        /// Limits music index within the range of indexes of the sources list.
        /// </summary>
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
