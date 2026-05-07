using MissedTeacherRate.Models;

namespace MissedTeacherRate.Parsers
{
    public interface IRatingMatrixParser
    {
        public MarksMatrix Parse(Stream stream);
    }
}
