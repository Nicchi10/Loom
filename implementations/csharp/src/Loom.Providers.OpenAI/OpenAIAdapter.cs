using Loom.Core.Enums;
using Loom.Core.Interfaces;
using Loom.Core.Models;
using Loom.Engine;
using Loom.Engine.Infrastructure;
using Loom.Providers.OpenAI.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Loom.Providers.OpenAI
{
    public class OpenAIAdapter : IProviderAdapter
    {

        private readonly IAssembler _assembler;
        private readonly ProviderHttpClient _http = new ProviderHttpClient();
        private readonly string _apikey;

        public string ProviderName => "OpenAI";

        public OpenAIAdapter(string apikey, IAssembler assembler)
        {
            _apikey = apikey;
            _assembler = assembler;
        }

        /// <summary>
        /// Provider core function:
        /// - Takes the current invocation
        /// - Maps invocation input
        /// - Builds the tool schema
        /// - Assembles the API calls payload + headers
        /// - Makes API calls
        /// - Extracts parameters from the call's response
        /// - Return the response
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns> LlmResponse </returns>
        public async Task<LlmResponse> ExecuteAsync(ILlmInvocation invocation)
        {
            var concreteInvocation = (LlmInvocation)invocation;

            var inputItems = MappingAssembledInput(concreteInvocation);

            // gpt-4.1-nano default
            var modelId = concreteInvocation.Hints?.PreferredModel ?? "gpt-4.1-nano";

            var toolList = BuildToolSchema(invocation);

            var payload = new Dictionary<string, object>
            {
                {"model", modelId },
                { "include", new List<string> { "file_search_call.results" } },
                { "input", inputItems },
                { "tools", toolList },
                { "metadata", new Dictionary<string, object>() },
                { "service_tier", "priority" },
                { "max_output_tokens", invocation.Options.MaxTokens },
                { "temperature", invocation.Options.Temperature }
            };

            var headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {_apikey}"}
            };

            ModelResponse rawResponse = await _http.PostAsync<ModelResponse>(headers, "https://api.openai.com/v1/responses", payload);

            return new LlmResponse
            {
                Content = ExtractResponse(rawResponse),
                Id = rawResponse.Id,
                ModelUsed = rawResponse.Model,
                Role = MessageRole.Assistant,
                ToolCalls = ExtractToolInfo(rawResponse),
                PromptTokens = rawResponse.Usage.Input_tokens,
                CompletionTokens = rawResponse.Usage.Output_tokens,
                TotalTokens = rawResponse.Usage.Total_tokens,
                Metadata = rawResponse.Metadata
            };
        }


        /// <summary>
        /// TODO: Revisiting the function
        /// </summary>
        /// <param name="capability"></param>
        /// <returns></returns>
        public bool SupportsCapability(string capability)
        {
            var supported = new[] { "vision", "tools", "json_mode" };

            return supported.Contains(capability.ToLower());
        }


        /// <summary>
        /// - Gets the AssembledItems list
        /// - Translates into OpenAI dialect
        /// </summary>
        /// <param name="concreteInvocation"></param>
        /// <returns> The input string to pass to the LLM </returns>
        /// <exception cref="Exception"></exception>
        private List<object> MappingAssembledInput(LlmInvocation concreteInvocation)
        {

            var inputItems = _assembler.AssembleInputItems(concreteInvocation);

            try
            {
                return inputItems.Select(x =>
                {
                    if (x.Type == "function_call")
                    {
                        return (object)new
                        {
                            type = x.Type,
                            name = x.ToolName,
                            arguments = JsonSerializer.Serialize(x.Arguments),
                            call_id = x.ToolCallId
                        };
                    }
                    else if (x.Type == "function_call_output")
                    {
                        return (object)new
                        {
                            type = x.Type,
                            call_id = x.ToolCallId,
                            output = x.Content
                        };
                    }
                    else
                    {
                        return (object)new
                        {
                            role = x.Role,
                            content = x.Content
                        };
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] OpenAIAdapter.MappingAssembledInput: {ex}");
            }

        }


        /// <summary>
        /// Extracts the model response string while avoiding other objects
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string ExtractResponse(ModelResponse rawResponse)
        {
            string model_response = "";

            try
            {

                foreach (var item in rawResponse.Output)
                {
                    if (item.Type == "message")
                    {
                        foreach (var content in item.Content)
                        {
                            if (content.Type == "output_text")
                                model_response = content.Text;
                        }
                    }
                }
                return model_response;

            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] The response received from the network call doesn't meet the correct parameters: {ex}");
            }
        }


        /// <summary>
        /// Extracts the model response in the tool usage domain, avoiding other objects
        /// OpenAI returns the arguments as objects -> deserializes them
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<ToolCallInvocation> ExtractToolInfo(ModelResponse rawResponse)
        {
            var toolList = new List<ToolCallInvocation>();

            try
            {
                foreach (var item in rawResponse.Output)
                {
                    if (item.Type == "function_call" || item.Type == "tool_call")
                    {
                        Dictionary<string, object> argsDict;

                        if (!string.IsNullOrEmpty(item.Arguments))
                            argsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(item.Arguments);
                        else
                            argsDict = new Dictionary<string, object>();

                        var tool = new ToolCallInvocation
                        {
                            Type = "function_call",
                            CallId = item.Call_id,
                            ToolName = item.Name,
                            Arguments = argsDict
                        };

                        toolList.Add(tool);
                    }
                }


            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] While parsing tool call: {ex}");
            }

            return toolList;

        }


        /// <summary>
        /// Builds tools schemas in OpenAI format
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        private List<OpenAIToolSchema> BuildToolSchema(ILlmInvocation invocation)
        {
            var toolSchemaList = new List<OpenAIToolSchema>();

            var builder = new OpenAIToolSchemaBuilder();

            if (invocation.Tools.RegisteredTools != null)
            {
                foreach (var tool in invocation.Tools.RegisteredTools)
                    toolSchemaList.Add(builder.Build(tool));
            }

            return toolSchemaList;
        }

    }
}
