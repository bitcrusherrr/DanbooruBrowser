using dbz.UIComponents;

namespace booruReader
{
    public partial class MainScreenVM : BaseIObservable
    {
        private DelegateCommand _performFetchCommand;
        private DelegateCommand _performSelectedImagesDownloadCommand;
        private DelegateCommand _clearSelectionCommand;
        private DelegateCommand _openSettingsCommand;
        private DelegateCommand _setFavoritesModeCommand;

        public DelegateCommand PerformSelectedImagesDownloadCommand { get { return _performSelectedImagesDownloadCommand; } }
        public DelegateCommand ClearSelectionCommand { get { return _clearSelectionCommand; } }
        public DelegateCommand OpenSettingsCommand { get { return _openSettingsCommand; } }
        public DelegateCommand SetFavoritesModeCommand { get { return _setFavoritesModeCommand; } }
        public DelegateCommand PerformFetchCommand { get { return _performFetchCommand; } }

        private void InitialiseDelegates()
        {
            _performFetchCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => FetchImages()
            };

            _performSelectedImagesDownloadCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => !IsFavoritesMode,
                ExecuteDelegate = x => SaveImages()
            };

            _clearSelectionCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => RemoveSelection()
            };

            _openSettingsCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => { SettingsOpen = !SettingsOpen; }
            };

            _setFavoritesModeCommand = new DelegateCommand
            {
                CanExecuteDelegate = x => true,
                ExecuteDelegate = x => { IsFavoritesMode = !IsFavoritesMode; }
            };
        }

    }
}
