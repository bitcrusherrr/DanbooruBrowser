using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Model;
using booruReader.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace booruReader.Settings_Screen
{
    class SettingsVM : INotifyPropertyChanged
    {
        #region Private variables
        private bool _safeModeBrowsing;
        private ObservableCollection<BooruBoard> _providerList;
        private BooruBoard _currentSelectedBoard;
        private string _folderPath;
        private DelegateCommand _selectFolderCommand;
        #endregion
        #region Public Variables

        public bool SafeModeBrowsing
        {
            get { return _safeModeBrowsing; }
            set
            {
                _safeModeBrowsing = value;
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
                    _folderPath = value;
                    GlobalSettings.Instance.SavePath = value ;
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

            FolderPath = GlobalSettings.Instance.SavePath;

            var index = _providerList.ElementAt(GlobalSettings.Instance.CurrentBooruIndex);
            if (index != null)
                CurrentSelectedBoard = index;

            _selectFolderCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => SelectFolder()
            };
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
            retList.Add(new BooruBoard("https://yande.re/", "Yande.re", ProviderAccessType.XML));
            retList.Add(new BooruBoard("http://konachan.com/", "Konachan.com", ProviderAccessType.XML));
            retList.Add(new BooruBoard("http://danbooru.donmai.us/", "Danbooru.donmai.us", ProviderAccessType.XML));
            retList.Add(new BooruBoard("http://booru.datazbytes.net/", "DataZbyteS.net", ProviderAccessType.XML));

            return retList;
        }

        
        public void Closing()
        {
            if (GlobalSettings.Instance.MainScreenVM != null)
                GlobalSettings.Instance.MainScreenVM.SettingsOpen = false;

            var index = ProviderList.IndexOf(GlobalSettings.Instance.CurrentBooru);
            if (index >= 0)
                GlobalSettings.Instance.CurrentBooruIndex = index;

            GlobalSettings.Instance.SaveSettings();
        }

        private bool ValidatePath(string path)
        {
            bool retVal = false;

            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                    retVal = true;
                else
                {
                    Directory.CreateDirectory(path);
                    if (Directory.Exists(path))
                        retVal = true;
                }
            }

            return retVal;
        }

        public DelegateCommand SelectFolderCommand { get { return _selectFolderCommand; } }

        private void SelectFolder()
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.ShowDialog();

            if (!string.IsNullOrEmpty(folderDialog.SelectedPath))
                FolderPath = folderDialog.SelectedPath + "\\";
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
