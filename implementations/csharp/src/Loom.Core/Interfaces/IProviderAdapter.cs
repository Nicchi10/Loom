using Loom.Core.Models;
using System.Threading.Tasks;

namespace Loom.Core.Interfaces
{
    /// <summary>
    /// Contract for specific provider
    /// </summary>
    public interface IProviderAdapter
    {

        string ProviderName { get; }

        /// <summary>
        /// Takes abstract invocation, translates into provider language and returns the response
        /// </summary>
        /// <param name="invocation">
        /// Invocation object
        /// </param>
        /// <returns>
        /// A task that resolves to the model's response
        /// </returns>
        Task<LlmResponse> ExecuteAsync(ILlmInvocation invocation);

        /// <summary>
        /// Check if provider supports specific capability
        /// </summary>
        /// <param name="capability">
        /// Name of capability (e.g. stream)
        /// </param>
        /// <returns>
        /// True or False
        /// </returns>
        bool SupportsCapability(string capability);

    }
}