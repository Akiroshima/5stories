using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DomModel.ViewModels.Converters
{
    /// <summary>
    /// Конвертер для преобразования состояния ячейки (bool) в цвет для отображения на WPF Canvas
    /// </summary>
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFilled)
            {
                return isFilled ? Brushes.Blue : Brushes.White;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для преобразования состояния игры в текст
    /// </summary>
    public class GameStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status;
            }
            return "Неизвестное состояние";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для инверсии логического значения (true → false, false → true)
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : (object)true;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : (object)false;
    }

    /// <summary>
    /// Конвертер для преобразования логического значения в видимость UI элемента
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
