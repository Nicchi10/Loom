Imports Loom.Core.Models

Namespace Infrastructure
    ''' <summary>
    ''' Calculates and checks if the estimated budget respects the tokens used
    ''' </summary>
    Public Class TokenCounter

        ''' <summary>
        ''' Approximates the number of tokens for a string
        ''' </summary>
        ''' <param name="text"> invocation's text</param>
        ''' <returns>Numbers of tokens </returns>
        Public Function CountTokens(text As String) As Integer

            If String.IsNullOrWhiteSpace(text) Then Return 0

            ' Heuristic: approximately 1 token every 4 text char
            Return CInt(Math.Ceiling(text.Length / 4.0))

        End Function

        ''' <summary>
        ''' Calculates total token weight of invocation
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns> Total numbers of tokens </returns>
        Public Function CalculateTotal(invocation As LlmInvocation) As Integer

            Dim total As Integer = 0

            For Each msg In invocation.Conversation.Messages
                total += CountTokens(msg.Content)

                ' fixed overhead for role metadata
                total += 4.0
            Next

            For Each chunk In invocation.Rag.RetrievedChunks
                total += CountTokens(chunk.Text)
            Next

            If Not String.IsNullOrEmpty(invocation.Memory.Content) Then
                total += CountTokens(invocation.Memory.Content)
            End If

            Return total
        End Function

        ''' <summary>
        ''' Checks if invocation respects the selected budget
        ''' </summary>
        ''' <param name="invocation"></param>
        ''' <returns></returns>
        Public Function IsWithinBudget(invocation As LlmInvocation) As Boolean
            If Not invocation.Conversation.TokenBudget.HasValue Then Return True

            Return CalculateTotal(invocation) <= invocation.Conversation.TokenBudget.Value

        End Function

    End Class
End Namespace


