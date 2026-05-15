using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Single chunk of the list
    /// </summary>
    public class RagChunk
    {
        public string SourceId { get; set; }
        public float Score { get; set; } // Semantic relevance (0.0 - 1.0)
        public string Text { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
