using MissedTeacherRate.Models;
using OfficeOpenXml;

namespace MissedTeacherRate.Parsers
{
    namespace MissedTeacherRate.Parsers
    {
        public class ExcelMatrixParser : IRatingMatrixParser
        {
            public MarksMatrix Parse(Stream excelStream)
            {
                ExcelPackage.License.SetNonCommercialPersonal("Vladyslav Pechenenko, PhD student");

                using var package = new ExcelPackage(excelStream);
                var worksheet = package.Workbook.Worksheets[0];

                var matrix = new MarksMatrix();

                var ids = new List<string>();
                int col = 2;
                while (true)
                {
                    var cellValue = worksheet.Cells[1, col].Text?.Trim();
                    if (string.IsNullOrEmpty(cellValue)) break;
                    ids.Add(cellValue);
                    col++;
                }

                string[] studentIds = [];
                if (ids.Count > 0)
                {
                    // All except last are students
                    studentIds = ids.Take(ids.Count - 1).ToArray();
                    var teacherId = ids.Last();

                    matrix.AddStudents(studentIds.Select(id => new Student(id)).ToArray());
                    matrix.SetTeacher(new Teacher(teacherId));
                }

                // 3. Parse work IDs from first column (A2 to An)
                var workIds = new List<string>();
                int row = 2;
                while (true)
                {
                    var cellValue = worksheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(cellValue)) break;
                    workIds.Add(cellValue);
                    row++;
                }

                var works = workIds.Select(id => new Work(id)).ToArray();
                matrix.AddWorks(works);

                // 4. Parse ratings (matrix cells)
                for (int i = 0; i < works.Length; i++)
                {
                    var work = works[i];
                    // Students
                    for (int j = 0; j < studentIds.Length; j++)
                    {
                        var cell = worksheet.Cells[i + 2, j + 2].Text?.Trim();
                        if (!string.IsNullOrEmpty(cell) && cell != "-")
                        {
                            if (double.TryParse(cell, out double score))
                            {
                                if (score >= 1 && score <= 10)
                                    matrix.AddRating(new Student(studentIds[j]), work, score);
                            }
                        }
                    }
                    // Teacher
                    if (matrix.Teacher != null)
                    {
                        var cell = worksheet.Cells[i + 2, studentIds.Length + 2].Text?.Trim();
                        if (!string.IsNullOrEmpty(cell) && cell != "-")
                        {
                            if (double.TryParse(cell, out double score))
                            {
                                if (score >= 1 && score <= 10)
                                    matrix.AddRating(matrix.Teacher, work, score);
                            }
                        }
                    }
                }

                return matrix;
            }
        }
    }
}
