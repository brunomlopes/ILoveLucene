using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class TimestampConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (DateTime) value;
            if((DateTime.Now -item).TotalHours > 23)
            {
                return item.ToShortDateString() + " " + item.ToShortTimeString();
            }
            return item.ToShortTimeString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}