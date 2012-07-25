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
        private ObservableCollection<BaseImage> _imageList;
        #endregion

        #region Public variables
        //UI list for the images
        public ObservableCollection<BaseImage> MainImageList
        {
            get { return _imageList; }
        }
        #endregion

        /// <summary>
        /// Main view model for the BooruReader window
        /// </summary>
        public MainScreenVM()
        {
            _imageList = new ObservableCollection<BaseImage>();

            PostsFetcher postFetcher = new PostsFetcher("","");

            for (int i = 0; i < 200; i++)
                MainImageList.Add(new BaseImage(@"Images\TestImages\Aliens.jpg",@"Images\TestImages\Aliens.jpg"));
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
