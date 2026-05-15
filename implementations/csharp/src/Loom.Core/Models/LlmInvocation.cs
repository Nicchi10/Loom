namespace Loom.Core.Models
{
    /// <summary>
    /// Main class for framework logic
    /// </summary>
    public class LlmInvocation : Interfaces.ILlmInvocation
    {
        public ConversationState Conversation { get; set; } = new ConversationState();
        public RagContext Rag { get; set; } = new RagContext();
        public MemoryContext Memory { get; set; } = new MemoryContext();
        public ToolContext Tools { get; set; } = new ToolContext();
        public GenerationOptions Options {  get; set; } = new GenerationOptions();  
        public ExecutionHints Hints { get; set; } = new ExecutionHints();

        public Interfaces.IValidationResult Validate()
        {
            return Validation.InvocationValidator.Validate(this);
        }
    }
}
