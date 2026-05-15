using System.Collections.Generic;

namespace Loom.Core.Interfaces
{
    /// <summary>
    /// Represents the formal outcome of the invocation
    /// </summary>
    public interface IValidationResult
    {
        /// <summary>
        /// Indicates if the invocation is ready to be sent
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Error message list
        /// </summary>
        IEnumerable<string> Errors {  get; }

    }
}