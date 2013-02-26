
using booruReader.Model;
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
    }
}
