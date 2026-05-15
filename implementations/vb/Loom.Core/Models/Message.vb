Namespace Models
    ''' <summary>
    ''' Single message
    ''' </summary>
    Public Class Message
        Public Property Role As Enums.MessageRole
        Public Property Content As String
        Public Property ToolCallId As String
        Public Property ToolName As String
        Public Property ToolArgs As New Dictionary(Of String, Object)
        Public Property Metadata As New Dictionary(Of String, Object)
    End Class
End Namespace