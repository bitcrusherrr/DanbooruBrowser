using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace booruReader.ViewModels
{
    class ExitScreenVM : INotifyPropertyChanged
    {
        private bool _carryOnExit = false;
        public bool CarryOnExit
        {
            get { return _carryOnExit; }
            set 
            { 
                _carryOnExit = value;
                RaisePropertyChanged("CarryOnExit");
            }
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
