Imports System.Text
Imports Loom.Core.Enums
Imports Loom.Core.Interfaces
Imports Loom.Core.Models

Namespace Assemblers
    ''' <summary>
    ''' Assembles the final list of messages to be sent to the LLM provider.
    ''' Combines the original conversation messages with the invocation's contextual information by injecting the generated
    ''' context block into the system message or creating one if missing
    ''' </summary>
    Public Class PromptAssembler
        Implements IAssembler
        ''' <summary>
        ''' Entry point: transform invocation into a messages list
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns> List of all messages </returns>
        Public Function Assemble(invocation As LlmInvocation) As List(Of Message) Implements IAssembler.Assemble

            ' Copy of original messages list
            Dim finalMessages As New List(Of Message)(invocation.Conversation.Messages)

            Dim contextBlock = BuildContextBlock(invocation)

            ' Finds system message or creates a new one on top
            Dim systemMsg = finalMessages.FirstOrDefault(Function(m) m.Role = MessageRole.System)

            If systemMsg IsNot Nothing Then

                systemMsg.Content = AddContent(systemMsg.Content, contextBlock)

            Else

                finalMessages.Insert(0, New Message With {
                    .Role = MessageRole.System,
                    .Content = AddContent(invocation.Conversation.SystemPrompt, contextBlock)
                })

            End If

            Return finalMessages

        End Function

        ''' <summary>
        ''' Assembles each role kind with its content (user/tool/assistant)
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns> Object with planned structure </returns>
        Public Function AssembleInputItems(invocation As LlmInvocation) As List(Of AssembledItem) Implements IAssembler.AssembleInputItems
            Dim msgs = Me.Assemble(invocation)
            Dim items As New List(Of AssembledItem)

            For Each msg In msgs
                If msg.Role = MessageRole.Tool Then
                    items.Add(New AssembledItem With {
                        .Role = "assistant",
                        .Type = "function_call",
                        .ToolName = msg.ToolName,
                        .Content = msg.Content,
                        .ToolCallId = msg.ToolCallId,
                        .Arguments = msg.ToolArgs
                    })
                    items.Add(New AssembledItem With {
                        .Role = "tool",
                        .Type = "function_call_output",
                        .ToolCallId = msg.ToolCallId,
                        .ToolName = msg.ToolName,
                        .Content = msg.Content
                    })
                Else
                    items.Add(New AssembledItem With {
                        .Role = msg.Role.ToString().ToLower(),
                        .Type = "message",
                        .Content = msg.Content
                    })
                End If
            Next

            Return items
        End Function

        ''' <summary>
        ''' Builds the string that aggregates RAG & memory
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns> The complete memory and context prompt </returns>
        Private Function BuildContextBlock(invocation As LlmInvocation) As String Implements IAssembler.BuildContextBlock

            Dim sb As New StringBuilder()

            ' ############ Memory ############
            If Not String.IsNullOrWhiteSpace(invocation.Memory.Content) Then
                sb.AppendLine("### LONG-TERM MEMORY ###")
                sb.AppendLine(invocation.Memory.Content)
                sb.AppendLine()
            End If

            ' ############ RAG ############
            If invocation.Rag.RetrievedChunks.Any() Then

                sb.AppendLine("### CONTEXT ###")

                For Each chunk In invocation.Rag.RetrievedChunks
                    sb.AppendLine($"[Source: {chunk.SourceId}] - {chunk.Text}")
                Next
                sb.AppendLine()
            End If

            Return sb.ToString()

        End Function


        Private Function AddContent(original As String, context As String) As String Implements IAssembler.AddContent
            If String.IsNullOrWhiteSpace(context) Then Return original

            Return $"{original}{Environment.NewLine}{context}"

        End Function



    End Class
End Namespace

