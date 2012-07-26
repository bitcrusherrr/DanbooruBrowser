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
            //this.LostFocus += new System.EventHandler(WindowLostFocus);
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
                this.WindowState = System.Windows.WindowState.Maximized;
            else
                this.WindowState = System.Windows.WindowState.Normal;
        }

        private void WindowLostFocus(Object sender, MouseEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        //#region Resize procedures

        //#region Sides

        //private void ResizeTop(object sender, MouseButtonEventArgs e)
        //{
        //    this.Cursor = Cursors.SizeNS;

        //    m_headerLastClicked = DateTime.Now;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeBottom(object sender, MouseButtonEventArgs e)
        //{
        //    this.Cursor = Cursors.SizeNS;

        //    m_headerLastClicked = DateTime.Now;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeLeft(object sender, MouseButtonEventArgs e)
        //{
        //    this.Cursor = Cursors.SizeWE;

        //    m_headerLastClicked = DateTime.Now;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeRight(object sender, MouseButtonEventArgs e)
        //{
        //    this.Cursor = Cursors.SizeWE;

        //    m_headerLastClicked = DateTime.Now;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //#endregion

        //#region Corners

        //private void ResizeTopRight(object sender, MouseButtonEventArgs e)
        //{
        //    m_headerLastClicked = DateTime.Now;
        //    this.Cursor = Cursors.SizeNESW;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeTopLeft(object sender, MouseButtonEventArgs e)
        //{
        //    m_headerLastClicked = DateTime.Now;
        //    this.Cursor = Cursors.SizeNWSE;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeBottomRight(object sender, MouseButtonEventArgs e)
        //{
        //    m_headerLastClicked = DateTime.Now;
        //    this.Cursor = Cursors.SizeNWSE;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //private void ResizeBottomLeft(object sender, MouseButtonEventArgs e)
        //{
        //    m_headerLastClicked = DateTime.Now;
        //    this.Cursor = Cursors.SizeNESW;
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        DragMove();
        //    }
        //}

        //#endregion


        //#endregion
    }
}
