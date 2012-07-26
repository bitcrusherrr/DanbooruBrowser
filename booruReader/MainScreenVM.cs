using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using booruReader.Helpers;
using booruReader.Model;

namespace booruReader
{
    class MainScreenVM
    {
        #region Private variables
        private ObservableCollection<BasePost> _imageList;
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

            //PostsFetcher postFetcher = new PostsFetcher();

            for (int i = 0; i < 10; i++)
                _imageList.Add(new BasePost("Images\\TestImages\\Aliens.jpg", "Images\\TestImages\\Aliens.jpg", PostRating.Safe));

            for (int i = 0; i < 10; i++)
                _imageList.Add(new BasePost("Images\\TestImages\\Aliens.jpg", "Images\\TestImages\\Aliens.jpg", PostRating.Questionable));

            //for (int i = 0; i < 1; i++)
            //{
            //    foreach (BasePost post in postFetcher.GetImages())
            //    {
            //        _imageList.Add(new BasePost(post));
            //    }
            //}
            //Invoke settings class here
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
    }
}
