using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using booruReader.Helpers;

namespace booruReader.Model
{
    public class BooruBoard
    {

        #region Public variables
        public string URL;
        public string Name;
        public ProviderAccessType ProviderType;
        #endregion 

        public BooruBoard(BooruBoard board)
        {
            URL = board.URL;
            Name = board.Name;
            ProviderType = board.ProviderType;
        }

        public BooruBoard(string url, string name, ProviderAccessType providerType)
        {
            URL = url;
            Name = name;
            ProviderType = providerType;
        }
    }
}
