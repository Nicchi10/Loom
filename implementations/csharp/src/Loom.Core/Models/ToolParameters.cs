using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Properties of tool input schema
    /// </summary>
    public class ToolParameters
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description  { get; set; }
        public List<string> Enums { get; set; }
    }
}
