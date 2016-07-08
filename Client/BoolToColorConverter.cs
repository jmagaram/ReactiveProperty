using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Client {
    public class BoolToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            bool? v = (bool?)value;
            return v == null ? NullBrush : v == false ? FalseBrush : TrueBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotImplementedException();
        }

        public Brush FalseBrush { get; set; }
        public Brush TrueBrush { get; set; }
        public Brush NullBrush { get; set; }
    }
}
