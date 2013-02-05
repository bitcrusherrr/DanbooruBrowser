using booruReader.Model;
using dbz.UIComponents.Debug_utils;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            Visibility = System.Windows.Visibility.Hidden;
            IsEnabled = false;
        }

        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ImageList.Items.CurrentItem != null)
            {
                var item = ImageList.Items.CurrentItem as BasePost;

                if (item != null && item.DownloadProgress == 100)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(item.GetFileLocation());
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogEvent("HandleDoubleClick", ex.Message);
                    }
                }
            }
        }
    }
}
