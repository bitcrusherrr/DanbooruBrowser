using booruReader.Properties;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace booruReader.Model
{
    [Serializable]
    public class GlobalSettings
    {
        private static GlobalSettings instance;
        private BooruBoard _currentBooru;

        #region Global Variables
        [XmlElement("SavePath")]
        public string SavePath;

        [XmlElement("IsSafeMode")]
        public bool IsSafeMode;

        [XmlElement("CurrentBooruIndex")]
        public int CurrentBooruIndex;

        //Keep the screen sizes
        [XmlElement("PreviewScreenWidth")]
        public double PreviewScreenWidth;
        [XmlElement("PreviewScreenHeight")]
        public double PreviewScreenHeight;

        [XmlElement("MainScreenWidth")]
        public double MainScreenWidth;
        [XmlElement("MainScreenHeight")]
        public double MainScreenHeight;

        [XmlElement("DoUseHumanReadableNames")]
        public bool DoUseHumanReadableNames;

        [XmlElement("CacheSizeMb")]
        public long CacheSizeMb;

        public string ApplicationFolder;

        //This 2 are related. One keeps track of posts loaded and another one keeps track of post offset.
        public int TotalPosts;
        public int PostsOffset;

        public MainScreenVM MainScreenVM = null;

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

        /// <summary>
        /// This function writes out current values 
        /// </summary>
        public void SaveSettings()
        {
            TextWriter textWriter = new StreamWriter(ApplicationFolder + @"\settings.xml");

            XmlSerializer x = new XmlSerializer(this.GetType());
            x.Serialize(textWriter, this);
            textWriter.Close();
        }

        /// <summary>
        /// This function reads initial settings on the class initialisation
        /// </summary>
        private void LoadSettings()
        {
            ApplicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            ApplicationFolder = Path.Combine(ApplicationFolder, "BooruReader");

            string path = ApplicationFolder + @"\settings.xml";

            //Try loading xml file if one exists
            if (File.Exists(path))
            {
                XmlTextReader reader = new XmlTextReader(path);
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);

                SavePath = xml.SelectSingleNode("/GlobalSettings/SavePath").InnerText;
                IsSafeMode = xml.SelectSingleNode("/GlobalSettings/IsSafeMode").InnerText.Contains("true");
                int.TryParse(xml.SelectSingleNode("/GlobalSettings/CurrentBooruIndex").InnerText, out CurrentBooruIndex);
                double.TryParse(xml.SelectSingleNode("/GlobalSettings/PreviewScreenWidth").InnerText, out PreviewScreenWidth);
                double.TryParse(xml.SelectSingleNode("/GlobalSettings/PreviewScreenHeight").InnerText, out PreviewScreenHeight);
                double.TryParse(xml.SelectSingleNode("/GlobalSettings/MainScreenWidth").InnerText, out MainScreenWidth);
                double.TryParse(xml.SelectSingleNode("/GlobalSettings/MainScreenHeight").InnerText, out MainScreenHeight);
                DoUseHumanReadableNames = xml.SelectSingleNode("/GlobalSettings/DoUseHumanReadableNames").InnerText.Contains("true");
                long.TryParse(xml.SelectSingleNode("/GlobalSettings/CacheSizeMb").InnerText, out CacheSizeMb);
            }
            //otherwise load default settings provided
            else
            {
                IsSafeMode = Settings.Default.SafeMode;
                SavePath = Settings.Default.SaveDirectory;
                CurrentBooruIndex = Settings.Default.LastUsedBoardIndex;
                PreviewScreenHeight = Settings.Default.PreviewHeight;
                PreviewScreenWidth = Settings.Default.PreviewWidth;
                MainScreenHeight = Settings.Default.MainHeight;
                MainScreenWidth = Settings.Default.MainWidth;
                DoUseHumanReadableNames = Settings.Default.DoUseHumanReadableNames;
                CacheSizeMb = Settings.Default.CacheSizeMb;
            }
        }
    }
}
