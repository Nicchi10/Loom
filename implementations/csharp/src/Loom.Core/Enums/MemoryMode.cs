namespace Loom.Core.Enums
{
    /// <summary>
    /// Defines how conversation history is processed
    /// </summary>
    public enum MemoryMode
    {
        /// <summary>
        /// No memory, only last message is sent
        /// </summary>
        None = 0,

        /// <summary>
        /// Every message is sent (full buffer) until TokenBudget runs out
        /// </summary>
        FullHistory = 1,

        /// <summary>
        /// All messages are condensed before being sent
        /// </summary>
        Summary = 2,

        /// <summary>
        /// Keeps only key facts
        /// </summary>
        Extractive = 3,

        /// <summary>
        /// Combines last N messages with a summary
        /// </summary>
        Hybrid = 4
    }

}
