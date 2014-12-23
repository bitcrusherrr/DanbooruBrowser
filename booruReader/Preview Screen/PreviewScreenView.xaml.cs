using booruReader.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace booruReader.Preview_Screen
{
    /// <summary>
    /// Interaction logic for PrviewScreenView.xaml
    /// </summary>
    public partial class PrviewScreenView : Window
    {
        PreviewScreenVM PreviewVM;

        public event EventHandler ScreenClosing;
        public event EventHandler AddedImageToFavorites;
        public event EventHandler RemovedImageFromFavorites;
        public event EventHandler UserTagSelection;

        public PrviewScreenView(BasePost post, ObservableCollection<BasePost> DowloadList)
        {
            InitializeComponent();

            PreviewVM = new PreviewScreenVM(post, DowloadList);
            DataContext = PreviewVM;

            PreviewVM.AddedImageToFavorites += PreviewVM_AddedImageToFavorites;
            PreviewVM.RemovedImageFromFavorites += PreviewVM_RemovedImageFromFavorites;

            if (GlobalSettings.Instance.PreviewScreenWidth > 0)
            {
                this.Width = GlobalSettings.Instance.PreviewScreenWidth;
                this.Height = GlobalSettings.Instance.PreviewScreenHeight;
            }
        }

        void PreviewVM_RemovedImageFromFavorites(object sender, EventArgs e)
        {
            if (RemovedImageFromFavorites != null)
                RemovedImageFromFavorites(sender, e);
        }

        void PreviewVM_AddedImageToFavorites(object sender, EventArgs e)
        {
            if (AddedImageToFavorites != null)
                AddedImageToFavorites(sender, e);
        }

        private DateTime m_headerLastClicked;

        private void HandleHeaderPreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            m_headerLastClicked = DateTime.Now;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                //Unmaximise window
                if (this.WindowState != System.Windows.WindowState.Normal)
                {
                    this.WindowState = System.Windows.WindowState.Normal;

                    //Move window to the cursor
                    this.Left = Mouse.GetPosition(this).X - 15;
                    this.Top = Mouse.GetPosition(this).Y - 15;
                }

                DragMove();
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Width > 0 && this.Height > 0)
            {
                GlobalSettings.Instance.PreviewScreenWidth = this.Width;
                GlobalSettings.Instance.PreviewScreenHeight = this.Height;
            }

            if (ScreenClosing != null)
                ScreenClosing(this, new EventArgs());

            PreviewVM.AddedImageToFavorites -= PreviewVM_AddedImageToFavorites;
            PreviewVM.RemovedImageFromFavorites -= PreviewVM_RemovedImageFromFavorites;

            this.Hide();
            this.Close();
        }

        private void ShowTaglist(object sender, RoutedEventArgs e)
        {
            PreviewVM.ShowTags();
        }    

        private void DownloadImage(object sender, RoutedEventArgs e)
        {
            PreviewVM.Download();
        }

        //This 2 functions are hackaround to make sure window loads on top of main application at first
        private void Window_ContentRendered(object sender, EventArgs e) 
        { 
            this.Topmost = false; 
        }

        private void Window_Initialized(object sender, EventArgs e) 
        { 
            this.Topmost = true; 
        }

        private void PrviewScreenView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.Width > 0 && this.Height > 0)
            {
                GlobalSettings.Instance.PreviewScreenWidth = this.Width;
                GlobalSettings.Instance.PreviewScreenHeight = this.Height;
            }
        }

        private void AddToFavorites(object sender, RoutedEventArgs e)
        {
            PreviewVM.AddToFavorites();
        }

        private void RemoveFromFavorites(object sender, RoutedEventArgs e)
        {
            PreviewVM.RemoveFromFavorites();
        }

        protected void HandleDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TagList.SelectedItem != null && TagList.SelectedIndex >= 3) // assumes first 3 entries are resolution+labels
            {
                var item = TagList.SelectedItem as string;

                if (item != null && UserTagSelection != null)
                    UserTagSelection(item, new EventArgs());
            }
        }
    }
}
