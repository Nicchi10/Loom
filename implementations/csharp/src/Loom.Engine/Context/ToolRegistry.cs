using Loom.Core.Interfaces;
using Loom.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Loom.Engine.Context
{
    /// <summary>
    /// Registration comes in two distinct ways:
    /// 1. The description sent to the LLM (def)
    /// 2. The object saved inside the performer, 
    /// so when the LLM invokes a Tool, the performer knows where to go to execute it
    /// </summary>
    public class ToolRegistry
    {
        private readonly LlmInvocation _invocation;
        private readonly Dictionary<string, ITool> _performer = new Dictionary<string, ITool>();

        public ToolRegistry(LlmInvocation invocation)
        {
            _invocation = invocation;
        }

        /// <summary>
        /// Add new tool in the engine
        /// </summary>
        /// <param name="tool"></param>
        /// <exception cref="ArgumentException">Tool already registered</exception>
        public void RegisterTool(ITool tool)
        {
            if (_performer.ContainsKey(tool.ToolName))
                throw new ArgumentException($"Tool: '{tool.ToolName}' is already registered");

            _performer.Add(tool.ToolName, tool);

            var def = new ToolDefinition
            {
                ToolName = tool.ToolName,
                Description = tool.Description,
                Parameters = tool.Parameters,
                Required = tool.Required,
                Strictness = tool.Strictness
            };

            _invocation.Tools.RegisteredTools.Add(def);
        }

        /// <summary>
        /// Executes a tool by JSON LLM request
        /// </summary>
        /// <param name="toolName"></param>
        /// <param name="arguments"></param>
        /// <returns>
        /// Result as a string or error message that inform LLM
        /// </returns>
        public async Task<string> ExecuteAsync(string toolName, Dictionary<string, object> arguments)
        {
            if (!_performer.ContainsKey(toolName))
                return $"[Error] '{toolName}' tool doesn't exist or isn't configured";

            try
            {
                return await _performer[toolName].ExecuteAsync(arguments);
            } catch (Exception ex)
            {
                return $"[Error] while running the tool '{toolName}': {ex.Message}";
            }

        }
    }
}
