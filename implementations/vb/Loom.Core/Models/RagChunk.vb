Namespace Models
    ''' <summary>
    ''' Single chunk of the list
    ''' </summary>
    Public Class RagChunk
        Public Property SourceId As String
        Public Property Score As Double ' Semantic relevance (0.0 - 1.0)
        Public Property Text As String
        Public Property Metadata As New Dictionary(Of String, Object)
    End Class
End Namespace