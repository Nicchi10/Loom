Imports Loom.Core.Enums
Imports Loom.Core.Interfaces
Imports Loom.Core.Models
Imports Loom.Engine.Context
Imports Loom.Engine.Infrastructure

Namespace Engine
    ''' <summary>
    ''' Client engine
    ''' </summary>
    Public Class LoomClient

        Public ReadOnly Property Invocation As LlmInvocation
        Public ReadOnly Property Conversation As ConversationManager
        Public ReadOnly Property Rag As RagManager
        Public ReadOnly Property Memory As MemoryManager
        Public ReadOnly Property Tool As ToolRegistry

        Private ReadOnly _router As New ExecutionRouter()
        Private ReadOnly _tokenCounter As New TokenCounter()

        Public Sub New()
            _Invocation = New LlmInvocation()

            _Conversation = New ConversationManager(_Invocation)
            _Rag = New RagManager(_Invocation)
            _Memory = New MemoryManager(_Invocation)
            _Tool = New ToolRegistry(_Invocation)

        End Sub

        ''' <summary>
        ''' Record a new provider
        ''' </summary>
        Public Sub RegisterProvider(adapter As IProviderAdapter)
            _router.RegisterAdapter(adapter)
        End Sub

        ''' <summary>
        ''' Manages the call to the model orchestrating all context
        ''' </summary>
        ''' <returns></returns>
        Public Async Function SendAsync() As Task(Of LlmResponse)

            Dim validation = _Invocation.Validate()

            ' Formal validation
            If Not validation.IsValid Then
                Throw New Exception($"Validation failed: {validation.Errors} ")
            End If

            ' TokenBudget checks
            If Not _tokenCounter.IsWithinBudget(_Invocation) Then
                Throw New Exception("Current context exceeds the TokenBudget set")
            End If

            Dim adapter = _router.Route(_Invocation)

            Dim currentDepth As Integer = 0
            Dim lastResponse As LlmResponse = Nothing

            While currentDepth < _Invocation.Tools.MaxToolDepth
                lastResponse = Await adapter.ExecuteAsync(_Invocation)

                If Not String.IsNullOrEmpty(lastResponse.Content) Then
                    _Conversation.AddMessage(MessageRole.Assistant, lastResponse.Content)
                End If

                If lastResponse.ToolCalls Is Nothing OrElse lastResponse.ToolCalls.Count = 0 Then
                    Return lastResponse
                End If

                For Each tCall In lastResponse.ToolCalls

                    Dim toolResult = Await _Tool.ExecuteAsync(tCall.ToolName, tCall.Arguments)

                    _Conversation.AddToolResult(toolResult, tCall.CallId, tCall.ToolName, tCall.Arguments)
                Next

                currentDepth += 1

            End While

            Return Await adapter.ExecuteAsync(_Invocation)

        End Function

    End Class
End Namespace


