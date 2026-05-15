Imports Loom.Core.Interfaces
Imports Loom.Core.Models
Imports Loom.Engine
Imports Loom.Providers.Google
Imports Loom.Providers.OpenAI

Namespace GoogleAI
    Public Class GoogleAIAdapter
        Implements IProviderAdapter

        Private ReadOnly _assembler As IAssembler
        Private ReadOnly _http As New Infrastructure.ProviderHttpClient()
        Private ReadOnly _apiKey As String

        Public Sub New(apiKey As String, assembler As IAssembler)
            _apiKey = apiKey
            _assembler = assembler
        End Sub

        Public ReadOnly Property ProviderName As String Implements IProviderAdapter.ProviderName
            Get
                Return "GoogleAI"
            End Get
        End Property

        Public Async Function ExecuteAsync(invocation As ILlmInvocation) As Task(Of LlmResponse) Implements IProviderAdapter.ExecuteAsync

            Dim concreteInvocation = DirectCast(invocation, LlmInvocation)

            Dim headers = New Dictionary(Of String, String) From {
                {"x-goog-api-key", _apiKey}
            }

            Dim modelId = If(concreteInvocation.Hints.PreferredModel, "gemini-2.5-flash")

            Dim payload = BuildPayload(concreteInvocation)


            Dim url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent"

            Dim rawResponse As GenerateContentResponse = Await _http.PostAsync(Of GenerateContentResponse)(headers, url, payload)

            Return New LlmResponse With {
                .Content = ExtractResponse(rawResponse),
                .Id = rawResponse.ResponseId,
                .ModelUsed = rawResponse.ModelVersion,
                .Role = 2, ' Assistant
                .ToolCalls = ExtractToolInfo(rawResponse),
                .PromptTokens = rawResponse.UsageMetadata.PromptTokenCount,
                .CompletionTokens = rawResponse.UsageMetadata.CandidatesTokenCount,
                .TotalTokens = rawResponse.UsageMetadata.TotalTokenCount,
                .Metadata = ExtractMetadata(rawResponse)
            }
        End Function

        'Public Async Function TestExeAsync() As Task(Of Object)
        '    Dim headers = New Dictionary(Of String, String) From {
        '        {"x-goog-api-key", _apiKey}
        '    }

        '    Dim hardcodedInput As String = "Ciao, sei un LLM?"

        '    Dim payload As New Dictionary(Of String, Object) From {
        '        {"contents", New List(Of Object) From {
        '            New Dictionary(Of String, Object) From {
        '                {"role", "user"},
        '                {"parts", New List(Of Object) From {
        '                    New Dictionary(Of String, Object) From {
        '                        {"text", hardcodedInput}
        '                    }
        '                }}
        '            }
        '        }},
        '        {"generationConfig", New Dictionary(Of String, Object) From {
        '            {"responseMimeType", "text/plain"}
        '        }}
        '    }

        '    Dim modelName = "gemini-2.5-flash"
        '    Dim url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent"

        '    Dim rawResponse As GenerateContentResponse = Await _http.PostAsync(Of GenerateContentResponse)(headers, url, payload)

        '    Return New LlmResponse With {
        '        .Content = ExtractResponse(rawResponse),
        '        .Id = rawResponse.ResponseId,
        '        .ModelUsed = rawResponse.ModelVersion,
        '        .Role = 2, ' Assistant
        '        .ToolCalls = ExtractToolInfo(rawResponse),
        '        .PromptTokens = rawResponse.UsageMetadata.PromptTokenCount,
        '        .CompletionTokens = rawResponse.UsageMetadata.CandidatesTokenCount,
        '        .TotalTokens = rawResponse.UsageMetadata.TotalTokenCount,
        '        .Metadata = ExtractMetadata(rawResponse)
        '    }
        'End Function

        Public Function SupportsCapability(capability As String) As Boolean Implements IProviderAdapter.SupportsCapability
            Throw New NotImplementedException()
        End Function

        Private Function BuildPayload(concreteInvocation As LlmInvocation) As Dictionary(Of String, Object)
            Dim inputItems = _assembler.AssembleInputItems(concreteInvocation)

            Dim systemItem = inputItems.FirstOrDefault(Function(x) x.Role = "system")
            Dim systemText = If(systemItem?.Content, "")

            Dim contents = inputItems.Where(Function(x) x.Role <> "system").Select(Function(x)
                                                                                       If x.Role = "tool" Then
                                                                                           Return CType(New Dictionary(Of String, Object) From {
                                                                                                {"role", "user"},
                                                                                                {"parts", New List(Of Object) From {
                                                                                                    New Dictionary(Of String, Object) From {
                                                                                                        {"functionResponse", New Dictionary(Of String, Object) From {
                                                                                                            {"name", x.ToolName},
                                                                                                            {"response", New Dictionary(Of String, Object) From {
                                                                                                                {"result", x.Content}
                                                                                                            }}
                                                                                                        }}
                                                                                                    }
                                                                                                }}
                                                                                            }, Object)
                                                                                       Else
                                                                                           Return CType(New Dictionary(Of String, Object) From {
                                                                                                {"role", If(x.Role = "assistant", "model", x.Role)},
                                                                                                {"parts", New List(Of Object) From {
                                                                                                    New Dictionary(Of String, Object) From {
                                                                                                        {"text", x.Content}
                                                                                                    }
                                                                                                }}
                                                                                            }, Object)
                                                                                       End If
                                                                                   End Function).ToList()

            Dim toolDeclarations = BuildToolSchema(concreteInvocation)

            Dim results As New Dictionary(Of String, Object) From {
                {"system_instruction", New Dictionary(Of String, Object) From {
                    {"parts", New List(Of Object) From {
                        New Dictionary(Of String, Object) From {
                            {"text", systemText}
                        }
                    }}
                }},
                {"contents", contents},
                {"generationConfig", New Dictionary(Of String, Object) From {
                    {"responseMimeType", "text/plain"}
                }}}

            If toolDeclarations.Count > 0 Then
                results.Add("tools", New List(Of Object) From {
                        New Dictionary(Of String, Object) From {
                            {"function_declarations", toolDeclarations}
                        }
                    })
            End If

            Return results
        End Function

        Private Function ExtractResponse(rawResponse As GenerateContentResponse) As String

            Dim risposta_modello As String = ""

            Try
                Dim firstCandidate = rawResponse.Candidates(0)

                If firstCandidate.Content Is Nothing OrElse
                    firstCandidate.Content.Parts Is Nothing Then
                    Return String.Empty
                End If

                Dim result As New Text.StringBuilder()

                For Each part In firstCandidate.Content.Parts
                    If part.Text IsNot Nothing Then
                        result.Append(part.Text)
                    End If
                Next

                Return result.ToString()
            Catch ex As Exception
                Throw New Exception($"[ERROR] The response received from the network call doesn't meet the correct parameters: {ex}")
            End Try

        End Function

        Private Function ExtractToolInfo(rawResponse As GenerateContentResponse) As List(Of ToolCallInvocation)

            Dim toolList As New List(Of ToolCallInvocation)

            Try
                For Each candidate In rawResponse.Candidates
                    For Each part In candidate.Content.Parts
                        If part.FunctionCall IsNot Nothing Then
                            toolList.Add(New ToolCallInvocation With {
                                .Type = "function_call",
                                .CallId = $"call_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                                .ToolName = part.FunctionCall.Name,
                                .Arguments = If(part.FunctionCall.Args, New Dictionary(Of String, Object))
                            })
                        End If
                    Next
                Next
            Catch ex As Exception
                Throw New Exception($"[ERROR] The response received from the network call doesn't meet the correct parameters: {ex}")
            End Try
            Return toolList

        End Function

        Private Function BuildToolSchema(invocation As ILlmInvocation) As List(Of GoogleFunctionDeclaration)

            Dim toolSchemaList As New List(Of GoogleFunctionDeclaration)

            Dim builder As New GoogleAIToolSchemaBuilder()

            If invocation.Tools.RegisteredTools IsNot Nothing Then
                For Each tool In invocation.Tools.RegisteredTools
                    toolSchemaList.Add(builder.Build(tool))
                Next
            End If
            Return toolSchemaList
        End Function

        Private Function ExtractMetadata(rawResponse As GenerateContentResponse) As Dictionary(Of String, Object)
            Return Nothing
        End Function
    End Class
End Namespace


