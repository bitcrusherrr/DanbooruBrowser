using System;
using System.Windows;
using System.Windows.Input;

namespace booruReader.Helpers
{
    /// <summary>
    /// Interaction logic for MetroMessagebox.xaml
    /// </summary>
    public partial class MetroMessagebox : Window
    {
        MetroMessageBoxVM viewModel;

        public MetroMessagebox(string caption, string message)
        {
            Owner = Application.Current.MainWindow;
            InitializeComponent();

            viewModel = new MetroMessageBoxVM(caption, message);
            DataContext = viewModel;
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
