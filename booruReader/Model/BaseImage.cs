using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
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
        private List<string> _tags;
        private bool _isSelected = false;
        private Visibility _progressBarVisibility;
        private int _downloadProgress = 0;
        #endregion

        #region Public 
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

        public List<string> Tags
        {
            get { return _tags; }
        }

        public string PreviewURL
        {
            get { return _previewURL; }
            set { _previewURL = value; }
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
        #endregion

        public BasePost(string previewURL, string fullPictureURL, PostRating rating, string fileMD, List<string> tags = null)
        {
            ProgressBarVisible = Visibility.Hidden;
            FullPictureURL = fullPictureURL;
            PreviewURL = previewURL;
            ImageRating = rating;
            FileMD = fileMD;

            if (tags != null && tags.Count > 0)
                _tags = new List<string>(tags);
            else
                _tags = null;
        }

        public BasePost(BasePost post)
        {
            ImageRating = post.ImageRating;
            FullPictureURL = post.FullPictureURL;
            PreviewURL = post.PreviewURL;
            FileMD = post.FileMD;

            _tags = null;
        }

        public BasePost()
        {
            FullPictureURL = string.Empty;
            PreviewURL = string.Empty;
            FileMD = string.Empty;

            _tags = null;
        }

        #region Image saving stuff
        public void SaveImage()
        {
            string extension;

            if (FullPictureURL.ToLowerInvariant().Contains("jpg") || FullPictureURL.ToLowerInvariant().Contains("jpeg"))
                extension = ".jpg";
            else if (FullPictureURL.ToLowerInvariant().Contains("png"))
                extension = ".png";
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
