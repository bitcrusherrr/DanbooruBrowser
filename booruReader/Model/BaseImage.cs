using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Helpers;

namespace booruReader.Model
{
    class BasePost
    {
        #region Private Fields

        private string _previewURL;
        private string _fullPictureURL;
        private List<string> _tags;
        #endregion

        #region Public 
        public PostRating ImageRating;

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

        public BasePost(string previewURL, string fullPictureURL, PostRating rating,List<string> tags = null)
        {
            FullPictureURL = fullPictureURL;
            PreviewURL = previewURL;
            ImageRating = rating;

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
            _tags = null;
        }

        public BasePost()
        {
            FullPictureURL = string.Empty;
            PreviewURL = string.Empty;
            _tags = null;
        }
    }
}
