Imports Loom.Core.Models

Namespace Interfaces
    ''' <summary>
    ''' Contract for prompt assembly across different providers
    ''' </summary>
    Public Interface IAssembler

        ''' <summary>
        ''' Builds final message list with context injected
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns></returns>
        Function Assemble(invocation As LlmInvocation) As List(Of Message)

        ''' <summary>
        ''' Returns neutral typed list, each provider maps it internally
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns></returns>
        Function AssembleInputItems(invocation As LlmInvocation) As List(Of AssembledItem)

        ''' <summary>
        ''' Aggregates RAG + Memory according to InjectionStrategy
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns></returns>
        Function BuildContextBlock(invocation As LlmInvocation) As String

        ''' <summary>
        ''' Merges original content with context block
        ''' </summary>
        ''' <param name="original"></param>
        ''' <param name="context"></param>
        ''' <returns></returns>
        Function AddContent(original As String, context As String) As String


    End Interface
End Namespace

