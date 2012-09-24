using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace booruReader.Settings_Screen
{
    /// <summary>
    /// Interaction logic for SettingsUserControl.xaml
    /// </summary>
    public partial class SettingsUserControl : System.Windows.Controls.UserControl
    {
        SettingsVM viewModel;

        public SettingsUserControl()
        {
            InitializeComponent();

            viewModel = new SettingsVM();
            DataContext = viewModel;
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Closing();
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            long local;
            long.TryParse(CacheSizeTextBox.Text, out local);
            viewModel.ImageChacheSize = local;
        }

        private void CacheSizeTextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsNumber(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }
    }
}
