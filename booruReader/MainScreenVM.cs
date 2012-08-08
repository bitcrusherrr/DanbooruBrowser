using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using booruReader.Helpers;
using booruReader.Model;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using booruReader.Preview_Screen;

namespace booruReader
{
    public class MainScreenVM : INotifyPropertyChanged
    {
        #region Private variables
        private ObservableCollection<BasePost> _imageList;
        PostsFetcher _postFetcher;
        private string _tagsBox;
        private DelegateCommand _performFetchCommand;
        private DelegateCommand _performSelectedImagesDownloadCommand; 
        private DelegateCommand _clearSelectionCommand;
        private DelegateCommand _openSettingsCommand;
        private BackgroundWorker _imageLoader;
        private Visibility _progressBarVisibility;
        //private bool _tagsChanged = false;
        private List<BasePost> _threadList;
        private bool _settingsOpen;

        //This list is used to keep track of all open preview screens
        private List<PrviewScreenView> _previewList;

        //navigation variables
        private int CurrentPage; //Keep track of the last loaded pages
        #endregion

        #region Public variables
        //UI list for the images
        public ObservableCollection<BasePost> MainImageList
        {
            get { return _imageList; }
        }

        public string TagsBox
        {
            get { return _tagsBox; }
            set
            {
                _tagsBox = value;
                RaisePropertyChanged("TagsBox");
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

        //NOTE: Crappy hack to to enable loading of "offloaded" images
        public int LastHiddenIndex = 0;
        #endregion

        /// <summary>
        /// Main view model for the BooruReader window
        /// </summary>
        public MainScreenVM()
        {
            _imageList = new ObservableCollection<BasePost>();
            _previewList = new List<PrviewScreenView>();
            _postFetcher = new PostsFetcher();
            _threadList = new List<BasePost>();
            _imageLoader = new BackgroundWorker();
            _imageLoader.DoWork += BackgroundLoaderWork;
            _imageLoader.RunWorkerCompleted += ServerListLoadWorkerCompleted;
            _imageLoader.WorkerSupportsCancellation = true;
            //Ugly hack for settings vm
            GlobalSettings.Instance.MainScreenVM = this;
            SettingsOpen = false;

            ProgressBarVisibility = Visibility.Hidden;

            _performFetchCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => FetchImages()
            };

            _performSelectedImagesDownloadCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => SaveImages()
            };

            _clearSelectionCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => RemoveSelection()
            };

            _openSettingsCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => OpenSettings()
            };
        }

        public void TriggerImageLoading()
        {
            if (!_imageLoader.IsBusy && !_imageLoader.CancellationPending)
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

            if (_imageLoader.CancellationPending == true)
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
                new MetroMessagebox("Fetch Error", e.Error.Message).ShowDialog();
            }
            else
            {

                foreach (BasePost post in _threadList)
                {
                    _imageList.Add(new BasePost(post));
                }

                TriggerOffloading(_threadList.Count);
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
            if (GlobalSettings.Instance.ProviderChanged)
            {
                GlobalSettings.Instance.ProviderChanged = false;
                FetchImages();
            }
        }


        #region Image Offloading 
        private int cahcedLastHidden;
        public void TriggerOffloading(int imagesToHide)
        {
            //This is enough to fill 2x 1920x1200 screens with images
            if (GlobalSettings.Instance.PostsOffset > 200)
            {
                int offsetIndex = 0;

                if (cahcedLastHidden > LastHiddenIndex)
                {
                    Debug.WriteLine("Hiding: " + (cahcedLastHidden + imagesToHide) + " Images.");
                    for (int i = 0; i <= (cahcedLastHidden + imagesToHide); i++)
                    {
                        if (i < _imageList.Count)
                        {
                            offsetIndex = i;
                            _imageList[offsetIndex].IsVisible = false;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Hiding: " + imagesToHide + " Images.");
                    for (int i = 0; i <= imagesToHide; i++)
                    {
                        offsetIndex = i + LastHiddenIndex;
                        if (offsetIndex < _imageList.Count)
                        {
                            _imageList[offsetIndex].IsVisible = false;
                        }
                    }
                }

                Debug.WriteLine("Last hidden image index: " + offsetIndex);
                LastHiddenIndex = offsetIndex;
                cahcedLastHidden = offsetIndex;
            }
        }

        public void TriggerReloading()
        {
            int i = 0;
            int imagesLoaded = GlobalSettings.Instance.PostsOffset / CurrentPage;
            Debug.WriteLine("Reloading triggered for index: " + LastHiddenIndex + " For: " + imagesLoaded + " images");
            while (i <= imagesLoaded && LastHiddenIndex >= 0)
            {
                i++;
                _imageList[LastHiddenIndex].IsVisible = true;
                //Debug.WriteLine("Index: " + LastHiddenIndex + " Is Visible: " + _imageList[LastHiddenIndex].IsVisible);
                if (i <= imagesLoaded)
                    LastHiddenIndex--;
            }
            //Debug.WriteLine("Reloaded: " + imagesLastLoaded + " Last hidden index: " + LastHiddenIndex + " Visibility: " + _imageList[LastHiddenIndex].IsVisible);
        }

        #endregion

        #region Commands

        public DelegateCommand PerformFetchCommand { get { return _performFetchCommand; } }

        private void FetchImages()
        {
            ProgressBarVisibility = Visibility.Visible;
            //Clear image list 
            _imageList.Clear();
            RaisePropertyChanged("MainImageList");
            _imageList.Add(new BasePost());
            _imageList[0].IsSelected = true;
            _imageList.Clear();
            LastHiddenIndex = 0;
            cahcedLastHidden = 0;

            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
                CurrentPage = 0;
            else
                CurrentPage = 1;

            if (_imageLoader.IsBusy)
            {
                _imageLoader.CancelAsync();
            }
            else
            {
                _imageLoader.RunWorkerAsync();
            }
        }

        public DelegateCommand PerformSelectedImagesDownloadCommand { get { return _performSelectedImagesDownloadCommand; } }

        private void SaveImages()
        {
            if (string.IsNullOrEmpty(GlobalSettings.Instance.SavePath))
            {
                new MetroMessagebox("Error", "No save directory specified. \nPlease go to settings and select a folder.").ShowDialog();
            }
            else
            {
                foreach (BasePost post in MainImageList)
                {
                    if (post.IsSelected)
                    {
                        post.SaveImage();
                    }
                }
            }
        }

        public DelegateCommand ClearSelectionCommand { get { return _clearSelectionCommand; } }

        private void RemoveSelection()
        {
            foreach (BasePost post in MainImageList)
            {
                if (post.IsSelected)
                    post.IsSelected = false;
            }
        }

        public DelegateCommand OpenSettingsCommand { get { return _openSettingsCommand; } }

        private void OpenSettings()
        {
            if (!SettingsOpen)
                SettingsOpen = true;
            else
                SettingsOpen = false;
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
            var post = _imageList.FirstOrDefault(x => x.PreviewURL == previewURL);
            if (post != null)
            {
                PrviewScreenView preview = new PrviewScreenView(post);
                _previewList.Add(preview);
                preview.Show();
            }

            //cleanup preview images
            int index = 0;
            while (index < _previewList.Count)
            {
                if (_previewList[index].IsLoaded == false)
                {
                    _previewList.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }
        }

        #region Close Command
        /// <summary>
        /// Close command that handles saving of the notes for the databases.
        /// </summary>
        public void Closing()
        {
            CloseAllPreviews();
            GlobalSettings.Instance.SaveSettings();
        }
        #endregion

        #endregion

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
