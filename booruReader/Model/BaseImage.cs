using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace booruReader.Model
{
    class BaseImage
    {
        #region Private Fields

        private string _previewURL;
        private string _fullPictureURL;
        private List<string> _tags;
        #endregion

        #region Public 
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
        #endregion

        public BaseImage(string previewURL, string fullPictureURL, List<string> tags = null)
        {
            FullPictureURL = fullPictureURL;
            PreviewURL = previewURL;

            if (tags != null && tags.Count > 0)
                _tags = new List<string>(tags);
            else
                _tags = null;
        }
    }
}
