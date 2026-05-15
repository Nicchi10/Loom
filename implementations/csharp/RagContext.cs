using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// All chunks recovered and the chosen strategy
    /// </summary>
    public class RagContext
    {
        public int MaxTokens { get; set; } = 2000 // recovered by retriever
        public Enums.InjectionStrategy Strategy { get; set; }
        public List<RagChunk> RetrievedChunks { get; set; }

    }
}
