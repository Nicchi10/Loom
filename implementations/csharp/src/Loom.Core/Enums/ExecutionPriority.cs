namespace Loom.Core.Enums
{
    /// <summary>
    /// Specifies priority optimization for request execution
    /// </summary>
    public enum ExecutionPriority
    {
        /// <summary>
        /// Tradeoff between cost and speed
        /// </summary>
        Balanced = 0,

        /// <summary>
        /// Priority to speed response
        /// </summary>
        LatencyOptimized = 1,

        /// <summary>
        /// Priority to savings
        /// </summary>
        CostOptimized = 2,

        /// <summary>
        /// Priority to max accuracy
        /// </summary>
        QualityOptimized = 3
    }
}
