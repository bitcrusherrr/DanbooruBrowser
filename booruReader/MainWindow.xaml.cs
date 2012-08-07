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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using booruReader.Helpers;
using booruReader.Model;

namespace booruReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<string> testcollection = new ObservableCollection<string>();
        MainScreenVM viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainScreenVM();
            DataContext = viewModel;

            ImageList.SelectedItem = null;

            if (GlobalSettings.Instance.MainScreenWidth > 0)
            {
                this.Width = GlobalSettings.Instance.MainScreenWidth;
                this.Height = GlobalSettings.Instance.MainScreenHeight;
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
                GlobalSettings.Instance.MainScreenWidth = this.Width;
                GlobalSettings.Instance.MainScreenHeight = this.Height;
            }

            this.Hide();
            viewModel.Closing();
            this.Close();
        }

        private void MinimiseButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximiseButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
                LoadMoreImages();
            }
            else
                this.WindowState = System.Windows.WindowState.Normal;
        }

        private void WindowLostFocus(Object sender, MouseEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Instance.PerformVersionCheck();
        }

        private void LoadMoreImages()
        {
            if (GlobalSettings.Instance.TotalPosts > GlobalSettings.Instance.PostsOffset)
            {
                int offset = 0;
                if (ImageList.Items.Count > 5)
                    offset = ImageList.Items.Count - 4;

                for (int index = offset; index < ImageList.Items.Count; index++)
                {
                    ListBoxItem listitem = ImageList.ItemContainerGenerator.ContainerFromItem(ImageList.Items[index]) as ListBoxItem;
                    if (listitem != null)
                    {
                        GeneralTransform transform = listitem.TransformToVisual(ImageList);
                        Point childToParentCoordinates = transform.Transform(new Point(0, 0));
                        if (childToParentCoordinates.Y >= 0 &&
                            childToParentCoordinates.Y + (listitem.ActualHeight / 4) <= ImageList.ActualHeight)
                        {
                            viewModel.TriggerImageLoading();
                            break;
                        }
                    }
                }              
            }

            //Should be able to trigger reload regardless if we ended fetching whole internet
            ImageReloading();
        }

        private void ImageReloading()
        {
            //check if last hidden images is about to be visible and start reloading it
            if (GlobalSettings.Instance.LastHiddenIndex > 0)// We want to have some hidden images 
            {
                ListBoxItem listitem = ImageList.ItemContainerGenerator.ContainerFromItem(ImageList.Items[GlobalSettings.Instance.LastHiddenIndex]) as ListBoxItem;
                if (listitem != null)
                {
                    GeneralTransform transform = listitem.TransformToVisual(ImageList);
                    Point childToParentCoordinates = transform.Transform(new Point(0, 0));
                    if (childToParentCoordinates.Y >= 0 &&
                        childToParentCoordinates.Y + (listitem.ActualHeight / 4) <= ImageList.ActualHeight)
                    {
                        viewModel.TriggerReloading();
                    }
                    else if (GlobalSettings.Instance.LastHiddenIndex > 0)
                    {
                        ListBoxItem listitem2 = ImageList.ItemContainerGenerator.ContainerFromItem(ImageList.Items[GlobalSettings.Instance.LastHiddenIndex]) as ListBoxItem;
                        if (listitem2 != null)
                        {
                            GeneralTransform transform2 = listitem2.TransformToVisual(ImageList);
                            Point childToParentCoordinates2 = transform2.Transform(new Point(0, 0));
                            if (childToParentCoordinates2.Y >= 0 &&
                                childToParentCoordinates2.Y + (listitem2.ActualHeight / 4) <= listitem2.ActualHeight)
                            {
                                ImageReloading(); //Just to check if last item is still visible
                            }
                        }
                    }
                }
            }
        }

        private void ImageList_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            LoadMoreImages();
        }

        private void TextboxKeypres(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                viewModel.TagsBox = (sender as TextBox).Text;
                viewModel.PerformFetchCommand.Execute(true);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                if (sender is Image)
                    viewModel.PreviewImage((sender as Image).Source.ToString());
        }
    }
}
