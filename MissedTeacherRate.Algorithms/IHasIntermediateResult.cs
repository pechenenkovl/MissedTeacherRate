namespace MissedTeacherRate.Algorithms
{
    /// <summary>
    /// Non-generic interface for calculators that expose intermediate results.
    /// Use this for runtime checks (e.g., if calculator is IHasIntermediateResult).
    /// </summary>
    public interface IHasIntermediateResult
    {
        /// <summary>
        /// Gets the intermediate result after calculation.
        /// Returns null if no calculation has been performed yet.
        /// </summary>
        object? IntermediateResult { get; }

        /// <summary>
        /// Gets the display name for the intermediate result (e.g., "Trust Graph").
        /// </summary>
        string IntermediateResultName { get; }
    }

    /// <summary>
    /// Generic interface for calculators that expose typed intermediate results.
    /// Provides type-safe access to the intermediate result.
    /// </summary>
    /// <typeparam name="T">The type of the intermediate result.</typeparam>
    public interface IHasIntermediateResult<T> : IHasIntermediateResult
    {
        /// <summary>
        /// Gets the typed intermediate result after calculation.
        /// Returns default if no calculation has been performed yet.
        /// </summary>
        new T? IntermediateResult { get; }

        // Default implementation for non-generic interface
        object? IHasIntermediateResult.IntermediateResult => IntermediateResult;
    }
}
