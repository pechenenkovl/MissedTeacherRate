using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;

namespace ComputeTrust.Algorithms
{
    internal static class DirectTrustCalculation
    {
        // Trust between teacher and each student
        public static List<Trust<Teacher, Student>> ComputeTeacherToStudentsTrust(MarksMatrix matrix)
        {
            var teacher = matrix.Teacher;
            if (teacher == null)
                throw new InvalidOperationException("Teacher must be set in the matrix.");
            var result = new List<Trust<Teacher, Student>>();

            foreach (var student in matrix.Students)
            {
                var trust = new Trust<Teacher, Student>(teacher, student);
                trust.Value = ComputeDirectTrust(matrix, teacher, student);
                trust.Type = TrustType.Direct;
                result.Add(trust);
            }
            return result;
        }

        // Trust between each students
        public static List<Trust<Student, Student>> ComputeStudentsTrust(MarksMatrix matrix)
        {
            var result = new List<Trust<Student, Student>>();
            var studentsArr = matrix.Students.ToArray();

            for (int i = 0; i < studentsArr.Length; i++)
            {
                for (int j = i + 1; j < studentsArr.Length; j++)
                {
                    var student1 = studentsArr[i];
                    var student2 = studentsArr[j];
                    var trustValue = ComputeDirectTrust(matrix, student1, student2);
                    if (trustValue == null)
                    {
                        continue;
                    }
                    var trust = new Trust<Student, Student>(student1, student2);
                    trust.Type = TrustType.Direct;
                    trust.Value = trustValue;
                    result.Add(trust);
                }
            }

            return result;
        }


        // Trust between any two persons (student/student, teacher/student, etc.)
        private static double? ComputeDirectTrust(MarksMatrix matrix, Person p1, Person p2)
        {
            var works = matrix.GetWorks();
            var trusts = new List<double>();

            foreach (var work in works)
            {
                var mark1 = matrix.GetMarkForWork(p1, work);
                var mark2 = matrix.GetMarkForWork(p2, work);

                if (mark1.HasValue && mark2.HasValue)
                {
                    double trust = 1 - Math.Abs(mark1.Value - mark2.Value) / 10.0;
                    trusts.Add(trust);
                }
            }

            return trusts.Count > 0 ? trusts.Average() : null;
        }
    }
}
