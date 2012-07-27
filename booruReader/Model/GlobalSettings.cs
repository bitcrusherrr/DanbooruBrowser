using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Helpers;

namespace booruReader.Model
{
    public class GlobalSettings
    {
        private static GlobalSettings instance;

        #region Global Variables
        public BooruBoard CurrentBooru
        {
            get;
            set;
        }

        public string SavePath = @"C:\TestFolder\";

        public bool IsSafeMode;

        public int CurrentPage;

        public int TotalPosts;

        public int PostsOffset;
        #endregion

        private GlobalSettings() 
        {
            //Load all thwe stuff'
            //NOTE: Temp Code
            /*
             * Other sites
             * http://chan.sankakucomplex.com/post/index.xml
             * http://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=1 this shit is all crazy bit &tags= for tags
             */

            //CurrentBooruURL = "http://booru.datazbytes.net/post/index.xml";
            //This should load default board or whatever was used last
            CurrentBooru = new BooruBoard("https://yande.re/post/index.xml", "Yandere", ProviderAccessType.XML);
            //CurrentBooru = new BooruBoard("http://booru.datazbytes.net/post/index.xml", "Yandere", ProviderAccessType.XML);
            IsSafeMode = false; 
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

        }

        /// <summary>
        /// This function reads initial settings on the class initilisation
        /// </summary>
        private void LoadSettings()
        {

        }
    }
}
