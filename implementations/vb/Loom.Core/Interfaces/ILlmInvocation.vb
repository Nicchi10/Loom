Namespace Interfaces
    ''' <summary>
    ''' Contract for the main invocation object
    ''' </summary>
    Public Interface ILlmInvocation
        ReadOnly Property Conversation As Models.ConversationState
        ReadOnly Property Rag As Models.RagContext
        ReadOnly Property Memory As Models.MemoryContext
        ReadOnly Property Tools As Models.ToolContext
        ReadOnly Property Options As Models.GenerationOptions
        ReadOnly Property Hints As Models.ExecutionHints

        ''' <summary>
        ''' Check internal coherency before the run
        ''' </summary>
        Function Validate() As IValidationResult

    End Interface
End Namespace