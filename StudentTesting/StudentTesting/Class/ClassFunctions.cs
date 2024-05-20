using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace StudentTesting.View.Pages
{
    public class TestEnabledMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is bool isCheck && values[1] is DateTime dateTime)
            {
                // Элемент неактивен если Check = true или время уже прошло
                return !(isCheck || dateTime < DateTime.Now);
            }
            return true; // По умолчанию элемент активен
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DateTimeHasPassedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime < DateTime.Now;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TestDateTimeStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime > DateTime.Now)
                {
                    // Возвращаем сообщение о том, когда тест будет закрыт
                    return $"Тест открыт до {dateTime:dd.MM.yyyy HH:mm}";
                }
                else
                {
                    // Возвращаем сообщение о закрытии теста, если нужно
                    return "Тест закрыт";
                }
            }

            // В случае некорректного значения или другой ошибки
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BallToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double ball)
            {
                if (ball > 44f)
                {
                    return Brushes.Green;
                }
                else if (ball >= 35f)
                {
                    return Brushes.Blue;
                }
                else if (ball >= 25f)
                {
                    return Brushes.Orange;
                }
                else
                {
                    return Brushes.Red;
                }
            }

            return Brushes.Black; // Default color if the value is not valid
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
