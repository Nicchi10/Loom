Imports Loom.Core.Interfaces
Imports Loom.Core.Models

Namespace Context
    ''' <summary>
    ''' Records new tool and executes client-side operations related to function tool results
    ''' </summary>
    Public Class ToolRegistry
        Private ReadOnly _invocation As LlmInvocation
        Private ReadOnly _performer As New Dictionary(Of String, ITool)

        Public Sub New(invocation As LlmInvocation)
            _invocation = invocation
        End Sub

        ''' <summary>
        ''' Add new tool in the engine
        ''' </summary>
        ''' <param name="tool"> Object </param>
        Public Sub RegisterTool(tool As ITool)
            If _performer.ContainsKey(tool.ToolName) Then
                Throw New ArgumentException($"Tool: '{tool.ToolName}' is already registered")
            End If

            _performer.Add(tool.ToolName, tool)

            Dim def As New ToolDefinition With {
                .ToolName = tool.ToolName,
                .Description = tool.Description,
                .Parameters = tool.Parameters,
                .Required = tool.Required,
                .Strictness = tool.Strictness
            }

            _invocation.Tools.RegisteredTools.Add(def)
        End Sub

        ''' <summary>
        ''' Executes a tool by JSON LLM request
        ''' </summary>
        ''' <param name="toolName"> Tool name </param>
        ''' <param name="arguments"> Tool arguments </param>
        ''' <returns>
        ''' Result as String or error message that inform LLM
        ''' </returns>
        Public Async Function ExecuteAsync(toolName As String, arguments As Dictionary(Of String, Object)) As Task(Of String)

            If Not _performer.ContainsKey(toolName) Then
                Return $"[Error] '{toolName}' tool doesn't exist or isn't configured"
            End If

            Try
                Dim result = Await _performer(toolName).ExecuteAsync(arguments)
                Return result
            Catch ex As Exception
                Return $"[Error] while running the tool '{toolName}': {ex.Message}"
            End Try

        End Function

    End Class

End Namespace
