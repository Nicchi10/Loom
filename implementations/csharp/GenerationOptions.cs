using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Technical parameters
    /// </summary>
    public class GenerationOptions
    {
        public float? Temperature { get; set; }
        public float? TopK { get; set; } // token filter
        public int? MaxTokens { get; set; } // token output limits
        public List<string> StopSequence {  get; set; }
        public bool JsonMode { get; set; } = false;
    }
}
