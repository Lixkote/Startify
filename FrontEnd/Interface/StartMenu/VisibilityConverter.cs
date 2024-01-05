using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;

namespace Shell.Interface.StartMenu
{
    /// <summary>
    /// Converter from/to Visibility and Boolean.
    /// </summary>
    /// <remarks>
    /// true = Visible
    /// false = Collapsed
    /// </remarks>
    public class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConverter"/> class.
        /// </summary>
        public VisibilityConverter()
        {
            Opposite = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="VisibilityConverter"/> is opposite.
        /// </summary>
        /// <value><c>true</c> if opposite; otherwise, <c>false</c>.</value>
        public bool Opposite { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var not = object.Equals(parameter, "not") || Opposite;
            if (value is bool && targetType == typeof(Visibility))
            {
                return ((bool)value) != not ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is Visibility && targetType.GetType() == typeof(Boolean))
            {
                return (((Visibility)value) == Visibility.Visible) != not;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return Convert(value, targetType, parameter, culture);
        }


        #endregion
    }
}
