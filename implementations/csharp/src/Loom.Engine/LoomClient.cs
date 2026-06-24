using Loom.Core.Enums;
using Loom.Core.Interfaces;
using Loom.Core.Models;
using Loom.Engine.Context;
using Loom.Engine.Infrastructure;
using System;
using System.Collections.Generic;
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

            // Route to model
            var adapter = _router.Route(Invocation);

            // Read the loop calls as a traffic light:
            //   GREEN  - the model returned a final answer (no tool calls) -> return it
            //   YELLOW - the model asked for tools and still has rounds left -> run
            //            them, feed the results back, let the model think again
            //   RED    - cannot continue (depth ceiling reached, or the budget ran
            //            out after a tool round) -> stop and return the most recent
            //            response WITHOUT running more tools the model could no longer
            //            read, the stop is tagged in Metadata["loom.stopReason"] so the
            //            caller can tell an early stop from a normal answer

            // How many times to retry a failed provider call before giving up
            int maxRetries = Invocation.Hints.MaxRetryCount < 0 ? 0 : Invocation.Hints.MaxRetryCount;

            int round = 0;
            LlmResponse response = null;

            while (true)
            {
                if (!_tokenCounter.IsWithinBudget(Invocation))
                {
                    // Round 0: the input itself is over budget -> fail fast, nothing to salvage
                    // (no response yet, so it throws instead of returning like the later rounds)
                    if (round == 0)
                        throw new Exception("Current context exceeds the TokenBudget set");

                    // Round n: the tool results pushed over budget, stop gracefully without discarding
                    if (response.Metadata == null)
                        response.Metadata = new Dictionary<string, object>();
                    response.Metadata["loom.stopReason"] = "budget_exceeded";
                    return response;
                }

                // Call the model, retrying transient provider failures with exponential backoff
                for (int attempt = 0; ; attempt++)
                {
                    try
                    {
                        response = await adapter.ExecuteAsync(Invocation);
                        break;
                    }
                    catch when (attempt < maxRetries)
                    {
                        await Task.Delay(200 * (1 << attempt));
                    }
                }

                // Records provider's LLM response
                if (!string.IsNullOrEmpty(response.Content))
                    Conversation.AddMessage(MessageRole.Assistant, response.Content);

                // GREEN: no tools requested -> this is the final answer
                bool modelRequestedTools = response.ToolCalls != null && response.ToolCalls.Count > 0;
                if (!modelRequestedTools)
                    return response;

                // RED: out of tool rounds -> stop rather than running tools the model can no longer act on
                if (round >= Invocation.Tools.MaxToolDepth)
                {
                    if (response.Metadata == null)
                        response.Metadata = new Dictionary<string, object>();
                    response.Metadata["loom.stopReason"] = "depth_ceiling";
                    return response;
                }

                // YELLOW: run every requested tool and feed its result back in
                foreach (var toolCall in response.ToolCalls)
                {
                    var toolResult = await Tool.ExecuteAsync(toolCall.ToolName, toolCall.Arguments);
                    Conversation.AddToolResult(toolResult, toolCall.CallId, toolCall.ToolName, toolCall.Arguments);
                }

                round++;
            }

        }

    }
}
