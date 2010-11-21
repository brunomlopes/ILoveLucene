using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Core.Abstractions;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof (ICommand), typeof (string))]
    public class ICommandToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICommand command = value as ICommand;
            if (command == null) return value;
            else
            {
                return command.Text;
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