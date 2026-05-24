using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MissedTeacherRate.WPF.Controls
{
    public class TrustGraphControl : Canvas
    {
        private List<Trust<Person, Person>>? _trusts;
        private Dictionary<Person, Point> _nodePositions = new();
        private const double NodeRadius = 25;
        private const double Padding = 40;
        private const double TeacherY = 60;
        private const double StudentY = 180;
        private const double ArcBaseY = 230;
        private const double ArcSpacing = 25;

        public void SetTrusts(List<Trust<Person, Person>> trusts)
        {
            _trusts = trusts;
            InvalidateVisual();
            Loaded += (s, e) => DrawGraph();
            SizeChanged += (s, e) => DrawGraph();
        }

        private void DrawGraph()
        {
            Children.Clear();
            _nodePositions.Clear();

            if (_trusts == null || _trusts.Count == 0 || ActualWidth == 0 || ActualHeight == 0)
                return;

            // Get unique persons
            var persons = _trusts
                .SelectMany(t => new[] { t.First, t.Second })
                .Distinct()
                .ToList();

            if (persons.Count == 0)
                return;

            // Separate teachers and students
            var teachers = persons.Where(p => p is Teacher).ToList();
            var students = persons.Where(p => p is Student).ToList();

            // Calculate positions - hierarchical layout
            CalculateHierarchicalLayout(teachers, students);

            // Get student-student trusts for arc drawing (sorted by distance for stacking)
            var studentTrusts = _trusts
                .Where(t => t.First is Student && t.Second is Student)
                .OrderBy(t => GetStudentDistance(t, students))
                .ToList();

            // Get teacher-student trusts
            var teacherStudentTrusts = _trusts
                .Where(t => t.First is Teacher || t.Second is Teacher)
                .ToList();

            // Draw student-student arcs first (below students)
            DrawStudentArcs(studentTrusts, students);

            // Draw teacher-student lines (straight lines)
            DrawTeacherStudentLines(teacherStudentTrusts);

            // Draw nodes on top
            DrawNodes(persons);
        }

        private void CalculateHierarchicalLayout(List<Person> teachers, List<Person> students)
        {
            double centerX = ActualWidth / 2;

            // Place teacher(s) at top center
            if (teachers.Count == 1)
            {
                _nodePositions[teachers[0]] = new Point(centerX, TeacherY);
            }
            else
            {
                double teacherSpacing = Math.Min(100, (ActualWidth - 2 * Padding) / Math.Max(1, teachers.Count - 1));
                double teacherStartX = centerX - (teachers.Count - 1) * teacherSpacing / 2;
                for (int i = 0; i < teachers.Count; i++)
                {
                    _nodePositions[teachers[i]] = new Point(teacherStartX + i * teacherSpacing, TeacherY);
                }
            }

            // Place students in a horizontal line below
            if (students.Count > 0)
            {
                double availableWidth = ActualWidth - 2 * Padding - 2 * NodeRadius;
                double studentSpacing = students.Count > 1 
                    ? Math.Min(80, availableWidth / (students.Count - 1)) 
                    : 0;
                double studentStartX = centerX - (students.Count - 1) * studentSpacing / 2;

                for (int i = 0; i < students.Count; i++)
                {
                    _nodePositions[students[i]] = new Point(studentStartX + i * studentSpacing, StudentY);
                }
            }
        }

        private int GetStudentDistance(Trust<Person, Person> trust, List<Person> students)
        {
            int idx1 = students.IndexOf(trust.First);
            int idx2 = students.IndexOf(trust.Second);
            return Math.Abs(idx2 - idx1);
        }

        private void DrawStudentArcs(List<Trust<Person, Person>> studentTrusts, List<Person> students)
        {
            int arcLevel = 0;

            foreach (var trust in studentTrusts)
            {
                var start = _nodePositions[trust.First];
                var end = _nodePositions[trust.Second];

                // Start and end points at bottom of circles
                var startPoint = new Point(start.X, start.Y + NodeRadius);
                var endPoint = new Point(end.X, end.Y + NodeRadius);

                // Calculate the Y level for this connection (stacked below previous)
                double bottomY = ArcBaseY + arcLevel * ArcSpacing;

                // Create angular path: |__|
                // Point 1: Start (bottom of first circle)
                // Point 2: Go down to the bottom level
                // Point 3: Go horizontally to below the second circle
                // Point 4: Go up to bottom of second circle

                var point1 = startPoint;
                var point2 = new Point(startPoint.X, bottomY);
                var point3 = new Point(endPoint.X, bottomY);
                var point4 = endPoint;

                // Draw the three line segments
                var brush = trust.Type == TrustType.Direct
                    ? new SolidColorBrush(Color.FromRgb(34, 139, 34))   // Forest Green
                    : new SolidColorBrush(Color.FromRgb(220, 20, 60)); // Crimson Red

                var dashArray = trust.Type == TrustType.Indirect
                    ? new DoubleCollection { 4, 2 }
                    : null;

                // Vertical line down from first student
                var line1 = new Line
                {
                    X1 = point1.X, Y1 = point1.Y,
                    X2 = point2.X, Y2 = point2.Y,
                    Stroke = brush,
                    StrokeThickness = 2,
                    StrokeDashArray = dashArray
                };
                Children.Add(line1);

                // Horizontal line at bottom
                var line2 = new Line
                {
                    X1 = point2.X, Y1 = point2.Y,
                    X2 = point3.X, Y2 = point3.Y,
                    Stroke = brush,
                    StrokeThickness = 2,
                    StrokeDashArray = dashArray
                };
                Children.Add(line2);

                // Vertical line up to second student
                var line3 = new Line
                {
                    X1 = point3.X, Y1 = point3.Y,
                    X2 = point4.X, Y2 = point4.Y,
                    Stroke = brush,
                    StrokeThickness = 2,
                    StrokeDashArray = dashArray
                };
                Children.Add(line3);

                // Draw trust value label on the horizontal segment (centered)
                var labelPos = new Point((point2.X + point3.X) / 2, bottomY - 8);
                DrawTrustLabel(trust, labelPos);

                arcLevel++;
            }
        }

        private void DrawTeacherStudentLines(List<Trust<Person, Person>> teacherStudentTrusts)
        {
            foreach (var trust in teacherStudentTrusts)
            {
                var start = _nodePositions[trust.First];
                var end = _nodePositions[trust.Second];

                // Determine which is teacher and which is student
                Point teacherPos, studentPos;
                if (trust.First is Teacher)
                {
                    teacherPos = start;
                    studentPos = end;
                }
                else
                {
                    teacherPos = end;
                    studentPos = start;
                }

                // Calculate line endpoints at circle edges
                var direction = studentPos - teacherPos;
                direction.Normalize();

                var lineStart = teacherPos + direction * NodeRadius;
                var lineEnd = studentPos - direction * NodeRadius;

                // Draw straight line
                var line = new Line
                {
                    X1 = lineStart.X,
                    Y1 = lineStart.Y,
                    X2 = lineEnd.X,
                    Y2 = lineEnd.Y,
                    Stroke = trust.Type == TrustType.Direct
                        ? new SolidColorBrush(Color.FromRgb(34, 139, 34))   // Forest Green
                        : new SolidColorBrush(Color.FromRgb(220, 20, 60)),  // Crimson Red
                    StrokeThickness = 2,
                    StrokeDashArray = trust.Type == TrustType.Indirect
                        ? new DoubleCollection { 4, 2 }
                        : null
                };
                Children.Add(line);

                // Draw trust value label at midpoint, offset to the side
                var midPoint = new Point(
                    (lineStart.X + lineEnd.X) / 2,
                    (lineStart.Y + lineEnd.Y) / 2);

                // Offset perpendicular to the line
                var perpendicular = new Vector(-direction.Y, direction.X);
                perpendicular.Normalize();
                var labelPos = midPoint + perpendicular * 15;

                DrawTrustLabel(trust, labelPos);
            }
        }

        private void DrawTrustLabel(Trust<Person, Person> trust, Point position)
        {
            var label = new TextBlock
            {
                Text = trust.Value?.ToString("0.##") ?? "N/A",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = trust.Type == TrustType.Direct
                    ? new SolidColorBrush(Color.FromRgb(34, 139, 34))
                    : new SolidColorBrush(Color.FromRgb(220, 20, 60)),
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                Padding = new Thickness(2)
            };

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            SetLeft(label, position.X - label.DesiredSize.Width / 2);
            SetTop(label, position.Y - label.DesiredSize.Height / 2);
            Children.Add(label);
        }

        private void DrawNodes(List<Person> persons)
        {
            foreach (var person in persons)
            {
                var pos = _nodePositions[person];

                // Draw circle
                var ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Fill = person is Teacher
                        ? new SolidColorBrush(Color.FromRgb(65, 105, 225))   // Royal Blue for Teacher
                        : new SolidColorBrush(Color.FromRgb(255, 165, 0)),   // Orange for Student
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                SetLeft(ellipse, pos.X - NodeRadius);
                SetTop(ellipse, pos.Y - NodeRadius);
                Children.Add(ellipse);

                // Draw label inside circle
                var label = new TextBlock
                {
                    Text = person.Id,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                SetLeft(label, pos.X - label.DesiredSize.Width / 2);
                SetTop(label, pos.Y - label.DesiredSize.Height / 2);
                Children.Add(label);
            }
        }
    }
}
