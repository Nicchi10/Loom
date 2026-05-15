Namespace Models
    ''' <summary>
    ''' All chunks recovered and the chosen strategy
    ''' </summary>
    Public Class RagContext
        Public Property Strategy As Enums.InjectionStrategy = Enums.InjectionStrategy.Sectioned
        Public Property MaxTokens As Integer = 2000 ' recovered by retriever
        Public Property RetrievedChunks As New List(Of RagChunk)
    End Class
End Namespace