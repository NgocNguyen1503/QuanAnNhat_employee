using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QuanAnNhat_admin.Converter
{
    class ThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new BitmapImage(new Uri("pack://application:,,,/Assets/Images/dish1.jpg"));
            }
            string thumbnailPath = $"pack://application:,,,/Assets/Images/{(int)value}.jpg";

            return new BitmapImage(new Uri(thumbnailPath, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
