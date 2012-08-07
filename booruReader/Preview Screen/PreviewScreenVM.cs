using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using booruReader.Helpers;
using booruReader.Model;

namespace booruReader.Preview_Screen
{
    class PreviewScreenVM : INotifyPropertyChanged
    {
        private BasePost _post;
        private string _imageSource;
        private ObservableCollection<string> _taglist;
        private Visibility _showTaglist;

        public ObservableCollection<string> TagList { get { return _taglist; } }

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        public BasePost PreviewPost
        {
            get
            {
                return _post;
            }
            set
            {
                _post = value;
                RaisePropertyChanged("PreviewPost");
            }
        }

        public Visibility ShowTagList
        {
            get
            {
                return _showTaglist;
            }
            set
            {
                _showTaglist = value;
                RaisePropertyChanged("ShowTagList");
            }
        }

        public PreviewScreenVM(BasePost post)
        {
            PreviewPost = post;
            ImageSource = _post.FullPictureURL;
            ShowTagList = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(post.Tags))
                _taglist = new ObservableCollection<string>(post.Tags.Split(' '));
            else
                _taglist = new ObservableCollection<string>();
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

        internal void Download()
        {
            if (string.IsNullOrEmpty(GlobalSettings.Instance.SavePath))
            {
                new MetroMessagebox("Error", "No save directory specified. \nPlease go to settings and select a folder.").ShowDialog();
            }
            else
            {
                _post.SaveImage();
            }
        }

        internal void ShowTags()
        {
            if(ShowTagList == Visibility.Collapsed)
                ShowTagList = Visibility.Visible;
            else
                ShowTagList = Visibility.Collapsed;
        }
    }
}
