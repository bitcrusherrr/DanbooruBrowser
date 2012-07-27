using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using booruReader.Helpers;

namespace booruReader.Model
{
    class BasePost : INotifyPropertyChanged  
    {
        #region Private Fields

        private string _previewURL;
        private string _fullPictureURL;
        private string _fileMD;
        private string _saveLocation;
        private List<string> _tags;
        private bool _isSelected = false;
        private BackgroundWorker imageSaver;
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
        #endregion

        public BasePost(string previewURL, string fullPictureURL, PostRating rating, string fileMD, List<string> tags = null)
        {
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
                    imageSaver = new BackgroundWorker();
                    imageSaver.DoWork += new DoWorkEventHandler(SaveTask);
                    imageSaver.RunWorkerAsync();
                }

            }
        }

        private void SaveTask(object sender, DoWorkEventArgs e)
        {
            byte[] imageBytes;
            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(FullPictureURL);
            WebResponse imageResponse = imageRequest.GetResponse();

            Stream responseStream = imageResponse.GetResponseStream();

            using (BinaryReader br = new BinaryReader(responseStream))
            {
                imageBytes = br.ReadBytes(50000000);
                br.Close();
            }
            responseStream.Close();
            imageResponse.Close();

            FileStream fs = new FileStream(_saveLocation, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(imageBytes);
                //new MetroMessagebox("Download finished.", string.Format("File saved at: " + _saveLocation)).Show();
            }
            finally
            {
                fs.Close();
                bw.Close();
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
