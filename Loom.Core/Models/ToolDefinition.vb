Namespace Models
    ''' <summary>
    ''' Defines tool content
    ''' </summary>
    Public Class ToolDefinition
        Public Property ToolName As String
        Public Property Description As String
        Public Property Parameters As List(Of ToolParameters)
        Public Property Required As List(Of String)
        Public Property Strictness As Boolean?
    End Class


End Namespace


