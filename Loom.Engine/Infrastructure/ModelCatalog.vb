Imports Loom.Core.Enums
Imports Loom.Core.Models

Namespace Infrastructure
    ''' <summary>
    ''' Contains the list of all available models and the rules on how to get them
    ''' </summary>
    Public Class ModelCatalog
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
        ''' Fallback if user input model isn't found
        ''' </summary>
        ''' <param name="priority"> Choose by user </param>
        ''' <returns> ModelMetaData object with user's settings priority </returns>
        Public Function GetFallBack(priority As ExecutionPriority) As ModelMetaData

            Dim best As ModelMetaData = Nothing

            If priority = ExecutionPriority.CostOptimized Then

                Dim lowestCost As Integer = Integer.MaxValue

                For Each m In _models
                    If m.CostLevel < lowestCost Then
                        lowestCost = m.CostLevel
                        best = m
                    End If
                Next
            Else
                Dim highestCost As Integer = Integer.MinValue

                For Each m In _models
                    If m.CostLevel > highestCost Then
                        highestCost = m.CostLevel
                        best = m
                    End If
                Next

            End If
            Return best
        End Function
    End Class
End Namespace

