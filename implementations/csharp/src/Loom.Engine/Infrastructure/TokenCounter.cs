using Loom.Core.Models;
using System;

namespace Loom.Engine.Infrastructure
{
    /// <summary>
    /// Calculates and checks if the estimated budget respects the tokens used
    /// </summary>
    public class TokenCounter
    {

        /// <summary>
        /// Computes the number of tokens for a string
        /// </summary>
        /// <param name="text"> invocation text </param>
        /// <returns> Numbers of tokens </returns>
        /// TODO: implements a valid token counting system for providers
        public int CountTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            // Heuristic: approximately 1 token every 4 text char
            return (int)Math.Ceiling(text.Length / 4.0);
        }

        /// <summary>
        /// Calculates total token weight of invocation
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns> Total numbers of tokens </returns>
        public int CalculateTotal(LlmInvocation invocation)
        {
            int total = 0;

            foreach (var msg in invocation.Conversation.Messages)
            {
                total += CountTokens(msg.Content);

                // fixed overhead for role metada
                total += 4;
            }

            foreach (var chunk in invocation.Rag.RetrievedChunks)
                total += CountTokens(chunk.Text);

            if (!string.IsNullOrEmpty(invocation.Memory.Content))
                total += CountTokens(invocation.Memory.Content);

            return total;
        }

        /// <summary>
        /// Checks if invocation respects the selected budget
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        public bool IsWithinBudget(LlmInvocation invocation)
        {
            if(!invocation.Conversation.TokenBudget.HasValue) return true;

            return CalculateTotal(invocation) <= invocation.Conversation.TokenBudget.Value;
        }

    }
}