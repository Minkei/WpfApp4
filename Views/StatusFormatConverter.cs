using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp4.Views
{
    public class StatusFormatConverter:IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                //string qrCodeContent = values[0] as string ?? string.Empty;
                string streamingStatus = values[0] as string ?? string.Empty;
                string streamingDevice = values[1] as string ?? string.Empty;
                return $"| Status: {streamingStatus} via by {streamingDevice} |";
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
