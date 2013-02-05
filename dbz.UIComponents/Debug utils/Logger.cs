using System;
using System.IO;

namespace dbz.UIComponents.Debug_utils
{
    /// <summary>
    /// This class is used to log the events throughout the booruReader
    /// </summary>
    public class Logger
    {
        private static Logger instance;
        private string _path;

        private Logger()
        {
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BooruReader") + @"\Log.txt";
        }

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }

        /// <summary>
        /// Basic logging function
        /// </summary>
        /// <param name="functionName">Name of the function that the call is in</param>
        /// <param name="message">Error message</param>
        /// <param name="extraDetails">Extra data</param>
        public void LogEvent(string functionName, string message, string extraDetails = null)
        {
            CheckLogFile();

            string lineOut = DateTime.Now.ToString() + ": Current call: " + functionName + " Result: " + message;

            if (extraDetails != null)
                lineOut += " Misc details: " + extraDetails;

            lineOut += ";" + Environment.NewLine;

            File.AppendAllText(_path, lineOut);
        }

        /// <summary>
        /// Checks if log file exists and creates one if it doesnt
        /// </summary>
        private void CheckLogFile()
        {
            if (!File.Exists(_path))
            {
                File.Create(_path).Dispose();
            }
        }
    }
}
