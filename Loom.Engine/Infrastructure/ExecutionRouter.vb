Imports Loom.Core.Interfaces
Imports Loom.Core.Models

Namespace Infrastructure
    ''' <summary>
    ''' Records new adapter and routes user's input model to a specific provider
    ''' </summary>
    Public Class ExecutionRouter
        Private ReadOnly _adapters As New List(Of IProviderAdapter)
        Private ReadOnly _catalog As New ModelCatalog()

        Public Sub RegisterAdapter(adapter As IProviderAdapter)
            If Not _adapters.Any(Function(a) a.ProviderName = adapter.ProviderName) Then
                _adapters.Add(adapter)
            End If
        End Sub

        ''' <summary>
        ''' Selects the provider based on the entered model
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns>Selected provider</returns>
        Public Function Route(invocation As ILlmInvocation) As IProviderAdapter

            Dim targetModelId = invocation.Hints.PreferredModel
            Dim selectedModelInfo As ModelMetaData = Nothing

            If Not String.IsNullOrWhiteSpace(targetModelId) Then
                selectedModelInfo = _catalog.GetModelInfo(targetModelId)
            End If

            If selectedModelInfo Is Nothing Then
                selectedModelInfo = _catalog.GetFallBack(invocation.Hints.Priority)

                invocation.Hints.PreferredModel = selectedModelInfo.ModelId
            End If

            Dim adapter = _adapters.FirstOrDefault(Function(a) a.ProviderName = selectedModelInfo.ProviderName)

            If adapter Is Nothing Then
                Throw New Exception($"No adapter installed for the provider: {selectedModelInfo.ProviderName}")
            End If

            Return adapter
        End Function

    End Class

End Namespace
