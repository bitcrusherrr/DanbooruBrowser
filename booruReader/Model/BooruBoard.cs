using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using booruReader.Helpers;

namespace booruReader.Model
{
    [Serializable]
    public class BooruBoard : INotifyPropertyChanged
    {
        private string _name;
        private string _URL;
        private string _userName;
        private string _password;

        #region Public variables
        [XmlElement("url")]
        public string URL
        {
            get { return _URL; }
            set
            {
                _URL = value;
                RaisePropertyChanged("URL");
            }
        }
    
        [XmlElement("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        [XmlElement("providerType")]
        public ProviderAccessType ProviderType;
        
        [XmlElement("userName")]
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }
        
        [XmlElement("password")]
        public string Pasword
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Pasword");
            }
        }
        
        #endregion 
        
        /// <summary>
        /// For serialization only, DO NOT USE
        /// </summary>
        public BooruBoard()
        {
        }

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
