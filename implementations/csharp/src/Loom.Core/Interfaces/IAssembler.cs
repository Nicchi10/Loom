using Loom.Core.Models;
using System.Collections.Generic;

namespace Loom.Core.Interfaces
{
    /// <summary>
    /// Contract for prompt assembly across different providers
    /// </summary>
    public interface IAssembler
    {
        /// <summary>
        /// Builds final message list with context injected
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        List<Message> Assemble(LlmInvocation invocation);

        /// <summary>
        /// Returns neutral typed list, each provider maps it internally
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        List<AssembledItem> AssembleInputItems(LlmInvocation invocation);

        /// <summary>
        /// Aggregates RAG + Memory according to InjectionStrategy
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        string BuildContextBlock(LlmInvocation invocation);

        /// <summary>
        /// Merges original content with context block
        /// </summary>
        /// <param name="original"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        string AddContent(string original, string context);
    }
}