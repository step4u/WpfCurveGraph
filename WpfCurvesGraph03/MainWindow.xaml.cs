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

namespace WpfCurvesGraph03
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
            // 이미지 로드 예시 (로컬 이미지 경로)
            var bitmap = new BitmapImage(new Uri("D:\\Project\\CSI\\도장분석\\Data\\KS-11.png", UriKind.Relative));
            originalBitmap = new WriteableBitmap(bitmap);
            ImageDisplay.Source = originalBitmap;
        }

        private void CurveSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyCurves((int)e.NewValue);
        }

        private void ApplyCurves(int adjustment)
        {
            if (originalBitmap == null) return;

            int width = originalBitmap.PixelWidth;
            int height = originalBitmap.PixelHeight;
            int stride = width * (originalBitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            originalBitmap.CopyPixels(pixelData, stride, 0);

            // Lookup Table(LUT) 생성
            byte[] lut = GenerateLUT(adjustment);

            // 픽셀 데이터 조정
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                // BGR 순서로 조정
                pixelData[i] = lut[pixelData[i]];       // Blue
                pixelData[i + 1] = lut[pixelData[i + 1]]; // Green
                pixelData[i + 2] = lut[pixelData[i + 2]]; // Red
            }

            // WriteableBitmap에 업데이트
            var adjustedBitmap = new WriteableBitmap(width, height, originalBitmap.DpiX, originalBitmap.DpiY, originalBitmap.Format, null);
            adjustedBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            ImageDisplay.Source = adjustedBitmap;
        }

        private byte[] GenerateLUT(int adjustment)
        {
            byte[] lut = new byte[256];

            double curveFactor = (100.0 + adjustment) / 100.0;
            double midPoint = 127.5;

            for (int i = 0; i < 256; i++)
            {
                double newValue = midPoint + (i - midPoint) * curveFactor;
                lut[i] = (byte)Math.Max(0, Math.Min(255, newValue));
            }

            return lut;
        }


    }
}