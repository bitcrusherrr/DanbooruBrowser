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
using booruReader.ViewModels;

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

            if (viewModel.DownloadsPending())
            {
                DoWaitForDownloads();
            }
            else
            {
                Exit();
            }
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

                int lastItem = ImageList.Items.Count - 1;
                //Check if last item is visible and trigger more loading
                if (lastItem > 0)
                {
                    ListBoxItem listitem = ImageList.ItemContainerGenerator.ContainerFromItem(ImageList.Items[lastItem]) as ListBoxItem;

                    //have to use code different from IsFullyOrPartiallyVisible as we don't know the scroller.
                    if (listitem != null)
                    {
                        GeneralTransform transform = listitem.TransformToVisual(ImageList);
                        Point childToParentCoordinates = transform.Transform(new Point(0, 0));
                        if (childToParentCoordinates.Y >= 0 &&
                            childToParentCoordinates.Y + (listitem.ActualHeight / 4) <= ImageList.ActualHeight)
                        {
                            viewModel.TriggerImageLoading();
                        }
                    }
                }

            }
            else
                this.WindowState = System.Windows.WindowState.Normal;
        }

        private void ShowVisibleItems(object sender)
        {
            var scrollViewer = (FrameworkElement)sender;
            int end = ImageList.Items.Count - 1;
            foreach (BasePost item in ImageList.Items)
            {
                if (item.IsVisible)
                {
                    var listBoxItem = (FrameworkElement)ImageList.ItemContainerGenerator.ContainerFromItem(item);
                    if (!IsFullyOrPartiallyVisible(listBoxItem, scrollViewer))
                        item.IsVisible = false;
                    else
                    {
                        if (ImageList.Items.IndexOf(item) == end && end > 0)
                        {
                            viewModel.TriggerImageLoading();
                        }
                    }
                }
                else
                {
                    var listBoxItem = (FrameworkElement)ImageList.ItemContainerGenerator.ContainerFromItem(item);
                    if (IsFullyOrPartiallyVisible(listBoxItem, scrollViewer))
                        item.IsVisible = true;
                }
            }
        }

        protected bool IsFullyOrPartiallyVisible(FrameworkElement child, FrameworkElement scrollViewer)
        {
            var childTransform = child.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(
                                      new Rect(new Point(0, 0), child.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        } 

        private void ImageList_ScrollChanged_1(object sender, ScrollChangedEventArgs e)
        {
            ShowVisibleItems(sender);
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

        DownloadTracker downloadTracker;
        private void Tracker_Button_Click(object sender, RoutedEventArgs e)
        {
            downloadTracker = new DownloadTracker(viewModel.DowloadList);
            downloadTracker.IsVisibleChanged += downloadTracker_IsVisibleChanged;

            MainGrid.Children.Add(downloadTracker);
        }

        void downloadTracker_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (downloadTracker != null && downloadTracker.Visibility != System.Windows.Visibility.Visible)
            {
                MainGrid.Children.Remove(downloadTracker);
                downloadTracker = null;
            }
        }

        ExitScreenView exitScreen;
        private void DoWaitForDownloads()
        {
            exitScreen = new ExitScreenView();
            exitScreen.IsVisibleChanged += exitScreen_IsVisibleChanged;

            MainGrid.Children.Add(exitScreen);
        }

        void exitScreen_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (exitScreen != null && exitScreen.Visibility != System.Windows.Visibility.Visible)
            {
                MainGrid.Children.Remove(exitScreen);

                if (((ExitScreenVM)exitScreen.DataContext).CarryOnExit)
                {
                    Exit();
                }
                else
                    downloadTracker = null;
            }
        }

        void Exit()
        {
            this.Hide();
            viewModel.Closing();
            this.Close();
        }

    }
}
