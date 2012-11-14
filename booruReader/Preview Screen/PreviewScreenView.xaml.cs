using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using booruReader.Model;
using booruReader.Helpers;
using System.Collections.ObjectModel;

namespace booruReader.Preview_Screen
{
    /// <summary>
    /// Interaction logic for PrviewScreenView.xaml
    /// </summary>
    public partial class PrviewScreenView : Window
    {
        PreviewScreenVM PreviewVM;

        public PrviewScreenView(BasePost post, ObservableCollection<BasePost> DowloadList)
        {
            InitializeComponent();

            PreviewVM = new PreviewScreenVM(post, DowloadList);
            DataContext = PreviewVM;

            if (GlobalSettings.Instance.PreviewScreenWidth > 0)
            {
                this.Width = GlobalSettings.Instance.PreviewScreenWidth;
                this.Height = GlobalSettings.Instance.PreviewScreenHeight;
            }
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
    }
}
