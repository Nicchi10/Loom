Imports Loom.Core.Enums
Imports Loom.Core.Models

Namespace Context
    ''' <summary>
    ''' Ingestion, sort chunks by score and ensures the MaxTokens limit is respected
    ''' </summary>
    Public Class RagManager

        Private ReadOnly _invocation As LlmInvocation

        Public Sub New(invocation As LlmInvocation)
            _invocation = invocation
        End Sub

        ''' <summary>
        ''' Adds a set of retrieved results to the RAG context
        ''' </summary>
        ''' <param name = "chunks" > The retrieved text chunks</param>
        ''' <param name = "clearExisting" > If True, clears the previous context</param>
        Public Sub AddResults(chunks As IEnumerable(Of RagChunk), Optional clearExisting As Boolean = False)

            If clearExisting Then _invocation.Rag.RetrievedChunks.Clear()

            Dim filtered As New List(Of RagChunk)

            For Each c In chunks
                If Not String.IsNullOrWhiteSpace(c.Text) Then
                    filtered.Add(c)
                End If
            Next

            Dim validChunks = filtered.OrderByDescending(Function(c) c.Score)

            _invocation.Rag.RetrievedChunks.AddRange(validChunks)

        End Sub

        ''' <summary>
        ''' Set strategy injection at runtime
        ''' </summary>
        ''' <param name="strategy"></param>
        Public Sub SetStrategy(strategy As InjectionStrategy)
            _invocation.Rag.Strategy = strategy
        End Sub

        ''' <summary>
        ''' Check if RAG context has sufficient data
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property HasContext As Boolean
            Get
                Return _invocation.Rag.RetrievedChunks.Any()
            End Get
        End Property

    End Class
End Namespace

