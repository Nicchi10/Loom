using Loom.Core.Enums;
using Loom.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Loom.Engine.Context
{
    public class RagManager
    {
        private readonly LlmInvocation _invocation;

        public RagManager(LlmInvocation invocation)
        {
            _invocation = invocation;
        }

        /// <summary>
        /// Check if RAG context has data
        /// </summary>
        public bool HasContext => _invocation.Rag.RetrievedChunks.Any();

        /// <summary>
        /// Adds a set of retrieved results to the RAG context
        /// </summary>
        /// <param name="chunks">The retrieved text chunks</param>
        /// <param name="clearExisting">If true, clears the previous context</param>
        public void AddResults(IEnumerable<RagChunk> chunks, bool clearExisting = false)
        {
            if (clearExisting) _invocation.Rag.RetrievedChunks.Clear();

            var filtered = new List<RagChunk>();

            foreach (var chunk in chunks)
            {
                if (!string.IsNullOrWhiteSpace(chunk.Text))
                    filtered.Add(chunk);
            }

            var validChunks = filtered.OrderByDescending(c => c.Score);
            _invocation.Rag.RetrievedChunks.AddRange(validChunks);
        }

        /// <summary>
        /// Set strategy injection at runtime
        /// </summary>
        /// <param name="strategy"></param>
        public void SetStrategy(InjectionStrategy strategy)
        {
            _invocation.Rag.Strategy = strategy;
        }

    }
}
