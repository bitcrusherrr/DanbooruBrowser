using booruReader.Model;
using dbz.UIComponents;
using System.Collections.ObjectModel;

namespace booruReader.ViewModels
{
    public class DownloadTrackerVM
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
    }
}
