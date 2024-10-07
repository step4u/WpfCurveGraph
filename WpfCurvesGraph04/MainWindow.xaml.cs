using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfCurvesGraph04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap originalBitmap;

        public MainWindow()
        {
            InitializeComponent();

            LoadImage();
        }

        private void LoadImage()
        {
            var bitmap = new BitmapImage(new Uri("D:\\Project\\CSI\\도장분석\\Data\\KS-11.png", UriKind.Relative));
            originalBitmap = new WriteableBitmap(bitmap);
            ImageDisplay.Source = originalBitmap;
        }

        private void CurveSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            if (!slider.IsMouseOver)
            {
                return;
            }

            ApplyCurves(ControlPoint1Slider.Value, ControlPoint2Slider.Value);
        }

        private void ApplyCurves(double controlPoint1, double controlPoint2)
        {
            if (originalBitmap == null) return;

            int width = originalBitmap.PixelWidth;
            int height = originalBitmap.PixelHeight;
            int stride = width * (originalBitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            originalBitmap.CopyPixels(pixelData, stride, 0);

            // LUT 생성
            byte[] lut = GenerateBezierLUT(controlPoint1, controlPoint2);

            // 픽셀 데이터 조정
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = lut[pixelData[i]];       // Blue
                pixelData[i + 1] = lut[pixelData[i + 1]]; // Green
                pixelData[i + 2] = lut[pixelData[i + 2]]; // Red
            }

            var adjustedBitmap = new WriteableBitmap(width, height, originalBitmap.DpiX, originalBitmap.DpiY, originalBitmap.Format, null);
            adjustedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            ImageDisplay.Source = adjustedBitmap;
        }

        private byte[] GenerateBezierLUT(double controlPoint1, double controlPoint2)
        {
            byte[] lut = new byte[256];
            Point p0 = new Point(0, 0);
            Point p1 = new Point(0.33, controlPoint1); // 사용자 제어 가능한 P1
            Point p2 = new Point(0.66, controlPoint2); // 사용자 제어 가능한 P2
            Point p3 = new Point(1, 1);

            for (int i = 0; i < 256; i++)
            {
                double t = i / 255.0;
                double bezierValue = CalculateBezier(t, p0, p1, p2, p3) * 255;
                lut[i] = (byte)Math.Max(0, Math.Min(255, bezierValue));
            }

            return lut;
        }

        private double CalculateBezier(double t, Point p0, Point p1, Point p2, Point p3)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;

            double x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
            double y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

            return y;
        }

    }
}