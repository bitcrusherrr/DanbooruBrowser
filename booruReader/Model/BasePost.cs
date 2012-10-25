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
        private WebClient _downloadClient;
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

        public string FileMD;

        public string PostId;

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
                    if (PreviewURL == null || PreviewURL == "")
                    {
                        PreviewURL = urlStore;
                    }
                }
                else
                {
                    //There's an issue if false called twice in a roll we will lose the original url
                    if (PreviewURL != null && PreviewURL != "")
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
            PostId = post.PostId;
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

        //Note:rewrite this to use Enviroment.LineBreak
        private void TagFormatter(string myString)
        {
            string[] words = myString.Split(' ');
            string newTags = string.Empty;
            int counter = 0;

            foreach (string tag in words)
            {
                if (counter == 6)
                {
                    if (tag.Count() > 0)
                        newTags += tag + " \n";

                    counter = 0;
                }
                else
                {
                    if (tag.Count() > 0)
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

            if (GlobalSettings.Instance.DoUseHumanReadableNames)
                _saveLocation = string.Format(GlobalSettings.Instance.SavePath + GetHumanFilename(extension));
            else
                _saveLocation = string.Format(GlobalSettings.Instance.SavePath + FileMD + extension);

            if (extension != null && !File.Exists(_saveLocation))
            {
                File.Copy(imageLcoation, _saveLocation, false);

                ProgressBarVisible = Visibility.Visible;
                DownloadProgress = 100;
            }
        }

        /// <summary>
        /// This call will try to save the image
        /// </summary>
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
                if(GlobalSettings.Instance.DoUseHumanReadableNames)
                    _saveLocation = string.Format(GlobalSettings.Instance.SavePath + GetHumanFilename(extension));
                else
                    _saveLocation = string.Format(GlobalSettings.Instance.SavePath + FileMD + extension);

                if (!File.Exists(_saveLocation) && Directory.Exists(GlobalSettings.Instance.SavePath))
                {
                    ProgressBarVisible = Visibility.Visible;
                    _downloadClient = new WebClient();
                    _downloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    _downloadClient.DownloadFileAsync(new Uri(FullPictureURL), _saveLocation);
                }
                else if (File.Exists(_saveLocation) && _downloadClient == null)
                {
                    //File already exists set the bar to visible and full
                    ProgressBarVisible = Visibility.Visible;
                    DownloadProgress = 100;
                }
            }
        }

        /// <summary>
        /// Returns human readable filename in format of: booru postid tag1 tag2... .extension
        /// Function will try adding as many tags as possible until we hit the 260 char limit for filename + folder path.
        /// Refer to http://msdn.microsoft.com/en-us/library/ee681827(VS.85).aspx#limits for the filename size limit.
        /// </summary>
        private string GetHumanFilename(string extension)
        {
            string filename;

            filename = GlobalSettings.Instance.CurrentBooru.Name + " " +  PostId;

            //Check if we have tags
            if (!string.IsNullOrEmpty(Tags))
            {
                string[] tags = Tags.Split(' ');

                //Try to append as many tags as we can within the filename size limit
                foreach (string tag in tags)
                {
                    if ((tag.Count() > 0) && (filename.Count() + tag.Count() + extension.Count() + GlobalSettings.Instance.SavePath.Count()) < 260)
                    {
                            filename += " " + tag;
                    }
                    //Otherwise we hit the limit and might as well dump out
                    else if (tag.Count() > 0)
                        break;
                }
            }

            //Remove all illegal chars from filename
            filename = Regex.Replace(filename, @"[^\w\.@-_& ]", "", RegexOptions.None);

            //Finally add extension
            filename += extension;

            return filename;
        }

        /// <summary>
        /// Progress callback for updating the image download progressbar
        /// </summary>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            DownloadProgress = int.Parse(Math.Truncate(percentage).ToString());
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
