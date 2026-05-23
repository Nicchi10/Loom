using Loom.Core.Enums;
using Loom.Core.Interfaces;
using Loom.Core.Models;
using Loom.Engine.Context;
using Loom.Engine.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Loom.Engine
{
    /// <summary>
    /// Client engine
    /// </summary>
    public class LoomClient
    {

        public LlmInvocation Invocation { get; }
        public ConversationManager Conversation { get; }
        public RagManager Rag { get; }
        public MemoryManager Memory { get; }
        public ToolRegistry Tool { get; }


        private readonly ExecutionRouter _router = new ExecutionRouter();
        private readonly TokenCounter _tokenCounter = new TokenCounter();

        public LoomClient()
        {
            Invocation = new LlmInvocation();
            Conversation = new ConversationManager(Invocation);
            Rag = new RagManager(Invocation);
            Memory = new MemoryManager(Invocation);
            Tool = new ToolRegistry(Invocation);
        }

        public void RegisterProvider(IProviderAdapter adapter)
        {
            _router.RegisterAdapter(adapter);
        }

        /// <summary>
        /// Manages the call to the model orchestrating all context
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<LlmResponse> SendAsync()
        {
            // Formal validation
            var validation = Invocation.Validate();

            if (!validation.IsValid)
                throw new Exception($"Validation failed: {validation.Errors}");

            // TokenBudget checks
            if (!_tokenCounter.IsWithinBudget(Invocation))
                throw new Exception("Current context exceeds the TokenBudget set");

            // Route to model
            var adapter = _router.Route(Invocation);

            // Core
            int currentDepth = 0;
            LlmResponse lastResponse = null;

            while (currentDepth < Invocation.Tools.MaxToolDepth)
            {
                lastResponse = await adapter.ExecuteAsync(Invocation);

                // If not empty, appenda the model response
                if (!string.IsNullOrEmpty(lastResponse.Content))
                    Conversation.AddMessage(MessageRole.Assistant, lastResponse.Content);

                // If no tool function has been called, returns the response
                if (lastResponse.ToolCalls == null || lastResponse.ToolCalls.Count == 0)
                    return lastResponse;

                // Takes all Tool calls
                // Computes them
                // Then adds them to the conversation
                foreach (var tCall in lastResponse.ToolCalls)
                {
                    var toolResult = await Tool.ExecuteAsync(tCall.ToolName, tCall.Arguments);

                    Conversation.AddToolResult(toolResult, tCall.CallId, tCall.ToolName, tCall.Arguments);
                }

                currentDepth++;
            }

            return await adapter.ExecuteAsync(Invocation);

        }

    }
}
