using booruReader.Helpers;
using booruReader.Model;
using booruReader.Model.NewDownloads;
using booruReader.Preview_Screen;
using dbz.UIComponents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Shell;

namespace booruReader
{
    public partial class MainScreenVM : BaseIObservable
    {
        #region Private variables
        private ObservableCollection<BasePost> _imageList; 
        private ObservableCollection<BasePost> _imageListCache;
        PostsFetcher _postFetcher;
        private string _tagsBox;
        private BackgroundWorker _imageLoader;
        private Visibility _progressBarVisibility;
        //private bool _tagsChanged = false;
        private List<BasePost> _threadList;
        private bool _settingsOpen;
        private bool _showedLastPageWarning;
        //This list is used to keep track of all open preview screens
        private List<PrviewScreenView> _previewList;

        //navigation variables
        private int CurrentPage; //Keep track of the last loaded pages

        private ImageCache _cache;
        private bool _isFavoritesMode;
        FavoriteHandler _favorites;
        #endregion

        #region Public variables

        public ObservableCollection<BasePost> DownloadList { get; set; }

        //UI list for the images
        public ObservableCollection<BasePost> MainImageList { get { return _imageList; } }

        public string TagsBox
        {
            get { return _tagsBox; }
            set
            {
                _tagsBox = value;
                OnPropertyChanged(() => TagsBox);
//                RaisePropertyChanged("TagsBox");
            }
        }

        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                RaisePropertyChanged("ProgressBarVisibility");
            }
        }

        public bool SettingsOpen
        {
            get { return _settingsOpen; }
            set
            {
                _settingsOpen = value;
                if(!value)
                    CheckForChangedSettings();
                RaisePropertyChanged("SettingsOpen");
            }
        }

        public bool IsFavoritesMode 
        {
            get 
            {
                return _isFavoritesMode;
            }
            set
            {
                _isFavoritesMode = value;

                if (_isFavoritesMode)
                {
                    _imageListCache = new ObservableCollection<BasePost>(_imageList);
                    _imageList = new ObservableCollection<BasePost>(_favorites.FetchFavorites(_tagsBox));
                }
                else if (_imageListCache != null)
                {
                    _imageList = new ObservableCollection<BasePost>(_imageListCache);
                    _imageListCache = null;
                }

                RaisePropertyChanged("MainImageList");
            }
        }
        #endregion

        /// <summary>
        /// Main view model for the BooruReader window
        /// </summary>
        public MainScreenVM() : base()
        {
            _imageList = new ObservableCollection<BasePost>();
            _previewList = new List<PrviewScreenView>();
            _postFetcher = new PostsFetcher();
            _threadList = new List<BasePost>();
            _imageLoader = new BackgroundWorker();
            _cache = new ImageCache();
            DownloadList = new ObservableCollection<BasePost>();
            DownloadList.CollectionChanged += DowloadList_CollectionChanged;
            _imageLoader.DoWork += BackgroundLoaderWork;
            _imageLoader.RunWorkerCompleted += ServerListLoadWorkerCompleted;
            _imageLoader.WorkerSupportsCancellation = true;
            _showedLastPageWarning = false;
            //Ugly hack for settings vm
            GlobalSettings.Instance.MainScreenVM = this;
            SettingsOpen = false;
            IsFavoritesMode = false;
            _favorites = new FavoriteHandler();

            InitialiseDelegates();

            ProgressBarVisibility = Visibility.Hidden;
        }

        public void TriggerImageLoading()
        {
            if (!IsFavoritesMode && !_imageLoader.IsBusy && !_imageLoader.CancellationPending)
            {
                ProgressBarVisibility = Visibility.Visible;
                _threadList.Clear();
                _imageLoader.RunWorkerAsync();
            }
        }

        /// <summary>
        /// ImageLoader worker todo work. 
        /// Currently this fetches images for the specified tag and page
        /// </summary>
        private void BackgroundLoaderWork(object sender, DoWorkEventArgs e)
        {
            foreach (BasePost post in _postFetcher.GetImages(CurrentPage, TagsBox))
            {
                _threadList.Add(new BasePost(post));
            }

            if (_imageLoader.CancellationPending)
            {
                _threadList.Clear();
            }
        }

        /// <summary>
        /// Event that gets fired when ImageLoader worker ends
        /// </summary>
        private void ServerListLoadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //Stop end of posts message appearing more then once
                if (e.Error.Message == "End of posts.")
                {
                    if(!_showedLastPageWarning)
                        new MetroMessagebox("Fetch Error", e.Error.Message).ShowDialog();

                    _showedLastPageWarning = true;
                }
                else
                    new MetroMessagebox("Fetch Error", e.Error.Message).ShowDialog();
            }
            else
            {
                foreach (BasePost post in _threadList)
                {
                    _imageList.Add(new BasePost(post, true));
                }

                _threadList.Clear();

                if (GlobalSettings.Instance.TotalPosts == 0)
                {
                    new MetroMessagebox("Fetch Error", "No posts for this tag/tags.").ShowDialog();
                }
                else
                {
                    //Were done loading and still have images to load update the page index
                    CurrentPage++;
                }
            }
            ProgressBarVisibility = Visibility.Hidden;
        }

        private void CheckForChangedSettings()
        {
            if (!IsFavoritesMode && GlobalSettings.Instance.ProviderChanged)
            {
                GlobalSettings.Instance.ProviderChanged = false;
                _showedLastPageWarning = false;
                FetchImages();
            }
        }
        #region Commands
        private void FetchImages()
        {
            if (!IsFavoritesMode)
            {
                ProgressBarVisibility = Visibility.Visible;
                //Clear image list 
                _imageList.Clear();
                RaisePropertyChanged("MainImageList");
                _imageList.Add(new BasePost());
                _imageList[0].IsSelected = true;
                _imageList.Clear();
                _showedLastPageWarning = false;

                CurrentPage = GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru ? 0 : 1;

                if (_imageLoader.IsBusy)
                {
                    _imageLoader.CancelAsync();
                }
                else
                {
                    _imageLoader.RunWorkerAsync();
                }
            }
            else
                _imageList = new ObservableCollection<BasePost>(_favorites.FetchFavorites(_tagsBox));

            RaisePropertyChanged("MainImageList");
        }

        //NOTE: Hacky test code
        int _itemsDownloadingCount = 0;
        void post_DownloadCompleted(object sender, EventArgs e)
        {
            BasePost item = (sender as BasePost);
            if (item == null)
                return;

            item.DownloadCompleted -= post_DownloadCompleted;

            // KBR 20150220 Fix issue #2: if completed items aren't removed from the list, they'll be downloaded repeatedly.
            DownloadList.Remove(item);

            // KBR 20150220 NOTE: the above change probably confuses the taskbar status display. Need to fix the "hacky test code"?

            var taskbar = Application.Current.MainWindow.TaskbarItemInfo;
            if (taskbar != null)
            {
                _itemsDownloadingCount++;

                taskbar.ProgressState = TaskbarItemProgressState.Normal;
                taskbar.ProgressValue = ((double)_itemsDownloadingCount / (double)DownloadList.Count);

                if (_itemsDownloadingCount >= DownloadList.Count)
                    taskbar.ProgressState = TaskbarItemProgressState.None;
            }
        }

        void DowloadList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Check if an item has been removed, if so it was completed and we want to decrease the counter
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && _itemsDownloadingCount > 0)
                _itemsDownloadingCount--;
        }

        private void SaveImages()
        {
            if (string.IsNullOrEmpty(GlobalSettings.Instance.SavePath))
            {
                new MetroMessagebox("Error", "No save directory specified. \nPlease go to settings and select a folder.").ShowDialog();
                return;
            }

            var selected = MainImageList.Where(x => x.IsSelected);

            if (DownloadList.Count == 0)
                _itemsDownloadingCount = 0;

            foreach (BasePost post in selected)
            {
                if (post.IsSelected && DownloadList.IndexOf(post) == -1)
                {
                    post.DownloadCompleted += post_DownloadCompleted;
                    DownloadList.Add(post);
                }
            }

            // Fire the save for each file after all files have been added to the list.
            // Otherwise the completed event could be fired at incorrect times and cause progress bar to be off sometimes and/or jump around
            foreach (BasePost post in DownloadList)
            {
                post.SaveImage();
            }
        }

        private void RemoveSelection()
        {
            foreach (BasePost post in MainImageList)
            {
                if (post.IsSelected)
                    post.IsSelected = false;
            }
        }

        private void OpenSettings()
        {
            SettingsOpen = !SettingsOpen;
        }

        private void CloseAllPreviews()
        {
            foreach (PrviewScreenView preview in _previewList)
            {
                preview.Close();
            }

            _previewList.Clear();
        }

        internal void PreviewImage(string previewURL)
        {
            /*
             * Have to reformat the string from 
             * "file:///C:/Users/Username/AppData/Roaming/BooruReader/SmallCache/somefile.jpg"
             * to
             * "C:\\Users\\Username\\AppData\\Roaming\\BooruReader\\SmallCache\\somefile.jpg"
             * otherwise its not matching.
             */
            string filepath = previewURL.Replace("file:///", "");
            filepath = filepath.Replace(@"/", @"\");
            var post = _imageList.FirstOrDefault(x => x.PreviewURL == filepath);
            if (post != null)
            {
                PrviewScreenView preview = new PrviewScreenView(post, DownloadList);
                _previewList.Add(preview);
                preview.AddedImageToFavorites += preview_AddedImageToFavorites;
                preview.RemovedImageFromFavorites += preview_RemovedImageFromFavorites;
                preview.UserTagSelection += preview_UserTagSelection;
                preview.Show();
            }

            //cleanup preview images
            int index = 0;
            while (index < _previewList.Count)
            {
                if (_previewList[index].IsLoaded == false)
                {
                    _previewList[index].UserTagSelection -= preview_UserTagSelection;
                    _previewList[index].AddedImageToFavorites -= preview_AddedImageToFavorites;
                    _previewList[index].RemovedImageFromFavorites -= preview_RemovedImageFromFavorites;
                    _previewList.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        //This is a terrible hack... Untill I figure this one out...
        public event EventHandler SearchBoxChanged;

        private void preview_UserTagSelection(object sender, EventArgs e)
        {
            if (sender is string && (TagsBox == null || !TagsBox.Contains(sender as string)))
                TagsBox += " " + sender;

            if (SearchBoxChanged != null)
                SearchBoxChanged(null, null);

            RaisePropertyChanged("TagsBox");
        }

        void preview_RemovedImageFromFavorites(object sender, EventArgs e)
        {
            if(IsFavoritesMode)
                _imageList = new ObservableCollection<BasePost>(_favorites.FetchFavorites(_tagsBox));
            RaisePropertyChanged("MainImageList");
        }

        void preview_AddedImageToFavorites(object sender, EventArgs e)
        {
            if (IsFavoritesMode)
                _imageList = new ObservableCollection<BasePost>(_favorites.FetchFavorites(_tagsBox));
            RaisePropertyChanged("MainImageList");
        }

        #region Close Command
        /// <summary>
        /// Close command that handles saving of the notes for the databases.
        /// </summary>
        public void Closing()
        {
            CloseAllPreviews();
            GlobalSettings.Instance.SaveSettings();
            _cache.CleanCache();
            _favorites.CheckForRemovedFavorites();
        }

        #endregion

        #endregion

        /// <summary>
        /// Performs check on pending downloads and if any are in progress asks user if he really wants to exit.
        /// </summary>
        internal bool DownloadsPending()
        {
            bool waitForDownload = false;

            foreach (BasePost post in DownloadList)
            {
                if(post.DownloadProgress != 100)
                {
                    waitForDownload = true;
                    break;
                }
            }

            return waitForDownload;
        }
    }
}
