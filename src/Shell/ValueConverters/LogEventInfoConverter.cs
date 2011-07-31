using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using NLog;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof(LogEventInfo), typeof(string))]
    public class LogEventInfoConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (LogEventInfo)value;
            return string.Format("{0}|{1}|{2}", item.TimeStamp, item.Level, item.FormattedMessage);
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