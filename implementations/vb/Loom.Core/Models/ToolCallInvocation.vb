Namespace Models
    ''' <summary>
    ''' Properties of tool when invoked
    ''' </summary>
    Public Class ToolCallInvocation
        Public Property Type As String = "function_call"
        Public Property CallId As String
        Public Property ToolName As String
        Public Property Arguments As Dictionary(Of String, Object)
    End Class

End Namespace
