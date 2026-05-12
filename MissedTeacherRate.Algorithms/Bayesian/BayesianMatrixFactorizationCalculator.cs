using MissedTeacherRate.Models;

namespace MissedTeacherRate.Algorithms.Bayesian
{
    public class BayesianMatrixFactorizationCalculator : IMissedMarksCalculator
    {
        public string Name => "Bayesian Matrix Factorization";

        private const double Regularization = 2.0;

        public MarksMatrix CalculateMissedMarks(MarksMatrix matrix)
        {
            var result = new MarksMatrix(matrix);

            var works = matrix.GetWorks().ToList();
            var students = matrix.Students.ToList();
            var teacher = matrix.Teacher;

            if (teacher == null)
                return result;

            var allRatings = new List<double>();

            foreach (var work in works)
            {
                foreach (var student in students)
                {
                    var mark = matrix.GetMarkForWork(student, work);
                    if (mark.HasValue)
                        allRatings.Add(mark.Value);
                }

                var teacherMark = matrix.GetMarkForWork(teacher, work);
                if (teacherMark.HasValue)
                    allRatings.Add(teacherMark.Value);
            }

            if (allRatings.Count == 0)
                return result;

            double globalMean = allRatings.Average();

            var workBiases = new Dictionary<Work, double>();

            foreach (var work in works)
            {
                var workMarks = new List<double>();

                foreach (var student in students)
                {
                    var mark = matrix.GetMarkForWork(student, work);
                    if (mark.HasValue)
                        workMarks.Add(mark.Value);
                }

                var teacherMark = matrix.GetMarkForWork(teacher, work);
                if (teacherMark.HasValue)
                    workMarks.Add(teacherMark.Value);

                if (workMarks.Count == 0)
                {
                    workBiases[work] = 0;
                }
                else
                {
                    double sumDeviation = workMarks.Sum(x => x - globalMean);
                    workBiases[work] = sumDeviation / (Regularization + workMarks.Count);
                }
            }

            var knownTeacherMarks = works
                .Select(w => matrix.GetMarkForWork(teacher, w))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToList();

            double teacherBias = 0;

            if (knownTeacherMarks.Count > 0)
            {
                double sumDeviation = knownTeacherMarks.Sum(x => x - globalMean);
                teacherBias = sumDeviation / (Regularization + knownTeacherMarks.Count);
            }

            foreach (var work in works)
            {
                var teacherMark = matrix.GetMarkForWork(teacher, work);

                if (!teacherMark.HasValue)
                {
                    double prediction = globalMean + workBiases[work] + teacherBias;

                    prediction = Math.Min(10, Math.Max(1, prediction));
                    prediction = Math.Round(prediction, 1);

                    result.AddRating(teacher, work, prediction);
                }
            }

            return result;
        }
    }
}
