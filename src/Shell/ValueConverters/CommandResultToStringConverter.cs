using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Core.Abstractions;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof (AutoCompletionResult.CommandResult), typeof (string))]
    public class CommandResultToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value as AutoCompletionResult.CommandResult;
            if (result == null) return value;
            else
            {
                return result.Command.Text;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}