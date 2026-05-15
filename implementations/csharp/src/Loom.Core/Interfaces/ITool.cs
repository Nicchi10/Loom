using Loom.Core.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Loom.Core.Interfaces
{
    /// <summary>
    /// Defines how tool must be invoked by the model
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Works like a primary key
        /// </summary>
        string ToolName { get; }
        string Description { get; }

        /// <summary>
        /// Raw parameters schema
        /// </summary>
        List<ToolParameters> Parameters { get; }
        List<string> Required {  get; }

        /// <summary>
        /// Whether or not it allows strict mode
        /// </summary>
        bool? Strictness { get; }

        /// <summary>
        /// Executes the tool logic using the provided raw input
        /// </summary>
        /// <param name="rawInput">
        /// A raw string containing the parameters required by the tool
        /// </param>
        /// <returns>
        /// A task that resolves to the tool response as a raw string
        /// </returns>
        Task<string> ExecuteAsync(Dictionary<string, object> rawInput);
    }

}