using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

        //This 2 are related. One keeps track of posts loaded and another one keeps track of post offset.
        public int TotalPosts;
        public int PostsOffset;

        public MainScreenVM MainScreenVM = null;

        public bool CheckLatest = false;

        //Keep the screensizes
        public double PreviewScreenWidth { get; set; }
        public double PreviewScreenHeight { get; set; }

        public double MainScreenWidth { get; set; }
        public double MainScreenHeight { get; set; }

        public bool DoUseHumanReadableNames { get; set; }

        public long CacheSizeMb;
        #endregion

        private GlobalSettings()
        {
            LoadSettings();
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

        public void PerformVersionCheck()
        {
            if (CheckLatest)
            {
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                try
                {
                    byte[] imageBytes;
                    HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create("http://datazbytes.net/programms/version");
                    WebResponse imageResponse = imageRequest.GetResponse();

                    Stream responseStream = imageResponse.GetResponseStream();

                    using (BinaryReader br = new BinaryReader(responseStream))
                    {
                        imageBytes = br.ReadBytes(50000000);
                        br.Close();
                    }
                    responseStream.Close();
                    imageResponse.Close();

                    string text = System.Text.Encoding.Default.GetString(imageBytes);

                    if (string.Compare(currentVersion, text, true) != 0)
                    {
                        new MetroMessagebox("New version", "New version is available.").Show();
                    }
                }
                catch
                {
                    //Do nothing, we dont really care if we failed to check version.
                }
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
            Settings.Default.CheckForLatest = CheckLatest;
            Settings.Default.PreviewHeight = PreviewScreenHeight;
            Settings.Default.PreviewWidth = PreviewScreenWidth;
            Settings.Default.MainHeight = MainScreenHeight;
            Settings.Default.MainWidth = MainScreenWidth;
            Settings.Default.DoUseHumanReadableNames = DoUseHumanReadableNames;
            Settings.Default.CacheSizeMb = CacheSizeMb;
            Settings.Default.Save();
        }

        /// <summary>
        /// This function reads initial settings on the class initialisation
        /// </summary>
        private void LoadSettings()
        {
            IsSafeMode = Settings.Default.SafeMode;
            SavePath = Settings.Default.SaveDirectory;
            CurrentBooruIndex = Settings.Default.LastUsedBoardIndex;
            CheckLatest = Settings.Default.CheckForLatest;
            PreviewScreenHeight = Settings.Default.PreviewHeight;
            PreviewScreenWidth = Settings.Default.PreviewWidth;
            MainScreenHeight = Settings.Default.MainHeight;
            MainScreenWidth = Settings.Default.MainWidth;
            DoUseHumanReadableNames = Settings.Default.DoUseHumanReadableNames;
            CacheSizeMb = Settings.Default.CacheSizeMb;
        }
    }
}
