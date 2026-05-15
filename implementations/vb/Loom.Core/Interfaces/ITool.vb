Imports Loom.Core.Models

Namespace Interfaces
    ''' <summary>
    ''' Defines how tool must be invoked by the model
    ''' </summary>
    Public Interface ITool
        ''' <summary>
        ''' Works like a primary key
        ''' </summary>
        ReadOnly Property ToolName As String
        ReadOnly Property Description As String
        ''' <summary>
        ''' Raw parameters schema
        ''' </summary>
        ReadOnly Property Parameters As List(Of ToolParameters)
        ReadOnly Property Required As List(Of String)
        ''' <summary>
        ''' Whether or not it allows strict mode
        ''' </summary>
        ReadOnly Property Strictness As Boolean?
        ''' <summary>
        ''' Executes the tool logic using the provided raw input
        ''' </summary>
        ''' <param name="rawInput">
        ''' A raw string containing the parameters required by the tool
        ''' </param>
        ''' <returns>
        ''' A task that resolves to the tool response as a raw string
        ''' </returns>
        Function ExecuteAsync(rawInput As Dictionary(Of String, Object)) As Task(Of String)
    End Interface
End Namespace