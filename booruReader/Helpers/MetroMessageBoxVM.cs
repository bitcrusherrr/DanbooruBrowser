using dbz.UIComponents;

namespace booruReader.Helpers
{
    public class MetroMessageBoxVM : BaseIObservable
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
    }
}
