using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using booruReader.Model;

namespace booruReader.ViewModels
{
    /// <summary>
    /// Interaction logic for DownloadTracker.xaml
    /// </summary>
    public partial class DownloadTracker : UserControl
    {
        private DownloadTrackerVM _downloadTrackerVM;
        public DownloadTracker(ObservableCollection<BasePost> imagelist)
        {
            _downloadTrackerVM = new DownloadTrackerVM(imagelist);
            DataContext = _downloadTrackerVM;
            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            //viewModel.Closing();
            Visibility = System.Windows.Visibility.Hidden;
            IsEnabled = false;
        }
    }
}
