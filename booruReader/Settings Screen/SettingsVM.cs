using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Model;
using booruReader.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace booruReader.Settings_Screen
{
    class SettingsVM : INotifyPropertyChanged
    {
        #region Private variables

        private bool _beenChanges = false;
        private bool _safeModeBrowsing;
        private ObservableCollection<BooruBoard> _providerList;
        private BooruBoard _currentSelectedBoard;
        private string _folderPath;
        #endregion
        #region Public Variables

        public bool SafeModeBrowsing
        {
            get { return _safeModeBrowsing; }
            set
            {
                _safeModeBrowsing = value;
                _beenChanges = true;
                GlobalSettings.Instance.IsSafeMode = value;
                RaisePropertyChanged("SafeModeBrowsing");
            }
        }

        public BooruBoard CurrentSelectedBoard
        {
            get { return _currentSelectedBoard; }
            set
            {
                _currentSelectedBoard = value;
                _beenChanges = true;

                //reset our current page as we changing provider
                GlobalSettings.Instance.CurrentPage = 1;
                GlobalSettings.Instance.CurrentBooru = value;
                RaisePropertyChanged("CurrentSelectedBoard");
            }
        }

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                if (ValidatePath(value))
                {
                    _folderPath = string.Format(value + "\\");
                    GlobalSettings.Instance.SavePath = string.Format(value + "\\");
                }

                RaisePropertyChanged("FolderPath");
            }
        }

        public ObservableCollection<BooruBoard> ProviderList
        {
            get { return _providerList; }
        }

        #endregion

        public SettingsVM()
        {
            //Reload all settings when oppening screen
            SafeModeBrowsing = GlobalSettings.Instance.IsSafeMode;
            _providerList = new ObservableCollection<BooruBoard>();

            foreach (BooruBoard board in GetProviders())
            {
                _providerList.Add(new BooruBoard(board));
            }

            CurrentSelectedBoard = GlobalSettings.Instance.CurrentBooru;

            //Reset been changes as this was just initialisation
            _beenChanges = false;
        }

        private List<BooruBoard> GetProviders()
        {
            List<BooruBoard> retList = new List<BooruBoard>();

            //Load all thwe stuff'
            //NOTE: Temp Code
            /*
             * Other sites
             * http://chan.sankakucomplex.com/post/index.xml
             * http://gelbooru.com/index.php?page=dapi&s=post&q=index&pid=1 this shit is all crazy bit &tags= for tags
             * CurrentBooruURL = "http://booru.datazbytes.net/post/index.xml";
             * CurrentBooruURL = "https://yande.re/post/index.xml";
             */

            //Temporarily hardcoding sources. This will need further improvement
            retList.Add(new BooruBoard("https://yande.re/post/index.xml", "Yande.re", ProviderAccessType.XML));
            retList.Add(new BooruBoard("http://booru.datazbytes.net/post/index.xml", "DataZbyteS.net", ProviderAccessType.XML));

            return retList;
        }

        
        public void Closing()
        {
            if(_beenChanges)
            {
                GlobalSettings.Instance.SaveSettings();
            }
        }

        private bool ValidatePath(string path)
        {
            bool retVal = false;

            if (Directory.Exists(path))
                retVal = true;
            else
            {
                Directory.CreateDirectory(path);
                if (Directory.Exists(path))
                    retVal = true;
            }

            return retVal;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

    }
}
