using MissedTeacherRate.Models;
using System.Text.RegularExpressions;

namespace MissedTeacherRate.Parsers
{
    public class LatexMatrixParser : IRatingMatrixParser
    {
        public string Name => "LaTeX";
        public string FileFilter => "LaTeX Files|*.tex";

        public MarksMatrix Parse(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var matrix = new MarksMatrix();

            // Find the first table environment
            var tableMatch = Regex.Match(content, @"\\begin\{tabular\}\{[^}]*\}(.*?)\\end\{tabular\}", RegexOptions.Singleline);
            if (!tableMatch.Success)
            {
                throw new FormatException("No tabular environment found in the LaTeX file.");
            }

            var tableContent = tableMatch.Groups[1].Value;

            // Split into rows by \\ (LaTeX row separator)
            var rows = Regex.Split(tableContent, @"\\\\")
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => Regex.Replace(r, @"\\hline", "").Trim()) // Remove \hline
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (rows.Count < 2)
            {
                throw new FormatException("Table must have at least a header row and one data row.");
            }

            // Parse header row (first row contains column headers: empty, S1, S2, ..., P)
            var headerCells = ParseRow(rows[0]);

            // Skip first empty cell, get person IDs
            var personIds = headerCells
                .Skip(1) // Skip the empty first cell
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();

            if (personIds.Count == 0)
            {
                throw new FormatException("No person IDs found in header row.");
            }

            // Last person is teacher, rest are students
            var studentIds = personIds.Take(personIds.Count - 1).ToArray();
            var teacherId = personIds.Last();

            matrix.AddStudents(studentIds.Select(id => new Student(id)).ToArray());
            matrix.SetTeacher(new Teacher(teacherId));

            // Parse data rows
            var workIds = new List<string>();
            var dataRows = rows.Skip(1).ToList();

            foreach (var row in dataRows)
            {
                var cells = ParseRow(row);
                if (cells.Count == 0) continue;

                var workId = cells[0].Trim();
                if (string.IsNullOrWhiteSpace(workId)) continue;

                workIds.Add(workId);
            }

            var works = workIds.Select(id => new Work(id)).ToArray();
            matrix.AddWorks(works);

            // Parse ratings
            for (int i = 0; i < dataRows.Count && i < works.Length; i++)
            {
                var cells = ParseRow(dataRows[i]);
                if (cells.Count <= 1) continue;

                var work = works[i];

                // Parse student ratings
                for (int j = 0; j < studentIds.Length && j + 1 < cells.Count; j++)
                {
                    var cellValue = cells[j + 1].Trim();
                    if (TryParseRating(cellValue, out double score))
                    {
                        matrix.AddRating(new Student(studentIds[j]), work, score);
                    }
                }

                // Parse teacher rating (last column)
                if (studentIds.Length + 1 < cells.Count && matrix.Teacher != null)
                {
                    var cellValue = cells[studentIds.Length + 1].Trim();
                    if (TryParseRating(cellValue, out double score))
                    {
                        matrix.AddRating(matrix.Teacher, work, score);
                    }
                }
            }

            return matrix;
        }

        private static List<string> ParseRow(string row)
        {
            // Split by & (LaTeX column separator)
            return row.Split('&')
                .Select(c => c.Trim())
                .ToList();
        }

        private static bool TryParseRating(string value, out double score)
        {
            score = 0;

            // Ignore empty cells and "-" (missing values)
            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return false;

            // Try to parse as number
            if (double.TryParse(value, out score))
            {
                // Validate score range
                return score >= 1 && score <= 10;
            }

            return false;
        }
    }
}
