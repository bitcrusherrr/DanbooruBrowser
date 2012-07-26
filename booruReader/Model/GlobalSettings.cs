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
        public string CurrentBooruURL
        {
            get;
            set;
        }

        public bool IsSafeMode;

        public int CurrentPage;
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
            CurrentBooruURL = "https://yande.re/post/index.xml";
            IsSafeMode = false; 
            CurrentPage = 0;
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
    }
}
