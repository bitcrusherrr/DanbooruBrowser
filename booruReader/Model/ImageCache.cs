using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace booruReader.Model
{
    public class ImageCache
    {
        public string BigCachePath;
        public string ThumbCachePath;

        public ImageCache()
        {
            //Setup default path, dont bother checking if it exists, settings screen creates it on load.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "BooruReader");
            BigCachePath = Path.Combine(path, "BigCache");
            ThumbCachePath = Path.Combine(path, "SmallCache");

            //Check if folders for cache exist
            if (!Directory.Exists(BigCachePath))
                Directory.CreateDirectory(BigCachePath);

            if (!Directory.Exists(ThumbCachePath))
                Directory.CreateDirectory(ThumbCachePath);
        }

        /// <summary>
        /// Retrieves image from the cahce or prepares image and then returns filename via action
        /// </summary>
        public string GetImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath, bool BigImage = true)
        {
            if (BigImage)
                return GetBigImage(imageName, imageURL, finalFilePath);
            else
                return GetSmallImage(imageName, imageURL, finalFilePath);
        }

        /// <summary>
        /// Checks if file exists in the cache
        /// </summary>
        public bool FileExists(string imageName, bool BigImage = true)
        {
            if (BigImage)
            {
                if (!File.Exists(BigCachePath + imageName))
                    return false;
                else
                    return true;
            }
            else
            {
                if (!File.Exists(ThumbCachePath + imageName))
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Gets fullsize image
        /// </summary>
        /// <returns>Path to the full image</returns>
        private string GetBigImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath)
        {
            string imagePath = Path.Combine(BigCachePath, FormFilename(imageName, imageURL));

            if (File.Exists(imagePath))
                return imagePath;
            else
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(finalFilePath);
                client.DownloadFileAsync(new Uri(imageURL), imagePath);

                return null;
            }
        }

        /// <summary>
        /// Gets small image
        /// </summary>
        /// <returns>filepath</returns>
        private string GetSmallImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath)
        {
            string imagePath;

            //This is a bit hacky and is based on assumption that if we get passed in null url that file was already loaded and this is a call for a thumbnail.
            if (imageURL != null)
                imagePath = Path.Combine(ThumbCachePath, FormFilename(imageName, imageURL));
            else
                imagePath = Path.Combine(ThumbCachePath, imageName);

            if (File.Exists(imagePath))
                return imagePath;
            else
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(finalFilePath);
                client.DownloadFileAsync(new Uri(imageURL), imagePath);

                return null;
            }
        }

        /// <summary>
        /// Cleans up cache, if its above maximum size, remove files until size is half of maximum allowed
        /// </summary>
        public void CleanCache()
        {
            DirectoryInfo bigCacheDir = new DirectoryInfo(BigCachePath);
            long size = bigCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            //1gb atm for each folder
            const long cacheSize = 1024000000;

            //Roughly 500 megs
            if (size > cacheSize)
            {
                foreach (FileInfo file in bigCacheDir.GetFiles())
                {
                    file.Delete();

                    //Do a little cleanup
                    if (bigCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length) < (cacheSize / 2))
                        break;
                }
            }

            DirectoryInfo smallCacheDir = new DirectoryInfo(ThumbCachePath);
            size = smallCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            //Roughly 500 megs
            if (size > cacheSize)
            {
                foreach (FileInfo file in smallCacheDir.GetFiles())
                {
                    file.Delete();

                    //Do a little cleanup
                    if (smallCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length) < (cacheSize / 2))
                        break;
                }
            }
        }

        private string FormFilename(string md5, string FullPictureURL)
        {
            string filename;

            if (FullPictureURL.ToLowerInvariant().Contains("jpg") || FullPictureURL.ToLowerInvariant().Contains("jpeg"))
                filename = ".jpg";
            else if (FullPictureURL.ToLowerInvariant().Contains("png"))
                filename = ".png";
            else if (FullPictureURL.ToLowerInvariant().Contains("gif"))
                filename = ".gif";
            else // This shouldnt happen
                filename = null;

            return md5 + filename;
        }

    }
}
