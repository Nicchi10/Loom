using Loom.Core.Enums;
using Loom.Core.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Loom.Engine.Context
{
    /// <summary>
    /// Keep chronological order, validate the input and manage token budget
    /// </summary>
    public class ConversationManager
    {
        private readonly LlmInvocation _invocation;

        /// <summary>
        /// Initializes the manager by binding it to a specific invocation
        /// </summary>
        public ConversationManager(LlmInvocation invocation)
        {
            _invocation = invocation;
        }

        /// <summary>
        /// Adds message into conversation and increment TurnIndex
        /// </summary>
        /// <param name="role"></param>
        /// <param name="content">User messagges, System messages </param>
        /// <param name="metadata">Optional</param>
        public void AddMessage(MessageRole role, string content, Dictionary<string, object> metadata = null)
        {
            var msg = new Message
            {
                Role = role,
                Content = content,
                Metadata = metadata ?? new Dictionary<string, object>()
            };


            _invocation.Conversation.Messages.Add(msg);
            _invocation.Conversation.TurnIndex += 1;

            // Budget is enforced by TokenCounter inside the orchestration loop
            // (LoomClient.SendAsync), so appending a message must never throw here.

        }

        /// <summary>
        /// User input method
        /// </summary>
        /// <param name="text">User query</param>
        public void AddUserRequest(string text)
        {
            AddMessage(MessageRole.User, text);
        }

        /// <summary>
        /// Tool output method
        /// </summary>
        /// <param name="toolResult">The result after tool calling</param>
        /// <param name="toolCallId"></param>
        /// <param name="toolName"></param>
        /// <param name="toolArgs">Parameters used for tooling</param>
        /// <param name="metadata">Optional</param>
        public void AddToolResult(string toolResult, string toolCallId, string toolName, Dictionary<string, object> toolArgs, Dictionary<string, object> metadata = null)
        {
            var msg = new Message
            {
                Role = MessageRole.Tool,
                Content = toolResult,
                ToolCallId = toolCallId,
                ToolName = toolName,
                ToolArgs = toolArgs,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            _invocation.Conversation.Messages.Add(msg);
        }

        /// <summary>
        /// Clear History
        /// </summary>
        /// <param name="keepSystemMessage">If true, keep system message</param>
        public void ResetHistory(bool keepSystemMessage)
        {
            var systemMsg = _invocation.Conversation.Messages.FirstOrDefault(m => m.Role == MessageRole.System);

            _invocation.Conversation.Messages.Clear();
            _invocation.Conversation.TurnIndex = 0;

            if (keepSystemMessage && systemMsg != null)
                _invocation.Conversation.Messages.Add(systemMsg);
        }

    }
}
