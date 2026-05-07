namespace MissedTeacherRate.Models
{
    public abstract record Person(string Id);

    public record Student(string Id) : Person(Id);

    public record Teacher(string Id) : Person(Id);

    public record Work(string Id);

    public record Rating(Person Rater, Work Work, double Score);

    public class MarksMatrix
    {
        private readonly List<Student> _students = [];
        private Teacher? _teacher;
        private readonly List<Work> _works = [];
        private readonly List<Rating> _ratings = [];

        public MarksMatrix() { }

        public MarksMatrix(MarksMatrix matrix)
        {
            _students = new List<Student>(matrix._students);
            _teacher = matrix._teacher;
            _works = new List<Work>(matrix._works);

            // Clone ratings: new Rating objects, but reference existing Person and Work
            _ratings = matrix._ratings
                .Select(r => new Rating(r.Rater, r.Work, r.Score))
                .ToList();
        }

        public IEnumerable<Student> Students => _students;

        public Teacher? Teacher => _teacher;

        public void AddStudents(params Student[] students) => _students.AddRange(students);

        public void SetTeacher(Teacher teacher) => _teacher = teacher;

        public void AddWorks(params Work[] works) => _works.AddRange(works);

        public void AddRating(Person person, Work work, double score)
        {
            if (score < 1 || score > 10)
                throw new ArgumentOutOfRangeException(nameof(score), "Score must be between 1 and 10.");
            if (!_works.Contains(work))
                throw new ArgumentException("Work not found.");
            if (!GetRaters().Contains(person))
            {
                throw new ArgumentException("Person not found among raters.");
            }
            _ratings.Add(new Rating(person, work, score));
        }

        public double? GetMarkForWork(Person person, Work work)
        {
            var rating = _ratings.FirstOrDefault(r => r.Rater == person && r.Work == work);
            return rating?.Score;
        }

        public bool IsPersonRatedWork(Person person, Work work)
        {
            return _ratings.Any(r => r.Rater == person && r.Work == work);
        }

        public IEnumerable<Work> GetWorks() => _works;

        public IEnumerable<Person> GetRaters()
        {
            var raters = _students.Cast<Person>().ToList();
            if (_teacher != null)
                raters.Add(_teacher);
            return raters;
        }

        public void PrintMatrix()
        {
            var raters = GetRaters().ToList();

            Console.Write("      ");
            foreach (var rater in raters)
                Console.Write($"{rater.Id,10}");
            Console.WriteLine();

            foreach (var work in _works)
            {
                Console.Write($"{work.Id,-6}");
                foreach (var rater in raters)
                {
                    var rating = _ratings.FirstOrDefault(r => r.Work == work && r.Rater == rater);
                    Console.Write($"{(rating != null ? rating.Score.ToString("0.#") : ""),10}");
                }
                Console.WriteLine();
            }
        }
    }
}
