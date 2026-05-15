Imports Newtonsoft.Json
Imports Loom.Core.Interfaces
Imports Loom.Core.Models
Imports Loom.Engine
Imports Loom.Providers.Core
Namespace OpenAI
    ''' <summary>
    ''' Provider for OpenAI solutions
    ''' </summary>
    Public Class OpenAIAdapter
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
                Return "OpenAI"
            End Get
        End Property

        Public Async Function ExecuteAsync(invocation As ILlmInvocation) As Task(Of LlmResponse) Implements IProviderAdapter.ExecuteAsync

            Dim concreteInvocation = DirectCast(invocation, LlmInvocation)

            Dim inputItems = MappingAssembledInput(concreteInvocation)

            Dim modelId = If(concreteInvocation.Hints.PreferredModel, "gpt-4.1-nano")

            Dim toolList = BuildToolSchema(invocation)

            Dim payload As New Dictionary(Of String, Object) From {
                {"model", modelId},
                {"include", New List(Of String) From {"file_search_call.results"}},
                {"input", inputItems},
                {"tools", toolList},
                {"metadata", New Dictionary(Of String, Object)()},
                {"service_tier", "priority"},
                {"max_output_tokens", invocation.Options.MaxTokens},
                {"temperature", invocation.Options.Temperature}
            }

            Dim headers = New Dictionary(Of String, String) From {
                {"Authorization", $"Bearer {_apiKey}"}
            }


            Dim rawResponse As ModelResponse = Await _http.PostAsync(Of ModelResponse)(headers, "https://api.openai.com/v1/responses", payload)

            Return New LlmResponse With {
                .Content = ExtractResponse(rawResponse),
                .Id = rawResponse.Id,
                .ModelUsed = rawResponse.Model,
                .Role = 2, ' Assistant
                .ToolCalls = ExtractToolInfo(rawResponse),
                .PromptTokens = rawResponse.Usage.Input_tokens,
                .CompletionTokens = rawResponse.Usage.Output_tokens,
                .TotalTokens = rawResponse.Usage.Total_tokens,
                .Metadata = rawResponse.Metadata
            }
        End Function

        Public Function SupportsCapability(capability As String) As Boolean Implements IProviderAdapter.SupportsCapability
            Dim supported = {"vision", "tools", "json_mode"}

            Return supported.Contains(capability.ToLower())
        End Function

        Private Function MappingAssembledInput(concreteInvocation As LlmInvocation) As List(Of Object)
            Dim inputItems = _assembler.AssembleInputItems(concreteInvocation)
            Try
                Return inputItems.Select(Function(x)
                                             If x.Type = "function_call" Then
                                                 Return CType(New With {
                                                                .type = x.Type,
                                                                .name = x.ToolName,
                                                                .arguments = JsonConvert.SerializeObject(x.Arguments),
                                                                .call_id = x.ToolCallId
                                                            }, Object)
                                             ElseIf x.Type = "function_call_output" Then
                                                 Return CType(New With {
                                                                .type = x.Type,
                                                                .call_id = x.ToolCallId,
                                                                .output = x.Content
                                                            }, Object)
                                             Else
                                                 Return CType(New With {
                                                                .role = x.Role,
                                                                .content = x.Content
                                                            }, Object)
                                             End If
                                         End Function).ToList()
            Catch ex As Exception
                Throw New Exception($"[ERROR] OpenAIAdapter.MappingAssembledInput: {ex}")
            End Try

        End Function

        Private Function ExtractResponse(rawResponse As ModelResponse) As String

            Dim risposta_modello As String = ""

            Try

                For Each item In rawResponse.Output
                    If item.Type = "message" Then
                        For Each content In item.Content
                            If content.Type = "output_text" Then
                                risposta_modello = content.Text
                            End If
                        Next
                    End If
                Next
                Return risposta_modello
            Catch ex As Exception
                Throw New Exception($"[ERROR] The response received from the network call doesn't meet the correct parameters: {ex}")
            End Try



        End Function

        Private Function ExtractToolInfo(rawResponse As ModelResponse) As List(Of ToolCallInvocation)

            Dim toolList As New List(Of ToolCallInvocation)

            Try
                For Each item In rawResponse.Output

                    If item.Type = "function_call" OrElse item.Type = "tool_call" Then

                        Dim argsDict As Dictionary(Of String, Object) = Nothing

                        If Not String.IsNullOrEmpty(item.Arguments) Then
                            argsDict = JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(item.Arguments)
                        Else
                            argsDict = New Dictionary(Of String, Object)
                        End If

                        Dim tool As New ToolCallInvocation With {
                            .Type = "function_call",
                            .CallId = item.Call_id,
                            .ToolName = item.Name,
                            .Arguments = argsDict
                        }

                        toolList.Add(tool)

                    End If

                Next
            Catch ex As Exception
                Throw New Exception($"[ERROR] While parsing tool call: {ex.Message}", ex)
            End Try

            Return toolList

        End Function

        Private Function BuildToolSchema(invocation As ILlmInvocation) As List(Of ProviderToolSchema)

            Dim toolSchemaList As New List(Of ProviderToolSchema)

            Dim builder As New OpenAIToolSchemaBuilder()

            If invocation.Tools.RegisteredTools IsNot Nothing Then
                For Each tool In invocation.Tools.RegisteredTools
                    toolSchemaList.Add(builder.Build(tool))
                Next
            End If
            Return toolSchemaList
        End Function


    End Class
End Namespace

