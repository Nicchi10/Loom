using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Current conversation status
    /// </summary>
    public class ConversationState
    {
        public string TraceId { get; set; }
        public string SystemPrompt { get; set; }
        public string TokenBudget { get; set; }
        public List<Message> Messages { get; set; }
        public int TurnIndex { get; set; } = 0;
    }
}
