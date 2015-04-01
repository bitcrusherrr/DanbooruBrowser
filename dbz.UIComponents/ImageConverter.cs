using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace dbz.UIComponents
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (!(value is String))
                throw new ApplicationException("Value must be string!");

            if (string.IsNullOrEmpty(value as string))
                return DependencyProperty.UnsetValue;

            try
            {
                return new BitmapImage(new Uri(value as string));
            }
            catch
            {
                try
                {
                    // Try to deal with some corrupt images
                    // See: http://www.hanselman.com/blog/DealingWithImagesWithBadMetadataCorruptedColorProfilesInWPF.aspx
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                    bi.UriSource = new Uri(value as string);
                    bi.EndInit();
                    return bi;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
