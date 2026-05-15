using System.Collections.Generic;

namespace Loom.Core.Models
{
    public class Message
    {
        public Enums.MessageRole Role { get; set; }
        public string Content { get; set; }
        public string ToolCallId { get; set; }
        public string ToolName { get; set; } 
        public Dictionary<string, object> ToolArgs { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
}
