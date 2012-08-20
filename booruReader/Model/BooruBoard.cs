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
        #region Public variables
        [XmlElement("url")]
        public string URL;
    
        [XmlElement("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

            }
        }
        
        [XmlElement("providerType")]
        public ProviderAccessType ProviderType;
        
        [XmlElement("userName")]
        public string UserName;
        
        [XmlElement("password")]
        public string Pasword;
        
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
