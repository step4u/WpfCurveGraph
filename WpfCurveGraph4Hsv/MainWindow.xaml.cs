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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace WpfCurveGraph4Hsv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage originalImage;
        private List<Rectangle> controlPoints = new List<Rectangle>();
        private List<Point> curvePoints = new List<Point>();
        private const int NumPoints = 256;
        private Rectangle? selectedPoint = null;

        private double ratio = 1.0d;

        public MainWindow()
        {
            InitializeComponent();

            ratio = CurvesCanvas.Width / 256;

            InitializeCurvePoints();
            DrawCurve();
        }

        private void InitializeCurvePoints()
        {
            curvePoints.Clear();
            for (int i = 0; i < NumPoints; i++)
            {
                curvePoints.Add(new Point(i * ratio, (NumPoints - i) * ratio));
            }
        }

        private void DrawCurve()
        {
            CurvesCanvas.Children.Clear();
            Polyline polyline = new Polyline
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
            };

            foreach (var point in curvePoints)
            {
                //polyline.Points.Add(new Point(point.X, point.Y * CurvesCanvas.Height / NumPoints));
                polyline.Points.Add(new Point(point.X, point.Y));
            }
            CurvesCanvas.Children.Add(polyline);

            // 제어점(클릭하여 조정할 수 있는 포인트) 추가
            controlPoints.Clear();
            for (int i = 0; i < NumPoints; i += 64)
            {
                var rect = new Rectangle
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                };

                //if (curvePoints[i].X == 0 && curvePoints[i].Y == 0)
                //{
                //    Canvas.SetLeft(rect, curvePoints[i].X);
                //    Canvas.SetTop(rect, curvePoints[i].Y);
                //}
                //else if (curvePoints[i].X == 256 && curvePoints[i].Y == 256)
                //{
                //    Canvas.SetLeft(rect, curvePoints[i].X);
                //    Canvas.SetTop(rect, curvePoints[i].Y);
                //}
                //else
                //{
                    Canvas.SetLeft(rect, curvePoints[i].X - rect.Width / 2);
                    Canvas.SetTop(rect, curvePoints[i].Y - rect.Height / 2);
                //}


                rect.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;

                CurvesCanvas.Children.Add(rect);
                controlPoints.Add(rect);
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            selectedPoint = sender as Rectangle;
        }

        private void CurvesCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 선택한 포인트가 없으면 새 포인트 생성
            if (selectedPoint == null)
            {
                Point clickedPosition = e.GetPosition(CurvesCanvas);
                var rect = new Rectangle
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                };
                Canvas.SetLeft(rect, clickedPosition.X - rect.Width / 2);
                Canvas.SetTop(rect, clickedPosition.Y - rect.Height / 2);

                rect.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;

                CurvesCanvas.Children.Add(rect);
                controlPoints.Add(rect);

                UpdateCurvePoints();
                DrawCurve();
            }
        }

        private void CurvesCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (selectedPoint != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(CurvesCanvas);

                double clampedX = Math.Max(0, Math.Min(mousePosition.X, CurvesCanvas.Width));
                double clampedY = Math.Max(0, Math.Min(mousePosition.Y, CurvesCanvas.Height));

                Canvas.SetLeft(selectedPoint, clampedX - selectedPoint.Width / 2);
                Canvas.SetTop(selectedPoint, clampedY - selectedPoint.Height / 2);

                UpdateCurvePoints();
                DrawCurve();
            }
        }

        private void UpdateCurvePoints()
        {
            foreach (var rect in controlPoints)
            {
                int x = (int)Canvas.GetLeft(rect) + (int)(rect.Width / 2);
                int y = (int)Canvas.GetTop(rect) + (int)(rect.Height / 2);

                int index = (int)(x * NumPoints / CurvesCanvas.Width);
                curvePoints[index] = new Point(index, (int)(y * NumPoints / CurvesCanvas.Height));
            }

            // 곡선 포인트를 부드럽게 만들려면 여기서 보간(Bezier Curve 등)을 적용할 수 있음
        }

        private void ApplyCurves_Click(object sender, RoutedEventArgs e)
        {
            if (originalImage == null)
                return;

            WriteableBitmap wb = new WriteableBitmap(originalImage);
            int width = wb.PixelWidth;
            int height = wb.PixelHeight;
            int stride = width * (wb.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            wb.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i += 4)
            {
                byte b = pixelData[i];
                byte g = pixelData[i + 1];
                byte r = pixelData[i + 2];

                pixelData[i] = ApplyCurve(b); // B
                pixelData[i + 1] = ApplyCurve(g); // G
                pixelData[i + 2] = ApplyCurve(r); // R
            }

            wb.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            ImageDisplay.Source = wb;
        }

        private byte ApplyCurve(byte value)
        {
            int index = Math.Clamp((int)value, 0, 255);
            return (byte)Math.Clamp(curvePoints[index].Y, 0, 255);
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.png)|*.jpg;*.png";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                originalImage = new BitmapImage(new Uri(openFileDialog.FileName));
                ImageDisplay.Source = originalImage;
            }
        }

    }
}