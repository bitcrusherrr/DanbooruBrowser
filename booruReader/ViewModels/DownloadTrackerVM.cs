using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using booruReader.Model;
using booruReader.Helpers;
using dbz.UIComponents;

namespace booruReader.ViewModels
{
    public class DownloadTrackerVM : INotifyPropertyChanged
    {
        private ObservableCollection<BasePost> _downloadsList;
        private DelegateCommand _clearCompletedCommand;

        public ObservableCollection<BasePost> DownloadsList
        {
            get { return _downloadsList; }
        }

        public DownloadTrackerVM(ObservableCollection<BasePost> imagelist)
        {
            _downloadsList = imagelist;

            _clearCompletedCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => ClearCompleted()
            };
        }

        public DelegateCommand ClearCompletedCommand { get { return _clearCompletedCommand; } }

        private void ClearCompleted()
        {
            int index = 0;
            while (index < _downloadsList.Count)
            {
                if (_downloadsList[index].DownloadProgress == 100)
                {
                    _downloadsList.RemoveAt(index);
                }
                else
                {
                    index++;
                }
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
