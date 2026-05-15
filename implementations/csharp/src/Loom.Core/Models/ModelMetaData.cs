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
        public List<string> Capabilities { get; set; } // (vision, audio, ...)
    }
}
