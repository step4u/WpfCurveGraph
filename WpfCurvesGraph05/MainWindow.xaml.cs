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

namespace WpfCurvesGraph05
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WriteableBitmap originalBitmap;
        private bool isDragging = false;
        private Ellipse activeControlPoint;

        public MainWindow()
        {
            InitializeComponent();

            LoadImage();
            InitializeControlPoints();
            DrawBezierCurve();
        }

        private void LoadImage()
        {
            var bitmap = new BitmapImage(new Uri("D:\\Project\\CSI\\도장분석\\Data\\KS-11.png", UriKind.Relative));
            originalBitmap = new WriteableBitmap(bitmap);
            ImageDisplay.Source = originalBitmap;
        }

        private void InitializeControlPoints()
        {
            // 초기 제어점 위치 설정 (그래프 기준)
            SetControlPointPosition(ControlPoint1, 0.33, 0.33);
            SetControlPointPosition(ControlPoint2, 0.66, 0.66);
        }

        private void SetControlPointPosition(Ellipse controlPoint, double xRatio, double yRatio)
        {
            double x = xRatio * GraphCanvas.Width - controlPoint.Width / 2;
            double y = (1 - yRatio) * GraphCanvas.Height - controlPoint.Height / 2;
            Canvas.SetLeft(controlPoint, x);
            Canvas.SetTop(controlPoint, y);
        }

        private void ControlPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            activeControlPoint = sender as Ellipse;
            activeControlPoint.CaptureMouse();
        }

        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && activeControlPoint != null)
            {
                Point position = e.GetPosition(GraphCanvas);

                // ControlPoint 위치 제한 (Canvas 내)
                double x = Math.Max(0, Math.Min(GraphCanvas.Width - activeControlPoint.Width, position.X - activeControlPoint.Width / 2));
                double y = Math.Max(0, Math.Min(GraphCanvas.Height - activeControlPoint.Height, position.Y - activeControlPoint.Height / 2));

                Canvas.SetLeft(activeControlPoint, x);
                Canvas.SetTop(activeControlPoint, y);

                DrawBezierCurve();
                UpdateImage();
            }
        }

        private void ControlPoint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            activeControlPoint.ReleaseMouseCapture();
            activeControlPoint = null;
        }

        private void DrawBezierCurve()
        {
            BezierCurve.Points.Clear();

            Point p0 = new Point(0, GraphCanvas.Height);
            Point p1 = GetControlPointPosition(ControlPoint1);
            Point p2 = GetControlPointPosition(ControlPoint2);
            Point p3 = new Point(GraphCanvas.Width, 0);

            // 곡선 그리기 (PolyLine에 점 추가)
            for (int i = 0; i <= 100; i++)
            {
                double t = i / 100.0;
                Point point = CalculateBezierPoint(t, p0, p1, p2, p3);
                BezierCurve.Points.Add(point);
            }
        }

        private Point GetControlPointPosition(Ellipse controlPoint)
        {
            double x = Canvas.GetLeft(controlPoint) + controlPoint.Width / 2;
            double y = Canvas.GetTop(controlPoint) + controlPoint.Height / 2;
            return new Point(x, y);
        }

        private Point CalculateBezierPoint(double t, Point p0, Point p1, Point p2, Point p3)
        {
            double u = 1 - t;
            double tt = t * t;
            double uu = u * u;
            double uuu = uu * u;
            double ttt = tt * t;

            double x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
            double y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

            return new Point(x, y);
        }

        private void UpdateImage()
        {
            if (originalBitmap == null) return;

            int width = originalBitmap.PixelWidth;
            int height = originalBitmap.PixelHeight;
            int stride = width * (originalBitmap.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            originalBitmap.CopyPixels(pixelData, stride, 0);

            // ControlPoint 위치를 기반으로 LUT 생성
            byte[] lut = GenerateBezierLUT();

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

        private byte[] GenerateBezierLUT()
        {
            byte[] lut = new byte[256];

            // 제어점 좌표 가져오기 (0-1 비율로 변환)
            Point p1 = GetControlPointPosition(ControlPoint1);
            Point p2 = GetControlPointPosition(ControlPoint2);

            Point p0 = new Point(0, 1);
            Point p3 = new Point(1, 0);

            p1 = new Point(p1.X / GraphCanvas.Width, 1 - (p1.Y / GraphCanvas.Height));
            p2 = new Point(p2.X / GraphCanvas.Width, 1 - (p2.Y / GraphCanvas.Height));

            for (int i = 0; i < 256; i++)
            {
                double t = i / 255.0;
                double bezierValue = CalculateBezierPoint(t, p0, p1, p2, p3).Y * 255;
                lut[i] = (byte)Math.Max(0, Math.Min(255, bezierValue));
            }

            return lut;
        }

    }
}