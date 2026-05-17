using Loom.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Loom.Core.Validation
{
    /// <summary>
    /// Provides validation logic for an <see cref="Interfaces.ILlmInvocation"/>
    /// Ensures that the invocation contains a coherent and executable configuration before being processed by the LLM
    /// </summary>
    public class InvocationValidator
    {
        /// <summary>
        /// Validates the specified invocation object and returns a <see cref="Models.ValidationResult"/> describing whether the invocation is internally consistent
        /// </summary>
        /// <param name="invocation">
        /// The invocation instance to validate. Must contain conversation state, RAG configuration, memory context, tool definitions, and generation options
        /// </param>
        /// <returns>
        /// A <see cref="Models.ValidationResult"/> indicating success or failure.
        /// If validation fails, the result contains a list of error messages
        /// </returns>
        public static Models.ValidationResult Validate(Interfaces.ILlmInvocation invocation)
        {

            var errors = new List<String>();

            // Conversation must contain at least one message
            if (invocation.Conversation.Messages == null || !invocation.Conversation.Messages.Any())
            {
                errors.Add("The conversation doesn't contain messages. At least user or system message is required.");
            }

            // Token budget consistency check
            if (invocation.Conversation.TokenBudget.HasValue)
            {
                var totalBudget = invocation.Conversation.TokenBudget.Value;
                var ragBudget = invocation.Rag.MaxTokens;
                if (ragBudget >= totalBudget)
                {
                    errors.Add($"The RAG's budget {ragBudget} can't exceed or equal the total budget {totalBudget}.");
                }
            }

            // Tool schema validation
            if (invocation.Tools.RegisteredTools.Any())
            {
                foreach (var tool in invocation.Tools.RegisteredTools )
                {
                    if (tool.Parameters == null)
                    {
                        errors.Add($"{tool.ToolName} hasn't got a valid input schema.");
                    }
                }
            }

            // Existence of system message
            var sysMsg = invocation.Conversation.Messages.FirstOrDefault(m => m.Role == MessageRole.System);
            if (string.IsNullOrWhiteSpace(invocation.Conversation.SystemPrompt) && (sysMsg == null || string.IsNullOrWhiteSpace(sysMsg.Content)))
            {
                errors.Add("No SystemMessage setted, provide a valid SystemMessage.");
            }


            return errors.Any() ? new Models.ValidationResult(false, errors) : Models.ValidationResult.Success();
        }

    }
}