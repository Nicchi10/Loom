Imports Loom.Core.Models

Namespace Interfaces
    ''' <summary>
    ''' Contract for specific provider
    ''' </summary>
    Public Interface IProviderAdapter

        ReadOnly Property ProviderName As String

        ''' <summary>
        ''' Takes abstract invocation, translates into provider language and returns the response
        ''' </summary>
        ''' <param name="invocation">
        ''' Invocation object
        ''' </param>
        ''' <returns>
        ''' A task that resolves to the model's response
        ''' </returns>
        Function ExecuteAsync(invocation As ILlmInvocation) As Task(Of LlmResponse)

        ''' <summary>
        ''' Check if provider supports specific capability
        ''' </summary>
        ''' <param name="capability">
        ''' Name of capability (e.g. stream)
        ''' </param>
        ''' <returns>
        ''' True or False
        ''' </returns>
        Function SupportsCapability(capability As String) As Boolean
    End Interface
End Namespace