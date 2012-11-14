using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
