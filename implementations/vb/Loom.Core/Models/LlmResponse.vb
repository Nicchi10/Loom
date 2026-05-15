Namespace Models

    ''' <summary>
    ''' Properties of LLM response
    ''' </summary>
    Public Class LlmResponse
        Public Property Id As String
        Public Property ModelUsed As String
        Public Property Role As Enums.MessageRole
        Public Property Content As String
        Public Property ToolCalls As List(Of ToolCallInvocation)



        Public Property PromptTokens As Integer
        Public Property CompletionTokens As Integer
        Public Property TotalTokens As Integer


        Public Property Metadata As New Dictionary(Of String, Object)
    End Class
End Namespace