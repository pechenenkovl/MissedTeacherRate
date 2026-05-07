using ComputeTrust.Algorithms;
using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;

namespace MissedTeacherRate.Algorithms.COMAS
{
    public class ComasAlgorithmCalculator : IMissedMarksCalculator
    {
        public string Name => "COMAS";

        public MarksMatrix CalculateMissedMarks(MarksMatrix matrix)
        {
            var teacherToStudentsDirectTrust = DirectTrustCalculation.ComputeTeacherToStudentsTrust(matrix);
            var studentsDirectTrust = DirectTrustCalculation.ComputeStudentsTrust(matrix);

            IndirectTrustCalculation.FillIndirectTrusts(teacherToStudentsDirectTrust, studentsDirectTrust);

            var resultMatrix = new MarksMatrix(matrix);
            FillMissedTeacherMarks(resultMatrix, teacherToStudentsDirectTrust);
            return resultMatrix;
        }

        private void FillMissedTeacherMarks(
            MarksMatrix matrix,
            IEnumerable<Trust<Teacher, Student>> teacherToStudentTrust,
            double omega = 3.0)
        {
            var teacher = matrix.Teacher;
            if (teacher == null) return;

            foreach (var work in matrix.GetWorks())
            {
                // If teacher already rated, skip
                if (matrix.IsPersonRatedWork(teacher, work))
                    continue;

                double numerator = 0.0;
                double denominator = 0.0;
                foreach (var student in matrix.Students)
                {
                    var studentMark = matrix.GetMarkForWork(student, work);
                    // Use Trust<Teacher, Student> to get trust value
                    double? trust = teacherToStudentTrust.FirstOrDefault(t => t.IsBetween(teacher, student))?.Value;
                    if (trust == null)
                    {
                        throw new InvalidOperationException($"There is no direct/indirect trust between teacher {teacher.Id} and student {student.Id}");
                    }

                    if (studentMark.HasValue)
                    {
                        var trustPow = Math.Pow(trust.Value, omega);
                        numerator += studentMark.Value * trustPow;
                        denominator += trustPow;
                    }
                }

                if (denominator > 0)
                {
                    var estimatedMark = numerator / denominator;
                    // Optionally round or clamp to 1-10
                    var finalMark = Math.Clamp(estimatedMark, 1, 10);
                    matrix.AddRating(teacher, work, finalMark);
                }
            }
        }
    }
}
