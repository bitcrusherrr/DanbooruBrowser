using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Helpers;
using booruReader.Properties;

namespace booruReader.Model
{
    public class GlobalSettings
    {
        private static GlobalSettings instance;
        private BooruBoard _currentBooru;

        #region Global Variables
        public BooruBoard CurrentBooru
        {
            get { return _currentBooru; }
            set
            {
                _currentBooru = value;
                ProviderChanged = true;
            }
        }

        public bool ProviderChanged = false;

        public string SavePath;

        public bool IsSafeMode;

        public int CurrentBooruIndex;

        public int CurrentPage;

        public int TotalPosts;

        public int PostsOffset;

        public MainScreenVM MainScreenVM = null;
        #endregion

        private GlobalSettings() 
        {
            LoadSettings();
            CurrentPage = 1;
            TotalPosts = 0;
            PostsOffset = 0;
        }

        public static GlobalSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalSettings();
                }
                return instance;
            }
        }

        /// <summary>
        /// This function writes out current values 
        /// </summary>
        public void SaveSettings()
        {
            Settings.Default.SafeMode = IsSafeMode;
            Settings.Default.SaveDirectory = SavePath;
            Settings.Default.LastUsedBoardIndex = CurrentBooruIndex;
            Settings.Default.Save();
        }

        /// <summary>
        /// This function reads initial settings on the class initilisation
        /// </summary>
        private void LoadSettings()
        {
            IsSafeMode = Settings.Default.SafeMode;
            SavePath = Settings.Default.SaveDirectory;
            CurrentBooruIndex = Settings.Default.LastUsedBoardIndex;
        }
    }
}
