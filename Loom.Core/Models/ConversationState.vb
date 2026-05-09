Namespace Models
    ''' <summary>
    ''' Current conversation status
    ''' </summary>
    Public Class ConversationState
        Public Property TraceId As String = Guid.NewGuid().ToString()
        Public Property SystemPrompt As String
        Public Property TokenBudget As Integer? ' Null == unlimited
        Public Property Messages As New List(Of Message)
        Public Property TurnIndex As Integer = 0
    End Class
End Namespace