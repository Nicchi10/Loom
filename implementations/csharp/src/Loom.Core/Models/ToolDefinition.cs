using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Defines tool content
    /// </summary>
    public class ToolDefinition
    {
        public string ToolName { get; set; }
        public string Description   { get; set; }
        public bool? Strictness { get; set; }
        public List<string> Required { get; set; }
        public List<ToolParameters> Parameters { get; set; }
    }
}
