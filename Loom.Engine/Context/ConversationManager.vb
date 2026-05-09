Imports Loom.Core.Enums
Imports Loom.Core.Models

Namespace Context
    ''' <summary>
    ''' Keep chronological order, validate the input and manage token budget
    ''' </summary>
    Public Class ConversationManager

        Private ReadOnly _invocation As LlmInvocation

        ''' <summary>
        ''' Initializes the manager by binding it to a specific invocation
        ''' </summary>
        Public Sub New(invocation As LlmInvocation)
            _invocation = invocation
        End Sub

        ''' <summary>
        ''' Adds message into conversation and increment TurnIndex
        ''' </summary>
        ''' <param name="role"></param>
        ''' <param name="content"> User messagges, System messages or ToolCall result</param>
        ''' <param name="toolCallId"></param> 
        ''' <param name="metadata"></param>
        Public Sub AddMessage(role As MessageRole, content As String, Optional metadata As Dictionary(Of String, Object) = Nothing)

            Dim msg As New Message With {
                .Role = role,
                .Content = content,
                .Metadata = If(metadata, New Dictionary(Of String, Object))
            }

            _invocation.Conversation.Messages.Add(msg)
            _invocation.Conversation.TurnIndex += 1

            If _invocation.Conversation.TokenBudget IsNot Nothing Then
                Throw New NotImplementedException("Preventive budget control(?)")
            End If

        End Sub

        ''' <summary>
        ''' User input method
        ''' </summary>
        ''' <param name="text">User query</param>
        Public Sub AddUserRequest(text As String)
            AddMessage(MessageRole.User, text)
        End Sub

        ''' <summary>
        ''' Tool output method
        ''' </summary>
        ''' <param name="toolName">The result after tool calling</param>
        Public Sub AddToolResult(toolResult As String, toolCallId As String, toolName As String, toolArgs As Dictionary(Of String, Object), Optional metadata As Dictionary(Of String, Object) = Nothing)
            Dim msg As New Message With {
                .Role = MessageRole.Tool,
                .Content = toolResult,
                .ToolCallId = toolCallId,
                .ToolName = toolName,
                .ToolArgs = toolArgs,
                .Metadata = If(metadata, New Dictionary(Of String, Object))
            }

            _invocation.Conversation.Messages.Add(msg)
        End Sub

        ''' <summary>
        ''' Clear history
        ''' </summary>
        ''' <param name="keepSystemMessage">If true, keep system message</param>
        Public Sub ResetHistory(keepSystemMessage As Boolean)

            Dim systemMsg As Message = _invocation.Conversation.Messages.FirstOrDefault(Function(m) m.Role = MessageRole.System)

            _invocation.Conversation.Messages.Clear()
            _invocation.Conversation.TurnIndex = 0

            If keepSystemMessage AndAlso systemMsg IsNot Nothing Then
                _invocation.Conversation.Messages.Add(systemMsg)
            End If
        End Sub
    End Class
End Namespace

