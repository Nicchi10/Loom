using System.Collections.Generic;

namespace Interfaces
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
        IEnumerable<string> Erros {  get; }

    }
}