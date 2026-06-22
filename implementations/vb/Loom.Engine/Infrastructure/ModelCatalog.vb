Imports Loom.Core.Enums
Imports Loom.Core.Models

Namespace Infrastructure
    ''' <summary>
    ''' Contains the list of all available models and the rules on how to get them
    ''' </summary>
    Public Class ModelCatalog

        ' TODO(#1): make catalog extensible, providers Declare their own models (SupportedModels),
        ' ModelCatalog becomes a registry exposed via LoomClient.Models, see issue #1 (pending UML review)
        Private ReadOnly _models As New List(Of ModelMetaData) From {
            New ModelMetaData With {.ModelId = "gpt-4o", .ProviderName = "OpenAI", .ContextWindow = 128000, .CostLevel = 4},
            New ModelMetaData With {.ModelId = "gpt-4.1-nano", .ProviderName = "OpenAI", .ContextWindow = 128000, .CostLevel = 1},
            New ModelMetaData With {.ModelId = "gemini-2.5-flash", .ProviderName = "GoogleAI", .ContextWindow = 128000, .CostLevel = 1}
        }

        ''' <summary>
        ''' Gets the model based on user input
        ''' </summary>
        ''' <param name="modelId"> e.g: "gpt-5.1" </param>
        ''' <returns> ModelMetaData object </returns>
        Public Function GetModelInfo(modelId As String) As ModelMetaData
            For Each m In _models
                If m.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase) Then
                    Return m
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Fallback when the user did not pick a model: chooses one from the catalog
        ''' according to the requested ExecutionPriority.
        ''' </summary>
        ''' <param name="priority"> Chosen by the user </param>
        ''' <returns> The best ModelMetaData for that priority </returns>
        ''' <exception cref="Exception"> Thrown if the catalog is empty </exception>
        ''' <remarks>
        ''' CostLevel is the only ranking signal the catalog has today, so latency and
        ''' quality are approximated from it: the cheapest models are usually the small,
        ''' fast ones, the most expensive are usually the largest, most capable ones.
        ''' TODO(#1): add real LatencyLevel / QualityLevel to ModelMetaData and rank on those.
        ''' </remarks>
        Public Function GetFallBack(priority As ExecutionPriority) As ModelMetaData

            If _models.Count = 0 Then
                Throw New Exception("Model catalog is empty: no fallback model available")
            End If

            Dim byCostAscending = _models.OrderBy(Function(m) m.CostLevel).ToList()

            Select Case priority
                ' Cheapest model: lowest bill and, as a proxy, the lowest latency.
                Case ExecutionPriority.CostOptimized, ExecutionPriority.LatencyOptimized
                    Return byCostAscending.First()

                ' Most expensive model: best proxy we have for "most capable".
                Case ExecutionPriority.QualityOptimized
                    Return byCostAscending.Last()

                ' Balanced: the positional middle of the cost-sorted list -- a rough
                ' proxy, not a true cost midpoint. On even counts it biases to the pricier
                ' side; with a richer catalog it lands between the cheap and pricey models.
                Case Else
                    Return byCostAscending(byCostAscending.Count \ 2)
            End Select
        End Function
    End Class
End Namespace

