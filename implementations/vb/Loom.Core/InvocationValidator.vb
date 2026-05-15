Imports Loom.Core.Enums

Namespace Validation
    ''' <summary> 
    ''' Provides validation logic for an <see cref="Interfaces.ILlmInvocation"/> 
    ''' Ensures that the invocation contains a coherent and executable configuration before being processed by the LLM
    ''' </summary>
    Public Class InvocationValidator

        ''' <summary> 
        ''' Validates the specified invocation object and returns a <see cref="Models.ValidationResult"/> describing whether the invocation is internally consistent
        ''' </summary> 
        ''' <param name="invocation"> 
        ''' The invocation instance to validate. Must contain conversation state, RAG configuration, memory context, tool definitions, and generation options
        ''' </param> 
        ''' <returns> 
        ''' A <see cref="Models.ValidationResult"/> indicating success or failure
        ''' If validation fails, the result contains a list of error messages
        ''' </returns>
        Public Shared Function Validate(invocation As Interfaces.ILlmInvocation) As Models.ValidationResult
            Dim errors As New List(Of String)()

            ' Conversation must contain at least one message
            If invocation.Conversation.Messages Is Nothing OrElse Not invocation.Conversation.Messages.Any() Then
                errors.Add("The conversation doesn't contain messages. At least a user or system message is required")
            End If

            ' Token budget consistency check
            If invocation.Conversation.TokenBudget.HasValue Then
                Dim totalBudget = invocation.Conversation.TokenBudget.Value
                Dim ragBudget = invocation.Rag.MaxTokens

                If ragBudget >= totalBudget Then
                    errors.Add($"The RAG's budget {ragBudget} can't exceed or equal the total budget {totalBudget} ")
                End If
            End If

            ' Tool schema validation
            If invocation.Tools.RegisteredTools.Any() Then
                For Each tool In invocation.Tools.RegisteredTools
                    If tool.Parameters Is Nothing Then
                        errors.Add($"{tool.ToolName} hasn't got a valid input schema")
                    End If
                Next
            End If

            ' Existence of system message
            Dim sysMsg = invocation.Conversation.Messages.FirstOrDefault(Function(m) m.Role = MessageRole.System)

            If String.IsNullOrWhiteSpace(invocation.Conversation.SystemPrompt) AndAlso
                (sysMsg Is Nothing OrElse String.IsNullOrWhiteSpace(sysMsg.Content)) Then

                errors.Add($"No SystemMessage set, provide a valid SystemMessage")

            End If


            If errors.Any() Then
                Return New Models.ValidationResult(False, errors)
            Else
                Return Models.ValidationResult.Success()
            End If
        End Function

    End Class

End Namespace
