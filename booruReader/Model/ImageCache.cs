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
        /// Retrieves image from the cache or prepares image and then returns filename via action
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
                //Check if file is actually valid, occasionally there is a rubbish 0byte file.
                if (!File.Exists(BigCachePath + imageName) && FileIsNotZero(BigCachePath + imageName))
                    return false;
                else
                    return true;
            }
            else
            {
                if (!File.Exists(ThumbCachePath + imageName) && FileIsNotZero(ThumbCachePath + imageName))
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// This function checks if file is actually valid, occasionally there is a rubbish 0byte file.
        /// </summary>
        private bool FileIsNotZero(string path)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);

                if (file.Length > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets full-size image
        /// </summary>
        /// <returns>Path to the full image</returns>
        private string GetBigImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath)
        {
            string imagePath = Path.Combine(BigCachePath, FormFilename(imageName, imageURL));

            if (File.Exists(imagePath) && FileIsNotZero(imagePath))
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
        /// <returns>file path</returns>
        private string GetSmallImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath)
        {
            string imagePath;

            //This is a bit hacky and is based on assumption that if we get passed in null url that file was already loaded and this is a call for a thumbnail.
            if (imageURL != null)
                imagePath = Path.Combine(ThumbCachePath, FormFilename(imageName, imageURL));
            else
                imagePath = Path.Combine(ThumbCachePath, imageName);

            if (File.Exists(imagePath) && FileIsNotZero(imagePath))
                return imagePath;
            else
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(finalFilePath);
                client.DownloadFileAsync(new Uri(imageURL), imagePath);

                return null;
            }
        }


        Comparison<FileInfo> FileInfoCompare = new Comparison<FileInfo>(delegate(FileInfo a, FileInfo b)
        {
            return DateTime.Compare(a.LastWriteTimeUtc, b.LastWriteTimeUtc);
        });

        /// <summary>
        /// Cleans up cache, if its above maximum size, remove files until size is half of maximum allowed
        /// </summary>
        public void CleanCache()
        {
            DirectoryInfo bigCacheDir = new DirectoryInfo(BigCachePath);
            long size = bigCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            long cacheSize = GlobalSettings.Instance.CacheSizeMb * 1048576;
            
            if (size > cacheSize)
            {
                //Try to sort files by date of last write to the file, we should get older files at the top of the list
                //Thus removing potentially less needed thumbs
                List<FileInfo> fileList = new List<FileInfo>(bigCacheDir.GetFiles());
                fileList.Sort(FileInfoCompare);

                foreach (FileInfo file in fileList)
                {
                    file.Delete();

                    //Do a little cleanup
                    if (bigCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length) < (cacheSize / 2))
                        break;
                }
            }

            DirectoryInfo smallCacheDir = new DirectoryInfo(ThumbCachePath);
            size = smallCacheDir.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);

            if (size > cacheSize)
            {
                //Try to sort files by date of last write to the file, we should get older files at the top of the list
                //Thus removing potentially less needed thumbs
                List<FileInfo> fileList = new List<FileInfo>(smallCacheDir.GetFiles());
                fileList.Sort(FileInfoCompare);

                foreach (FileInfo file in fileList)
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
            else // This shouldn't happen
                filename = null;

            return md5 + filename;
        }

    }
}
