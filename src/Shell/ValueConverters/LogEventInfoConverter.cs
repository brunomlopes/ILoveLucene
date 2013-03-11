using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using NLog;
using System.Linq;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof(LogEventInfo), typeof(string))]
    public class LogEventInfoConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (LogEventInfo)value;
            var builder = new StringBuilder();
            builder.AppendFormat("{0}|{1}|{2}", item.TimeStamp, item.Level, item.FormattedMessage);
            if (item.Exception !=null)
            {
                var exceptions = new []{item.Exception};
                if (item.Exception is AggregateException)
                {
                    exceptions = ((AggregateException) item.Exception).InnerExceptions.ToArray();
                }
                foreach (var ex in exceptions)
                {
                    builder.AppendFormat("\n{0}{1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
                }
            }
            return builder.ToString();
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