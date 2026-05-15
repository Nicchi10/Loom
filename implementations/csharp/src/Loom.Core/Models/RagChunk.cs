using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Single chunk of the list
    /// </summary>
    public class RagChunk
    {
        public string SourceId { get; set; }
        public double Score { get; set; } // Semantic relevance (0.0 - 1.0)
        public string Text { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
