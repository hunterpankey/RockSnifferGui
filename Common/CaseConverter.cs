using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace RockSnifferGui.Common
{
    public class CaseConverter : IValueConverter
    {
        public CharacterCasing Case { get; set; }

        public CaseConverter()
        {
            this.Case = CharacterCasing.Upper;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (str != null)
            {
                switch (this.Case)
                {
                    case CharacterCasing.Lower:
                        return str.ToLower();
                    case CharacterCasing.Normal:
                        return str;
                    case CharacterCasing.Upper:
                        return str.ToUpper();
                    default:
                        return str;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
