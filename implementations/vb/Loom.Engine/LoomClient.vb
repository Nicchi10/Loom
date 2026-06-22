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

            ' Formal validation
            Dim validation = _Invocation.Validate()

            If Not validation.IsValid Then
                Throw New Exception($"Validation failed: {validation.Errors} ")
            End If

            Dim adapter = _router.Route(_Invocation)

            ' ── The tool-calling loop, a.k.a. the "semaforo" (traffic light) ──────────
            ' A model can answer in one shot, or it can ask us to run a tool and then
            ' look at the result before answering. That second case is a loop:
            '   model -> tool -> model -> tool -> ...
            ' Without a brake, a model that keeps asking for tools (or a tool that keeps
            ' replying "try again") would spin forever and burn money. MaxToolDepth is
            ' that brake: the maximum number of tool rounds we are willing to run.
            '
            ' Read the loop as a traffic light:
            '   GREEN  - the model returned a final answer (no tool calls) -> return it.
            '   YELLOW - the model asked for tools and we still have rounds left -> run
            '            them, feed the results back, let the model think again.
            '   RED    - we cannot continue (depth ceiling reached, or the budget ran
            '            out after a tool round) -> stop and return the most recent
            '            response WITHOUT running more tools the model could no longer
            '            read. The response is tagged with Metadata("loom.stopReason")
            '            ("depth_ceiling" or "budget_exceeded") so the caller can tell
            '            an early stop apart from a normal answer.
            '
            ' Every early stop RETURNS the transcript built so far -- we never throw work
            ' away once tools have run. The single throw below is the fail-fast for an
            ' input that is already over budget before the very first model call.
            Dim round As Integer = 0
            Dim response As LlmResponse = Nothing

            Do
                If Not _tokenCounter.IsWithinBudget(_Invocation) Then
                    ' Round 0: the input itself is over budget -> fail fast, nothing to salvage.
                    If round = 0 Then
                        Throw New Exception("Current context exceeds the TokenBudget set")
                    End If

                    ' Later rounds: the tool results we just appended pushed us over
                    ' budget. Stop gracefully (like RED) instead of discarding the
                    ' conversation the caller already paid for.
                    If response.Metadata Is Nothing Then
                        response.Metadata = New Dictionary(Of String, Object)()
                    End If
                    response.Metadata("loom.stopReason") = "budget_exceeded"
                    Return response
                End If

                response = Await adapter.ExecuteAsync(_Invocation)

                ' Record the model's textual content. (The assistant tool-call turn
                ' itself is not persisted here; only tool results are, via AddToolResult.)
                If Not String.IsNullOrEmpty(response.Content) Then
                    _Conversation.AddMessage(MessageRole.Assistant, response.Content)
                End If

                ' GREEN: no tools requested -> this is the final answer.
                Dim modelRequestedTools As Boolean = response.ToolCalls IsNot Nothing AndAlso response.ToolCalls.Count > 0
                If Not modelRequestedTools Then
                    Return response
                End If

                ' RED: out of tool rounds -> stop rather than running tools the model
                ' can no longer act on.
                If round >= _Invocation.Tools.MaxToolDepth Then
                    If response.Metadata Is Nothing Then
                        response.Metadata = New Dictionary(Of String, Object)()
                    End If
                    response.Metadata("loom.stopReason") = "depth_ceiling"
                    Return response
                End If

                ' YELLOW: run every requested tool and feed its result back in.
                For Each toolCall In response.ToolCalls
                    Dim toolResult = Await _Tool.ExecuteAsync(toolCall.ToolName, toolCall.Arguments)
                    _Conversation.AddToolResult(toolResult, toolCall.CallId, toolCall.ToolName, toolCall.Arguments)
                Next

                round += 1
            Loop

        End Function

    End Class
End Namespace


