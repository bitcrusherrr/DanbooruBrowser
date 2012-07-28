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
using System.Xml;
using System.Reflection;

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

            GlobalSettings.Instance.ProviderChanged = false;

            _selectFolderCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => SelectFolder()
            };
        }

        private List<BooruBoard> GetProviders()
        {
            List<BooruBoard> retList = new List<BooruBoard>();

            LoadXMLDataProviders(retList);

            //Temporarily hardcoding sources. This will need further improvement
            //retList.Add(new BooruBoard("https://yande.re/", "Yande.re", ProviderAccessType.XML));
            //retList.Add(new BooruBoard("http://konachan.com/", "Konachan.com", ProviderAccessType.XML));
            //retList.Add(new BooruBoard("http://danbooru.donmai.us/", "Danbooru.donmai.us", ProviderAccessType.XML));
            //retList.Add(new BooruBoard("http://booru.datazbytes.net/", "DataZbyteS.net", ProviderAccessType.XML));

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

        /// <summary>
        /// This function either writes out existing xml file and reads it ot reads already existing one to get list of websites.
        /// </summary>
        /// <param name="booruList">The list that needs to be populated with the *booru sites</param>
        private void LoadXMLDataProviders(List<BooruBoard> booruList)
        {
            booruList.Clear();
            XmlDocument whiteList = new XmlDocument();
            try
            {
                string externalFilePath = string.Format(@"{0}\ProviderList.xml", Environment.CurrentDirectory);
                if (File.Exists(externalFilePath))
                {
                    whiteList.Load(externalFilePath);
                }
                else
                {
                    Stream xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("booruReader.Settings_Screen.defaultBoorus.xml");
                    whiteList.Load(xmlStream);
                    whiteList.Save(externalFilePath);
                }

                XmlNode root = whiteList.DocumentElement;
                XmlNodeList nodelist = root.SelectNodes("/somethingBoorus/Booru");
                foreach (XmlNode node in nodelist)
                {
                    string booruName = node.SelectSingleNode("@name").Value.ToString();
                    string booruURL = node.SelectSingleNode("@url").Value.ToString();
                    string booruPoviderType = node.SelectSingleNode("@providerType").Value.ToString();//NOTE: We dont care yet

                    if (!string.IsNullOrEmpty(booruName) && !string.IsNullOrEmpty(booruURL) && !string.IsNullOrEmpty(booruPoviderType))
                        booruList.Add(new BooruBoard(booruURL, booruName, ProviderAccessType.XML));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "File Error");
            }
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
