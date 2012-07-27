using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace booruReader.Helpers
{
    public class MetroMessageBoxVM : INotifyPropertyChanged
    {
        private string _caption;
        private string _message;

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                RaisePropertyChanged("Caption");
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        public MetroMessageBoxVM(string caption, string message)
        {
            Message = message;
            Caption = caption;
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
