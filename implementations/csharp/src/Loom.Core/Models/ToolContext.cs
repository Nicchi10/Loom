using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Which tools are available for the invoked type
    /// </summary>
    public class ToolContext
    {
        public List<ToolDefinition> RegisteredTools { get; set; }
        public int MaxToolDepth { get; set; } = 5; // prevents eternal loop calls
    }
}
