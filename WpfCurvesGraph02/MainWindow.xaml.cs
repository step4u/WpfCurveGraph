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

namespace WpfCurvesGraph02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Point> controlPoints = new List<Point>();
        private Path curvePath;
        private const int CANVAS_SIZE = 256;
        private const int CONTROL_POINT_RADIUS = 5;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCurveCanvas();
        }

        private void InitializeCurveCanvas()
        {
            Canvas curveCanvas = new Canvas
            {
                Width = CANVAS_SIZE,
                Height = CANVAS_SIZE,
                Background = Brushes.White
            };

            // Add grid lines
            for (int i = 0; i <= CANVAS_SIZE; i += CANVAS_SIZE / 4)
            {
                curveCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = i,
                    X2 = CANVAS_SIZE,
                    Y2 = i,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                });

                curveCanvas.Children.Add(new Line
                {
                    X1 = i,
                    Y1 = 0,
                    X2 = i,
                    Y2 = CANVAS_SIZE,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                });
            }

            // Add diagonal line
            curveCanvas.Children.Add(new Line
            {
                X1 = 0,
                Y1 = CANVAS_SIZE,
                X2 = CANVAS_SIZE,
                Y2 = 0,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            });

            curvePath = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            curveCanvas.Children.Add(curvePath);

            curveCanvas.MouseLeftButtonDown += CurveCanvas_MouseLeftButtonDown;
            curveCanvas.MouseMove += CurveCanvas_MouseMove;

            Content = curveCanvas;

            // Initialize with default curve (diagonal line)
            controlPoints.Add(new Point(0, CANVAS_SIZE));
            controlPoints.Add(new Point(CANVAS_SIZE / 2, CANVAS_SIZE / 2));
            controlPoints.Add(new Point(CANVAS_SIZE, 0));
            UpdateCurve();
        }

        private void CurveCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition((IInputElement)sender);
            int index = FindNearestControlPointIndex(clickPoint);

            if (index == -1 && controlPoints.Count < 10) // Limit the number of control points
            {
                controlPoints.Add(clickPoint);
            }
            else if (index != -1)
            {
                controlPoints[index] = clickPoint;
            }

            controlPoints = controlPoints.OrderBy(p => p.X).ToList();
            UpdateCurve();
        }

        private void CurveCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dragPoint = e.GetPosition((IInputElement)sender);
                int index = FindNearestControlPointIndex(dragPoint);

                if (index != -1)
                {
                    controlPoints[index] = dragPoint;
                    controlPoints = controlPoints.OrderBy(p => p.X).ToList();
                    UpdateCurve();
                }
            }
        }

        private int FindNearestControlPointIndex(Point point)
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                if (Point.Subtract(controlPoints[i], point).Length < CONTROL_POINT_RADIUS)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateCurve()
        {
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = controlPoints.First();

            for (int i = 1; i < controlPoints.Count; i += 3)
            {
                if (i + 2 < controlPoints.Count)
                {
                    pathFigure.Segments.Add(new BezierSegment(
                        controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], true));
                }
                else if (i + 1 < controlPoints.Count)
                {
                    pathFigure.Segments.Add(new QuadraticBezierSegment(
                        controlPoints[i], controlPoints[i + 1], true));
                }
                else
                {
                    pathFigure.Segments.Add(new LineSegment(controlPoints[i], true));
                }
            }

            pathGeometry.Figures.Add(pathFigure);
            curvePath.Data = pathGeometry;

            // Redraw control points
            Canvas canvas = (Canvas)Content;

            // Remove existing control point visuals
            for (int i = canvas.Children.Count - 1; i >= 0; i--)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    canvas.Children.RemoveAt(i);
                }
            }

            // Add new control point visuals
            foreach (var point in controlPoints)
            {
                canvas.Children.Add(new Ellipse
                {
                    Width = CONTROL_POINT_RADIUS * 2,
                    Height = CONTROL_POINT_RADIUS * 2,
                    Fill = Brushes.Red,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Margin = new Thickness(point.X - CONTROL_POINT_RADIUS, point.Y - CONTROL_POINT_RADIUS, 0, 0)
                });
            }

            // Here you would apply the curve to your image processing
            // This is where you'd implement the actual image adjustment based on the curve
        }

    }
}