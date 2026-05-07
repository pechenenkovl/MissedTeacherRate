using MissedTeacherRate.Models;

namespace MissedTeacherRate.Algorithms
{
    public interface IMissedMarksCalculator
    {
        string Name { get; }

        MarksMatrix CalculateMissedMarks(MarksMatrix matrix);
    }
}
