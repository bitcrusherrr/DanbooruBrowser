
using booruReader.Model;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace booruReader.Helpers
{
    public static class UtilityFunctions
    {
        /// <summary>
        /// Local utility function that returns extension for the file
        /// </summary>
        public static string GetUrlExtension(string file)
        {
            string retval = null;

            if (file.ToLowerInvariant().Contains("jpg") || file.ToLowerInvariant().Contains("jpeg"))
                retval = ".jpg";
            else if (file.ToLowerInvariant().Contains("png"))
                retval = ".png";
            else if (file.ToLowerInvariant().Contains("gif"))
                retval = ".gif";

            return retval;
        }

        public static string NormaliseURL(string url)
        {
            if (!url.Contains("http"))
            {
                url = GlobalSettings.Instance.CurrentBooru.URL + url;
            }

            return url;
        }

        public static string FormTags(string tags)
        {
            string returnTags = tags;

            if (!string.IsNullOrEmpty(tags))
            {
                returnTags = tags.Replace(" ", "+");
            }

            return returnTags;
        }

        public static string GetMD5HashFromFile(string file)
        {
            using (MD5CryptoServiceProvider md5hasher = new MD5CryptoServiceProvider())
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    byte[] checksum = new MD5CryptoServiceProvider().ComputeHash(stream);
                    return (BitConverter.ToString(checksum).Replace("-", string.Empty)).ToLower();
                }
            }
        }


    }
}
