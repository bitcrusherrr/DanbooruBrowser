using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using booruReader.Helpers;
using System.Windows.Forms;
using System.Diagnostics;

namespace booruReader.Model.NewDownloads
{
    //Load this class once and have it keeping the folder contents md5 cached
    internal class FolderCache
    {
        private struct FileDetails
        {
            public string FileMD5;
            public string FilePath;
        };

        private List<FileDetails> _fileList;
        internal FolderCache()
        {
            Stopwatch sw = Stopwatch.StartNew();

            _fileList = ConstructFileMD5List();

            sw.Stop();

            MessageBox.Show("Parsed: " + _fileList.Count.ToString() + " Files, in " + sw.Elapsed.TotalSeconds + "s", "Loaded file details");
        }

        private List<FileDetails> ConstructFileMD5List()
        {
            List<FileDetails> fileList = new List<FileDetails>();

            if (Directory.Exists(GlobalSettings.Instance.SavePath))
            {
                string[] files = Directory.GetFiles(GlobalSettings.Instance.SavePath);

                //Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 2 }, currentFile =>
                //{
                //    FileDetails details = new FileDetails();
                //    details.FileMD5 = UtilityFunctions.GetMD5HashFromFile(currentFile);
                //    details.FilePath = currentFile;

                //    fileList.Add(details);
                //}
                //);

                foreach (string file in files)
                {
                    FileDetails details = new FileDetails();
                    details.FileMD5 = UtilityFunctions.GetMD5HashFromFile(file);
                    details.FilePath = file;

                    fileList.Add(details);
                }
            }

            return fileList;
        }

        public bool CheckIfFileExists(string externalMD5)
        {
            bool fileExists = false;

            return fileExists;
        }
    }
}
