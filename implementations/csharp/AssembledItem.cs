using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Neutral DTO produced by IAssembler
    /// </summary>
    public class AssembledItems
    {
        public string Role {  get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string ToolCallId { get; set; }
        public string ToolName { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
    }
}
