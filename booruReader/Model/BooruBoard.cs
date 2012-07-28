using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using booruReader.Helpers;

namespace booruReader.Model
{
    public class BooruBoard : INotifyPropertyChanged
    {
        private string _name;
        #region Public variables
        public string URL;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

            }
        }
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}
