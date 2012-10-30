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
using System.Net;
using System.Windows;
using System.Xml.Serialization;

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
        private bool _enableEditing;

        private DelegateCommand _addBooruCommand;
        private DelegateCommand _removeBooruCommand;
        private DelegateCommand _finalizeBooruCommand;
        private DelegateCommand _cancelBooruCommand;
        private Visibility _doShowNewBooru;
        private string _sizeString;
        private Visibility _doShowProgressBar;
        bool _isValidBooru;
        bool _isBackArrowEnabled;
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
                    GlobalSettings.Instance.SavePath = value;
                }

                RaisePropertyChanged("FolderPath");
            }
        }

        public string VersionInfo
        {
            get { return "v"+Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public ObservableCollection<BooruBoard> ProviderList
        {
            get { return _providerList; }
        }

        public bool EnableEditing
        {
            get { return _enableEditing; }
            set
            {
                _enableEditing = value;
                RaisePropertyChanged("EnableEditing");
            }
        }

        public Visibility DoShowNewBooru
        {
            get { return _doShowNewBooru; }
            set
            {
                _doShowNewBooru = value;
                RaisePropertyChanged("DoShowNewBooru");
            }
        }

        public bool DoUseHumanReadableNames
        {
            get { return GlobalSettings.Instance.DoUseHumanReadableNames; }
            set
            {
                GlobalSettings.Instance.DoUseHumanReadableNames = value;
                RaisePropertyChanged("SafeModeBrowsing");
            }
        }

        public long ImageChacheSize
        {
            get { return GlobalSettings.Instance.CacheSizeMb; }
            set
            {
                GlobalSettings.Instance.CacheSizeMb = value;
                RaisePropertyChanged("ImageChacheSize");
            }
        }

        public string SizeString
        {
            get { return _sizeString; }
            set
            {
                _sizeString = value;
                RaisePropertyChanged("SizeString");
            }
        }

        public Visibility DoShowProgressBar
        {
            get { return _doShowProgressBar; }
            set
            {
                _doShowProgressBar = value;
                RaisePropertyChanged("DoShowProgressBar");
            }
        }

        public bool IsBackArrowEnabled
        {
            get { return _isBackArrowEnabled; }
            set
            {
                _isBackArrowEnabled = value;
                RaisePropertyChanged("IsBackArrowEnabled");
            }
        }

        //public string NewBooruName;
        //public string NewBooruURL;
        #endregion

        public SettingsVM()
        {
            //Reload all settings when oppening screen
            SafeModeBrowsing = GlobalSettings.Instance.IsSafeMode;
            _providerList = new ObservableCollection<BooruBoard>();
            EnableEditing = false;
            DoShowNewBooru = Visibility.Collapsed;
            DoShowProgressBar = Visibility.Hidden;

            foreach (BooruBoard board in GetProviders())
            {
                _providerList.Add(new BooruBoard(board));
            }

            FolderPath = GlobalSettings.Instance.SavePath;

            var index = _providerList.ElementAt(GlobalSettings.Instance.CurrentBooruIndex);
            if (index != null)
                CurrentSelectedBoard = index;

            GlobalSettings.Instance.ProviderChanged = false;

            #region Delegates

            _selectFolderCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => SelectFolder()
            };

            _addBooruCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => AddBooru()
            };

            _removeBooruCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => RemoveBooru()
            };

            _finalizeBooruCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => FinalizeBooru()
            };

            _cancelBooruCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => CancelBooru()
            };

            #endregion
            SizeString = GlobalSettings.Instance.CacheSizeMb.ToString();

            IsBackArrowEnabled = true;
        }

        private List<BooruBoard> GetProviders()
        {
            List<BooruBoard> retList = new List<BooruBoard>();

            LoadXMLDataProviders(retList);

            return retList;
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

        public DelegateCommand AddBooruCommand { get { return _addBooruCommand; } }

        private BooruBoard _previousBoard;

        private void AddBooru()
        {
            _previousBoard = CurrentSelectedBoard;
            CurrentSelectedBoard = new BooruBoard();
            EnableEditing = true;
            DoShowNewBooru = Visibility.Visible;
        }

        public DelegateCommand RemoveBooruCommand { get { return _removeBooruCommand; } }

        private void RemoveBooru()
        {
            _providerList.Remove(CurrentSelectedBoard);
        }

        public DelegateCommand FinalizeBooruCommand { get { return _finalizeBooruCommand; } }

        private void FinalizeBooru()
        {
            DoShowProgressBar = Visibility.Visible;
            _isValidBooru = false;
            EnableEditing = false;
            IsBackArrowEnabled = false;

            BackgroundWorker booruValidator = new BackgroundWorker();
            booruValidator.DoWork += booruValidator_DoWork;
            booruValidator.RunWorkerCompleted += booruValidator_RunWorkerCompleted;

            booruValidator.RunWorkerAsync();
        }

        void booruValidator_DoWork(object sender, DoWorkEventArgs e)
        {
            string result = IsValidBooru();
            if (result == null)
                _isValidBooru = true;
            else
                throw new Exception(result);
        }

        void booruValidator_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DoShowProgressBar = Visibility.Hidden;
            IsBackArrowEnabled = true;

            if (_isValidBooru)
            {
                _providerList.Add(CurrentSelectedBoard);
                CurrentSelectedBoard = CurrentSelectedBoard;
                EnableEditing = false;
                DoShowNewBooru = Visibility.Collapsed;
            }
            else
            {
                MetroMessagebox metroBox = new MetroMessagebox("Error", e.Error.Message);
                metroBox.Owner = System.Windows.Application.Current.MainWindow;
                metroBox.ShowDialog();
                EnableEditing = true;
            }
        }

        public DelegateCommand CancelBooruCommand { get { return _cancelBooruCommand; } }

        private void CancelBooru()
        {
            GlobalSettings.Instance.ProviderChanged = false;
            CurrentSelectedBoard = _previousBoard;
            EnableEditing = false;
            DoShowNewBooru = Visibility.Collapsed;
        }

        /// <summary>
        /// Checks that the board user is adding is valid
        /// </summary>
        private string IsValidBooru()
        {
            string retval = null;
            bool hadErrors = false;

            try
            {
                if (!string.IsNullOrEmpty(CurrentSelectedBoard.Name) && !string.IsNullOrEmpty(CurrentSelectedBoard.URL))
                {
                    CurrentSelectedBoard.URL = NormalizeURL(CurrentSelectedBoard.URL);

                    //Now we try and fetch a page until we get result... or an exception
                    PostsFetcher posts = new PostsFetcher(true);

                    try
                    {
                        CurrentSelectedBoard.ProviderType = ProviderAccessType.XML;
                        if (posts.GetImages(1).Count > 0)
                            hadErrors = false;
                    }
                    catch
                    {
                        hadErrors = true;
                    }

                    if (hadErrors)
                    {
                        try
                        {
                            CurrentSelectedBoard.ProviderType = ProviderAccessType.Gelbooru;
                            if (posts.GetImages(1).Count > 0)
                                hadErrors = false;
                        }
                        catch
                        {
                            hadErrors = true;
                        }
                    }

                    if (hadErrors)
                    {
                        try
                        {
                            CurrentSelectedBoard.ProviderType = ProviderAccessType.JSON;
                            if (posts.GetImages(1).Count > 0)
                                hadErrors = false;
                        }
                        catch
                        {
                            hadErrors = true;
                        }
                    }

                    if (hadErrors)
                        retval = "Invalid or unsupported booru.";
                }
                else
                {
                    hadErrors = true;
                    retval = "Enter address and name.";
                }
            }
            catch
            {
                hadErrors = true;
                retval = "Invalid or unsupported booru.";
            }
            finally
            {
                if (!hadErrors)
                    retval = null;
            }

            return retval;
        }

        /// <summary>
        /// Checks if url starts with http and ends with a /
        /// If not adds them in and returns an updated string
        /// </summary>
        private string NormalizeURL(string url)
        {
            string retval = url;

            //Check for http
            if (!retval.Contains("http"))
            {
                retval = "http://" + retval;
            }

            //Check that we end with a /
            if (retval[retval.Length - 1] != '/')
            {
                retval = retval + "/";
            }

            return retval;
        }

        /// <summary>
        /// This function either writes out existing xml file and reads it ot reads already existing one to get list of websites.
        /// </summary>
        /// <param name="booruList">The list that needs to be populated with the *booru sites</param>
        private void LoadXMLDataProviders(List<BooruBoard> booruList)
        {
            booruList.Clear();
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                path = Path.Combine(path, "BooruReader") + @"\Providers.xml";
                TextReader textReader;

                if (File.Exists(path))
                    textReader = new StreamReader(path);
                else
                {
                    Stream xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("booruReader.Settings_Screen.defaultBoorus.xml");
                    textReader = new StreamReader(xmlStream);
                }

                XmlSerializer deserializer = new XmlSerializer(_providerList.GetType());
                _providerList = (ObservableCollection<BooruBoard>)deserializer.Deserialize(textReader);
                textReader.Close();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "File Error");
            }
        }

        public void Closing()
        {
            if (DoShowNewBooru == Visibility.Visible)
            {
                CancelBooru();
            }
            else
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                path = Path.Combine(path, "BooruReader");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                TextWriter textWriter = new StreamWriter(path + @"\Providers.xml");

                XmlSerializer x = new XmlSerializer(_providerList.GetType());
                x.Serialize(textWriter, _providerList);
                textWriter.Close();

                if (GlobalSettings.Instance.MainScreenVM != null)
                    GlobalSettings.Instance.MainScreenVM.SettingsOpen = false;

                var index = ProviderList.IndexOf(GlobalSettings.Instance.CurrentBooru);
                if (index >= 0)
                    GlobalSettings.Instance.CurrentBooruIndex = index;
            }
            GlobalSettings.Instance.SaveSettings();
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
