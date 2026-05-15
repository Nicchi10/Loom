Namespace Models
    ''' <summary>
    ''' Master class for framework logic
    ''' </summary>
    Public Class LlmInvocation
        Implements Interfaces.ILlmInvocation
        Public Property Conversation As New ConversationState Implements Interfaces.ILlmInvocation.Conversation
        Public Property Rag As New RagContext Implements Interfaces.ILlmInvocation.Rag
        Public Property Memory As New MemoryContext Implements Interfaces.ILlmInvocation.Memory
        Public Property Tools As New ToolContext Implements Interfaces.ILlmInvocation.Tools
        Public Property Options As New GenerationOptions Implements Interfaces.ILlmInvocation.Options
        Public Property Hints As New ExecutionHints Implements Interfaces.ILlmInvocation.Hints

        Public Function Validate() As Interfaces.IValidationResult Implements Interfaces.ILlmInvocation.Validate
            Return Validation.InvocationValidator.Validate(Me)
        End Function
    End Class
End Namespace

