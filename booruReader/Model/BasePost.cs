using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using booruReader.Helpers;

namespace booruReader.Model
{
    public class BasePost : INotifyPropertyChanged  
    {
        #region Private Fields

        private string _previewURL;
        private string _fullPictureURL;
        private string _fileMD;
        private string _saveLocation;
        private string _tags;
        private bool _isSelected = false;
        private string _dimensions;
        private Visibility _progressBarVisibility;
        private int _downloadProgress = 0;
        private bool _isVisible = true;
        private string urlStore;
        private ImageCache _cache;
        private string _extension;
        #endregion

        #region Public 
        //This 2 shouldnt really be public, but ill deal with that later
        public int _width;
        public int _height;

        public PostRating ImageRating;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public int DownloadProgress
        {
            get
            {
                return _downloadProgress;
            }
            set
            {
                _downloadProgress = value;
                RaisePropertyChanged("DownloadProgress");
            }
        } 

        public string Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                RaisePropertyChanged("Tags");
            }
        }

        public string Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                RaisePropertyChanged("Dimensions");
            }
        }

        public string PreviewURL
        {
            get { return _previewURL; }
            set 
            { 
                _previewURL = value;
                RaisePropertyChanged("PreviewURL");
            }
        }

        public string FullPictureURL
        {
            get { return _fullPictureURL; }
            set { _fullPictureURL = value; }
        }

        public string FileMD
        {
            get { return _fileMD; }
            set { _fileMD = value; }
        }

        public Visibility ProgressBarVisible
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                _progressBarVisibility = value;
                RaisePropertyChanged("ProgressBarVisible");
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                if (value)
                {
                    if (PreviewURL != null && PreviewURL == "")
                    {
                        PreviewURL = urlStore;
                    }
                }
                else
                {
                    //Theres an issue if false called twice in a roll we will lose the original url
                    if (PreviewURL != null && PreviewURL == "")
                    {
                        urlStore = PreviewURL;
                        PreviewURL = @"";
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Constructor that SHOULD be used for images
        /// </summary>
        /// <param name="post"></param>
        public BasePost(BasePost post, bool isUIImage = false)
        {
            _cache = new ImageCache();

            ImageRating = post.ImageRating;
            FullPictureURL = post.FullPictureURL;

            if (isUIImage)
            {
                if (post.PreviewURL.ToLowerInvariant().Contains("jpg") || post.PreviewURL.ToLowerInvariant().Contains("jpeg"))
                    _extension = ".jpg";
                else if (post.PreviewURL.ToLowerInvariant().Contains("png"))
                    _extension = ".png";
                else if (post.PreviewURL.ToLowerInvariant().Contains("gif"))
                    _extension = ".gif";

                urlStore = PreviewURL = _cache.GetImage(post.FileMD, post.PreviewURL, LateFilePath, false);
            }
            else
                urlStore = PreviewURL = post.PreviewURL;

            FileMD = post.FileMD;
            Tags = post.Tags;
            _width = post._width;
            _height = post._height;
            IsVisible = true;

            if(!string.IsNullOrEmpty(Tags))
                TagFormatter(Tags);

            Dimensions = "Resolution " + _width + "x" + _height + "\n" + "Tags: " + "\n" + Tags;
        }

        public BasePost()
        {
            FullPictureURL = string.Empty;
            urlStore = PreviewURL = string.Empty;
            FileMD = string.Empty;
            IsVisible = true;
        }

        private void LateFilePath(object e, AsyncCompletedEventArgs args)
        {
            //Yeah, I know...
            if(_isVisible)
                urlStore = PreviewURL = _cache.GetImage(FileMD + _extension, null, LateFilePath, false);
            else
                urlStore = _cache.GetImage(FileMD + _extension, null, LateFilePath, false);
        }

        private void TagFormatter(string myString)
        {
            string[] words = myString.Split(' ');
            string newTags = string.Empty;
            int counter = 0;

            foreach (string tag in words)
            {
                if (counter == 6)
                {
                    newTags += tag + "\n";
                    counter = 0;
                }
                else
                {
                    newTags += tag + " ";
                }
                counter++;
            }

            Tags = newTags;
        }


        #region Image saving stuff
        /// <summary>
        /// This save method is invoked from preview screen.
        /// We already have full-size image downloaded so just copy it.
        /// </summary>
        public void SaveImage(string imageLcoation)
        {
            string extension = null;

            if (FullPictureURL.ToLowerInvariant().Contains("jpg") || FullPictureURL.ToLowerInvariant().Contains("jpeg"))
                extension = ".jpg";
            else if (FullPictureURL.ToLowerInvariant().Contains("png"))
                extension = ".png";
            else if (FullPictureURL.ToLowerInvariant().Contains("gif"))
                extension = ".gif";

            _saveLocation = string.Format(GlobalSettings.Instance.SavePath + FileMD + extension);

            if (extension != null && !File.Exists(_saveLocation))
            {
                File.Copy(imageLcoation, _saveLocation, false);

                ProgressBarVisible = Visibility.Visible;
                DownloadProgress = 100;
            }
        }

        public void SaveImage()
        {
            string extension;

            if (FullPictureURL.ToLowerInvariant().Contains("jpg") || FullPictureURL.ToLowerInvariant().Contains("jpeg"))
                extension = ".jpg";
            else if (FullPictureURL.ToLowerInvariant().Contains("png"))
                extension = ".png";
            else if (FullPictureURL.ToLowerInvariant().Contains("gif"))
                extension = ".gif";
            else
                extension = null;

            if (extension != null)
            {
                _saveLocation = string.Format(GlobalSettings.Instance.SavePath + FileMD + extension);

                if (!File.Exists(_saveLocation) && Directory.Exists(GlobalSettings.Instance.SavePath))
                {
                    ProgressBarVisible = Visibility.Visible;
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(FullPictureURL), _saveLocation);
                }
                else if (File.Exists(_saveLocation))
                {
                    //File already exists set the bar to visible and full
                    ProgressBarVisible = Visibility.Visible;
                    DownloadProgress = 100;
                }

            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            DownloadProgress = int.Parse(Math.Truncate(percentage).ToString());
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Image downloaded
        }
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
