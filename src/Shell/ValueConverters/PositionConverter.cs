using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ILoveLucene.ValueConverters
{
    [ValueConversion(typeof (ListBoxItem), typeof (string))]
    public class PositionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListBoxItem item = (ListBoxItem) value;
            ListBox view = (ListBox) ItemsControl.ItemsControlFromItemContainer(item);
            int index = (view.ItemContainerGenerator.IndexFromContainer(item) + 1);
            return index.ToString();
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