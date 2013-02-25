using booruReader.Helpers;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace booruReader.Model
{
    [Serializable]
    public class BooruBoard : INotifyPropertyChanged
    {
        private string _name;
        private string _URL;
        private string _userName;
        private string _password;
        private string _password_salt;
        private string _password_hash;

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
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        [XmlElement("passwordSalt")]
        public string PasswordSalt
        {
            get { return _password_salt; }
            set
            {
                _password_salt = value;
                RaisePropertyChanged("PasswordSalt");
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
            Password = board.Password;
            UserName = board.UserName;
            ProviderType = board.ProviderType;
            PasswordSalt = board.PasswordSalt;
        }

        public BooruBoard(string url, string name, ProviderAccessType providerType)
        {
            URL = url;
            Name = name;
            ProviderType = providerType;
        }

        public bool HashPassword()
        {
            bool result = true;

            try
            {
                if (!string.IsNullOrEmpty(_password_salt) && _password_salt.Contains("!PASSWORD!"))
                {
                    System.Security.Cryptography.SHA1 hash = System.Security.Cryptography.SHA1.Create();
                    System.Text.ASCIIEncoding encoder = new System.Text.ASCIIEncoding();
                    string str = _password_salt.Replace("!PASSWORD!", _password);
                    byte[] combined = encoder.GetBytes(str);
                    hash.ComputeHash(combined);
                    _password_hash = Convert.ToBase64String(hash.Hash);
                }
                else
                    result = false;
            }
            catch
            {
                result = false;
            }

            //We have password hash now, replace password field with the hash.
            if (result)
            {
                Password = _password_hash;
            }

            return result;
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
