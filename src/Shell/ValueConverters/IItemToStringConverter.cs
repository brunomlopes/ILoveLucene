using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Core.Abstractions;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof (IItem), typeof (string))]
    public class IItemToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IItem item = value as IItem;
            if (item == null) return value;
            else
            {
                return item.Text;
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