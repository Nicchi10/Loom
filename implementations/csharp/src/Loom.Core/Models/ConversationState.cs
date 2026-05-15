using System.Collections.Generic;

namespace Loom.Core.Models
{
    /// <summary>
    /// Current conversation status
    /// </summary>
    public class ConversationState
    {
        public string TraceId { get; set; }
        public string SystemPrompt { get; set; }
        public int? TokenBudget { get; set; }
        public List<Message> Messages { get; set; }
        public int TurnIndex { get; set; } = 0;
    }
}
