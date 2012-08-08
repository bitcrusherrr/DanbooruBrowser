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
                    if (PreviewURL.Contains("emptyImage"))
                    {
                        PreviewURL = urlStore;
                    }
                }
                else
                {
                    //Theres an issue if false called twice in a roll we will lose the original url
                    if (!PreviewURL.Contains("emptyImage"))
                    {
                        urlStore = PreviewURL;
                        PreviewURL = @"Images\Toolbar\emptyImage.png";
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Depreciated test constructor
        /// </summary>
        /// <param name="previewURL"></param>
        /// <param name="fullPictureURL"></param>
        /// <param name="rating"></param>
        /// <param name="fileMD"></param>
        /// <param name="tags"></param>
        public BasePost(string previewURL, string fullPictureURL, PostRating rating, string fileMD, string tags = null)
        {
            ProgressBarVisible = Visibility.Hidden;
            FullPictureURL = fullPictureURL;
            urlStore = PreviewURL = previewURL;
            ImageRating = rating;
            FileMD = fileMD;
            Tags = tags;
            IsVisible = true;
        }

        /// <summary>
        /// Constructor that SHOULD be used for images
        /// </summary>
        /// <param name="post"></param>
        public BasePost(BasePost post)
        {
            ImageRating = post.ImageRating;
            FullPictureURL = post.FullPictureURL;
            urlStore = PreviewURL = post.PreviewURL;
            FileMD = post.FileMD;
            Tags = post.Tags;
            _width = post._width;
            _height = post._height;
            IsVisible = true;

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
