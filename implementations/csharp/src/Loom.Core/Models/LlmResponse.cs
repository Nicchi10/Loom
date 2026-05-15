using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Properties of LLM response
    /// </summary>
    public class LlmResponse
    {
        public string Id { get; set; }
        public string ModelUsed { get; set; }
        public string Content { get; set; }
        public List<ToolCallInvocation> ToolCalls { get; set; }
        public Enums.MessageRole Role { get; set; }

        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }

        public Dictionary<string, object> Metadata { get; set; }
    }
}
