using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tools;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Client {
    public sealed partial class ErrorViewer : UserControl {
        static Brush errorBrush = new SolidColorBrush(Colors.Red);
        static Brush unknownBrush = new SolidColorBrush(Colors.Yellow);
        static Brush goodBrush = new SolidColorBrush(Colors.Green);

        public ErrorViewer() {
            this.InitializeComponent();
        }

        public bool ShowCompositeStatus { get; set; }

        public bool OnlyShowStatusWhenUnknown { get; set; } = true;

        public IReadOnlyProperty<ValidationDataErrorInfo> ErrorInfo
        {
            get { return (IReadOnlyProperty<ValidationDataErrorInfo>)GetValue(ErrorInfoProperty); }
            set { SetValue(ErrorInfoProperty, value); }
        }

        private static string StatusToString(ValidationStatus s) => s.ToString();
        private static string ErrorsToString(IEnumerable errors) => errors?.Cast<object>().Select(i => i.ToString()).Aggregate(string.Empty, (i, j) => $"{i},{j}") ?? string.Empty;

        public static readonly DependencyProperty ErrorInfoProperty =
            DependencyProperty.Register("ErrorInfo", typeof(IReadOnlyProperty<ValidationDataErrorInfo>), typeof(ErrorViewer), new PropertyMetadata(null, (d, args) => {
                ErrorViewer viewer = d as ErrorViewer;
                IReadOnlyProperty<ValidationDataErrorInfo> value = args.NewValue as IReadOnlyProperty<ValidationDataErrorInfo>;
                if (value != null) {
                    value.Subscribe(i => {
                        if (i == null) return;
                        var x = value.Value;
                        if (viewer.OnlyShowStatusWhenUnknown && x.Status == ValidationStatus.IsValid) {
                            viewer.nodeStatus.Visibility = Visibility.Collapsed;
                        }
                        else {
                            viewer.nodeStatus.Visibility = Visibility.Visible;
                            viewer.shape.Fill = x.Status.HasFlag(ValidationStatus.HasErrors) ? errorBrush : ValidationStatus.Unknown.HasFlag(x.Status) ? unknownBrush : goodBrush;
                            viewer.errorMessage.Text = $"{StatusToString(x.Status)} {ErrorsToString(x.Errors)}";
                            viewer.errorMessage.Visibility = (x.Status != ValidationStatus.IsValid) ? Visibility.Visible : Visibility.Collapsed;
                        }
                        if (viewer.ShowCompositeStatus && (x.CompositeStatus != ValidationStatus.IsValid || !viewer.OnlyShowStatusWhenUnknown)) {
                            viewer.compositeStatus.Visibility = Visibility.Visible;
                            viewer.compositeShape.Fill = x.CompositeStatus.HasFlag(ValidationStatus.HasErrors) ? errorBrush : ValidationStatus.Unknown.HasFlag(x.CompositeStatus) ? unknownBrush : goodBrush;
                            viewer.compositeErrorMessage.Text = $"OVERALL: {StatusToString(x.CompositeStatus)}";
                            viewer.compositeErrorMessage.Visibility = (x.CompositeStatus != ValidationStatus.IsValid) ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else {
                            viewer.compositeStatus.Visibility = Visibility.Collapsed;
                        }
                    });
                }
            }));
    }
}
