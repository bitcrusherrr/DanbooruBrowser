using System.Windows;
using System.Windows.Controls;

namespace booruReader.ViewModels
{
    /// <summary>
    /// Interaction logic for ExitScreenView.xaml
    /// </summary>
    public partial class ExitScreenView : UserControl
    {
        ExitScreenVM _exitScreenVM;
        public ExitScreenView()
        {
            _exitScreenVM = new ExitScreenVM();
            DataContext = _exitScreenVM;

            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            _exitScreenVM.CarryOnExit = false;
            Visibility = System.Windows.Visibility.Hidden;
            IsEnabled = false;
        }

        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            _exitScreenVM.CarryOnExit = false;
            Visibility = System.Windows.Visibility.Hidden;
            IsEnabled = false;
        }

        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            _exitScreenVM.CarryOnExit = true;
            Visibility = System.Windows.Visibility.Hidden;
            IsEnabled = false;
        }
    }
}
