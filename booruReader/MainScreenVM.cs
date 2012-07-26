using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using booruReader.Helpers;
using booruReader.Model;
using System.ComponentModel;

namespace booruReader
{
    class MainScreenVM : INotifyPropertyChanged
    {
        #region Private variables
        private ObservableCollection<BasePost> _imageList;
        PostsFetcher _postFetcher;
        #endregion

        #region Public variables
        //UI list for the images
        public ObservableCollection<BasePost> MainImageList
        {
            get { return _imageList; }
        }
        #endregion

        /// <summary>
        /// Main view model for the BooruReader window
        /// </summary>
        public MainScreenVM()
        {
            _imageList = new ObservableCollection<BasePost>();

            _postFetcher = new PostsFetcher();
            foreach (BasePost post in _postFetcher.GetImages())
            {
                _imageList.Add(new BasePost(post));
            }
            //for (int i = 0; i < 10; i++)
            //    _imageList.Add(new BasePost("Images\\TestImages\\Aliens.jpg", "Images\\TestImages\\Aliens.jpg", PostRating.Safe));

            //for (int i = 0; i < 10; i++)
            //    _imageList.Add(new BasePost("Images\\TestImages\\Aliens.jpg", "Images\\TestImages\\Aliens.jpg", PostRating.Questionable));

            //for (int i = 0; i < 1; i++)
            //{
            //    foreach (BasePost post in postFetcher.GetImages())
            //    {
            //        _imageList.Add(new BasePost(post));
            //    }
            //}
            //Invoke settings class here
        }

        public void TriggerImageLoading()
        {
            foreach (BasePost post in _postFetcher.GetImages())
            {
                _imageList.Add(new BasePost(post));
            }
        }

        #region Commands

        #region Close Command
        /// <summary>
        /// Close command that handles saving of the notes for the databases.
        /// </summary>
        public void Closing()
        {
            //Do saving n shit
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
