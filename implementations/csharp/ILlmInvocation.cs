namespace Interfaces
{
    /// <summary>
    /// Contract for the main invocation object
    /// </summary>
    public interface ILlmInvocation
    {
        Models.ConversationState Conversation {  get; }
        Models.RagContext Rag { get; }
        Models.MemoryContext Memory { get; }
        Models.ToolContext Tools { get; }
        Models.GenerationOptions Options { get; }
        Models.ExecutionHints Hints { get; }

        /// <summary>
        /// Check internal coherency before the run
        /// </summary>
        IvalidationResult Validate();
    }
}
