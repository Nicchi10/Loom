Namespace Models
    ''' <summary>
    ''' Which tools are available for the invoked type
    ''' </summary>
    Public Class ToolContext
        Public Property RegisteredTools As New List(Of ToolDefinition)
        Public Property MaxToolDepth As Integer = 5 ' prevents eternal loop calls
    End Class
End Namespace