using MissedTeacherRate.Models;
using MissedTeacherRate.Models;

namespace MissedTeacherRate.Parsers
{
    public interface IRatingMatrixParser
    {
        string Name { get; }
        string FileFilter { get; }
        MarksMatrix Parse(Stream stream);
    }
}
