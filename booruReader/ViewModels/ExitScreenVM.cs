using dbz.UIComponents;

namespace booruReader.ViewModels
{
    class ExitScreenVM : BaseIObservable
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
    }
}
