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
            New ModelMetaData With {.ModelId = "gpt-5.5", .ProviderName = "OpenAI", .ContextWindow = 128000, .CostLevel = 5, .LatencyLevel = 4, .QualityLevel = 5},
            New ModelMetaData With {.ModelId = "gpt-4.1-nano", .ProviderName = "OpenAI", .ContextWindow = 128000, .CostLevel = 1, .LatencyLevel = 1, .QualityLevel = 2},
            New ModelMetaData With {.ModelId = "gemini-2.5-flash", .ProviderName = "GoogleAI", .ContextWindow = 128000, .CostLevel = 1, .LatencyLevel = 2, .QualityLevel = 3}
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
        ''' according to the requested ExecutionPriority, ranking on the relevant signal
        ''' (CostLevel, LatencyLevel or QualityLevel).
        ''' </summary>
        ''' <param name="priority"> Chosen by the user </param>
        ''' <returns> The best ModelMetaData for that priority </returns>
        ''' <exception cref="Exception"> Thrown if the catalog is empty </exception>
        Public Function GetFallBack(priority As ExecutionPriority) As ModelMetaData

            If _models.Count = 0 Then
                Throw New Exception("Model catalog is empty: no fallback model available")
            End If

            Select Case priority
                ' Cheapest model.
                Case ExecutionPriority.CostOptimized
                    Return _models.OrderBy(Function(m) m.CostLevel).First()

                ' Fastest model (lowest latency rank).
                Case ExecutionPriority.LatencyOptimized
                    Return _models.OrderBy(Function(m) m.LatencyLevel).First()

                ' Most capable model (highest quality rank).
                Case ExecutionPriority.QualityOptimized
                    Return _models.OrderByDescending(Function(m) m.QualityLevel).First()

                ' Balanced: a simple value score that rewards quality while penalising
                ' cost and latency (quality is double-weighted). Highest score wins.
                Case Else
                    Return _models.OrderByDescending(Function(m) 2 * m.QualityLevel - m.CostLevel - m.LatencyLevel).First()
            End Select
        End Function
    End Class
End Namespace

