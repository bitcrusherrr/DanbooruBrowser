using booruReader.Model;
using booruReader.ViewModels;
using dbz.UIComponents.Debug_utils;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;

namespace booruReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<string> testcollection = new ObservableCollection<string>();
        MainScreenVM viewModel;
        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        public MainWindow()
        {
            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            
            InitializeComponent();
            viewModel = new MainScreenVM();
            DataContext = viewModel;
            viewModel.SearchBoxChanged += viewModel_SearchBoxChanged;
            ImageList.SelectedItem = null;

            if (GlobalSettings.Instance.MainScreenWidth > 0)
            {
                this.Width = GlobalSettings.Instance.MainScreenWidth;
                this.Height = GlobalSettings.Instance.MainScreenHeight;
            }
        }

        //Manually hide label.
        void viewModel_SearchBoxChanged(object sender, EventArgs e)
        {
            SearchBox.HideLabel();
        }

        //Attempt at catching all thread exceptions. As the Domain one doesnt always catch thread unhandled exceptions.
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Instance.LogEvent("Dispatcher exception", e.Exception.Message.ToString(), "Handled: " + e.Handled.ToString());
        }

        //Attempt at catching all top level crashes.
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception) e.ExceptionObject;
            if(exc != null)
                Logger.Instance.LogEvent("Unhadled exception", exc.Message.ToString(), "Terminating: " + e.IsTerminating);
        }

        private DateTime m_headerLastClicked;

        #region Window resize handling

        private void HandleTopResize(Object sender, MouseButtonEventArgs e)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + ResizeDirection.Top), IntPtr.Zero);
        }

        private void HandleLeftResize(Object sender, MouseButtonEventArgs e)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + ResizeDirection.Left), IntPtr.Zero);
        }

        private void HandleBottomResize(Object sender, MouseButtonEventArgs e)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + ResizeDirection.Bottom), IntPtr.Zero);
        }

        private void HandleRightResize(Object sender, MouseButtonEventArgs e)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + ResizeDirection.Right), IntPtr.Zero);
        }

        private void HandleULBR(Object sender, MouseButtonEventArgs e)
        {

        }

        private void HandleURBL(Object sender, MouseButtonEventArgs e)
        {

        }

        private void ResizeMouseTopBottom(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeNS;
        }

        private void ResizeMouseLeftRight(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeWE;
        }

        private void ResizeMouseLeft(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void ResizeMouseULBR(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeNWSE;
        }

        private void ResizeMouseURBL(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeNESW;
        }

        #endregion

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

        #region Scrollview handling

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

        #endregion

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

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

    }
}
