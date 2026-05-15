using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Properties of tool when invoked
    /// </summary>
    public class ToolCallInvocation
    {
        public string Type { get; set; } = "function_call";
        public string CallId { get; set; }
        public string ToolName { get; set; }
        public Dictionary<string, string> Arguments { get; set;}
}
