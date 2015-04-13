using booruReader.Helpers;
using dbz.UIComponents.Debug_utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace booruReader.Model
{
    public class ImageCache
    {
        private string BigCachePath;
        private string ThumbCachePath;

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
        public string GetImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath, DownloadProgressChangedEventHandler progChange = null, bool BigImage = true)
        {
            if (BigImage)
                return GetBigImage(imageName, imageURL, finalFilePath, progChange);
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
                return File.Exists(BigCachePath + imageName) || !FileIsNotZero(BigCachePath + imageName);
            }
            return File.Exists(ThumbCachePath + imageName) || !FileIsNotZero(ThumbCachePath + imageName);
        }

        /// <summary>
        /// This function checks if file is actually valid, occasionally there is a rubbish 0byte file.
        /// </summary>
        private bool FileIsNotZero(string path)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);

                return (file.Length > 0);
            }
            return false;
        }

        /// <summary>
        /// Gets full-size image
        /// </summary>
        /// <returns>Path to the full image</returns>
        private string GetBigImage(string imageName, string imageURL, Action<object, AsyncCompletedEventArgs> finalFilePath, DownloadProgressChangedEventHandler progChange)
        {
            string imagePath = Path.Combine(BigCachePath, string.Format(imageName + UtilityFunctions.GetUrlExtension(imageURL)));

            if (File.Exists(imagePath) && FileIsNotZero(imagePath))
                return imagePath;

            WebClient client = new WebClient();

            // KBR 20150405 Issue #5: optional progress handler, so the preview download progress updates
            if (progChange != null)
                client.DownloadProgressChanged += progChange;

            client.DownloadFileCompleted += new AsyncCompletedEventHandler(finalFilePath);
            client.DownloadFileAsync(new Uri(imageURL), imagePath);

            return null;
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
                imagePath = Path.Combine(ThumbCachePath, string.Format(imageName + UtilityFunctions.GetUrlExtension(imageURL)));
            else
                imagePath = Path.Combine(ThumbCachePath, imageName);

            if (File.Exists(imagePath) && FileIsNotZero(imagePath))
                return imagePath;

            if (imageURL == null) // KBR 20150413 hacky assumption above fails
                return null;

            try
            {
                WebClient client = new WebClient();
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(finalFilePath);

                // KBR 20150413 This seems to be what gets Sankaku working again [and no apparent negative impact on other servers]
                client.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.116 Safari/537.36");
                client.DownloadFileAsync(new Uri(imageURL), imagePath);
            }
            catch (Exception e)
            {
                //This occasionally dumps out, I think when downloading times out, need to handle this on the correct ways.
                Logger.Instance.LogEvent("GetSmallImage", e.Message);
            }

            return null;
        }


        Comparison<FileInfo> FileInfoCompare = delegate(FileInfo a, FileInfo b)
        {
            return DateTime.Compare(a.LastWriteTimeUtc, b.LastWriteTimeUtc);
        };

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
    }
}
