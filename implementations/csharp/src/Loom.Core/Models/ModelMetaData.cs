using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Select the best model for a provider
    /// </summary>
    public class ModelMetaData
    {
        public string ModelId { get; set; }
        public string ProviderName { get; set; }
        public int ContextWindow {  get; set; }
        public bool SupportsTools { get; set; }
        public int CostLevel { get; set; } // 1 (cheap) -> 5 (expensive)
        public int LatencyLevel { get; set; } // 1 (fastest) -> 5 (slowest)
        public int QualityLevel { get; set; } // 1 (basic) -> 5 (most capable)
        public List<string> Capabilities { get; set; } // (vision, audio, ...)
    }
}
