using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace booruReader.Model
{
    /// <summary>
    /// This is an offline posts fetcher for the favorites
    /// </summary>
    public class FavoriteHandler
    {
        private List<BasePost> _favoritesList;

        public FavoriteHandler()
        {
            FolderSetup();
            _favoritesList = new List<BasePost>();
            //This is slightly more open

            //Load in favorites xml that contains tag info and file detail

            //Go through file paths and if some dont exist remove them 
        }

        #region Public functions

        public List<BasePost> FetchFavorites(string tagsToMatch)
        {
            List<BasePost> returnList;
            if (string.IsNullOrEmpty(tagsToMatch))
                returnList = _favoritesList;
            else
            {
                returnList = new List<BasePost>();

                Parallel.ForEach<string>(tagsToMatch.Split(' '), (tag) =>
                {
                    foreach (BasePost post in _favoritesList.Where(x => x.Tags.Contains(tag)))
                    {
                        if (returnList.FirstOrDefault(x => x.Tags.Contains(tag)) == null)
                            returnList.Add(post);
                    }
                });
            }

            return returnList;
        }

        public void AddToFavorites(BasePost webPost)
        {
            // Replace http paths with the drive paths to the images, ideally caching should solve this problem, but this needs testing

            UpdateFavoritesConfigFile();
        }

        public void RemoveFromFavorites(BasePost post)
        {
            //Delete files off the drive and remove entry from list/xml file

            UpdateFavoritesConfigFile();
        }

        #endregion

        #region Private functions

        private void FolderSetup()
        {
            //Check for favorites folder
            //Make it
            
            //Check for favorite thumbs folder
            //Make it

            throw new NotImplementedException();
        }

        private void UpdateFavoritesConfigFile()
        {
            //Check if file exists
            //Write out file
            
            throw new NotImplementedException();
        }

        #endregion

    }
}
