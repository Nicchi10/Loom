using Loom.Core.Enums;
using Loom.Core.Interfaces;
using Loom.Core.Models;
using Loom.Engine.Infrastructure;
using Loom.Providers.GoogleAI.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loom.Providers.GoogleAI
{
    public class GoogleAIAdapter : IProviderAdapter
    {

        private readonly IAssembler _assembler;
        private readonly ProviderHttpClient _http = new ProviderHttpClient();
        private readonly string _apikey;

        public string ProviderName => "GoogleAI";

        public GoogleAIAdapter(string apikey, IAssembler assembler)
        {
            _apikey = apikey;
            _assembler = assembler;
        }

        /// <summary>
        /// Provider core function:
        /// - Takes the current invocation
        /// - Maps invocation input (system message goes into system_instruction)
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

            var inputItems = _assembler.AssembleInputItems(concreteInvocation);

            var systemText = inputItems.FirstOrDefault(x => x.Role == "system")?.Content ?? "";

            var contents = MappingAssembledInput(inputItems);

            // gemini-2.5-flash default
            var modelId = concreteInvocation.Hints?.PreferredModel ?? "gemini-2.5-flash";

            var toolList = BuildToolSchema(invocation);

            var payload = new Dictionary<string, object>
            {
                { "system_instruction", new { parts = new[] { new { text = systemText } } } },
                { "contents", contents },
                { "generationConfig", BuildGenerationConfig(invocation.Options) }
            };

            // Gemini rejects an empty function_declarations array
            if (toolList.Count > 0)
                payload.Add("tools", new[] { new { function_declarations = toolList } });

            var headers = new Dictionary<string, string>
            {
                {"x-goog-api-key", _apikey}
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelId}:generateContent";

            GenerateContentResponse rawResponse = await _http.PostAsync<GenerateContentResponse>(headers, url, payload);

            return new LlmResponse
            {
                Content = ExtractResponse(rawResponse),
                Id = rawResponse.ResponseId,
                ModelUsed = rawResponse.ModelVersion ?? modelId,
                Role = MessageRole.Assistant,
                ToolCalls = ExtractToolInfo(rawResponse),
                PromptTokens = rawResponse.UsageMetadata?.PromptTokenCount ?? 0,
                CompletionTokens = rawResponse.UsageMetadata?.CandidatesTokenCount ?? 0,
                TotalTokens = rawResponse.UsageMetadata?.TotalTokenCount ?? 0,
                Metadata = ExtractMetadata(rawResponse)
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
        /// - Gets the AssembledItems list (system message excluded)
        /// - Translates into Gemini dialect:
        ///     assistant -> "model", function_call -> functionCall part, function_call_output -> functionResponse part
        /// </summary>
        /// <param name="inputItems"></param>
        /// <returns> The contents list to pass to the LLM </returns>
        /// <exception cref="Exception"></exception>
        private List<object> MappingAssembledInput(List<AssembledItem> inputItems)
        {
            try
            {
                return inputItems.Where(x => x.Role != "system").Select(x =>
                {
                    if (x.Type == "function_call")
                    {
                        return (object)new
                        {
                            role = "model",
                            parts = new[] { new { functionCall = new { name = x.ToolName, args = x.Arguments ?? new Dictionary<string, object>() } } }
                        };
                    }
                    else if (x.Type == "function_call_output")
                    {
                        return (object)new
                        {
                            role = "user",
                            parts = new[] { new { functionResponse = new { name = x.ToolName, response = new { result = x.Content } } } }
                        };
                    }
                    else
                    {
                        return (object)new
                        {
                            role = x.Role == "assistant" ? "model" : x.Role,
                            parts = new[] { new { text = x.Content } }
                        };
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] GoogleAIAdapter.MappingAssembledInput: {ex}");
            }
        }


        /// <summary>
        /// Maps GenerationOptions into Gemini generationConfig, null options are not sent
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        private Dictionary<string, object> BuildGenerationConfig(GenerationOptions options)
        {
            var config = new Dictionary<string, object>
            {
                { "responseMimeType", options != null && options.JsonMode ? "application/json" : "text/plain" }
            };

            if (options == null)
                return config;

            if (options.MaxTokens.HasValue)
                config.Add("maxOutputTokens", options.MaxTokens.Value);

            if (options.Temperature.HasValue)
                config.Add("temperature", options.Temperature.Value);

            if (options.StopSequences != null && options.StopSequences.Count > 0)
                config.Add("stopSequences", options.StopSequences);

            return config;
        }


        /// <summary>
        /// Extracts the model response string while avoiding other objects
        /// Gemini splits the answer in parts -> concatenates the text ones (thought parts skipped)
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string ExtractResponse(GenerateContentResponse rawResponse)
        {
            var model_response = new StringBuilder();

            try
            {
                if (rawResponse.Candidates == null || rawResponse.Candidates.Count == 0)
                    return "";

                var firstCandidate = rawResponse.Candidates[0];

                if (firstCandidate.Content == null || firstCandidate.Content.Parts == null)
                    return "";

                foreach (var part in firstCandidate.Content.Parts)
                {
                    if (part.Text != null && part.Thought != true)
                        model_response.Append(part.Text);
                }

                return model_response.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"[ERROR] The response received from the network call doesn't meet the correct parameters: {ex}");
            }
        }


        /// <summary>
        /// Extracts the model response in the tool usage domain, avoiding other objects
        /// Gemini returns no call id -> generates one to keep the tool loop consistent
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<ToolCallInvocation> ExtractToolInfo(GenerateContentResponse rawResponse)
        {
            var toolList = new List<ToolCallInvocation>();

            try
            {
                if (rawResponse.Candidates == null)
                    return toolList;

                foreach (var candidate in rawResponse.Candidates)
                {
                    if (candidate.Content == null || candidate.Content.Parts == null)
                        continue;

                    foreach (var part in candidate.Content.Parts)
                    {
                        if (part.FunctionCall != null)
                        {
                            var tool = new ToolCallInvocation
                            {
                                Type = "function_call",
                                CallId = $"call_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                                ToolName = part.FunctionCall.Name,
                                Arguments = part.FunctionCall.Args ?? new Dictionary<string, object>()
                            };

                            toolList.Add(tool);
                        }
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
        /// Builds tools schemas in Gemini format
        /// </summary>
        /// <param name="invocation"></param>
        /// <returns></returns>
        private List<GoogleFunctionDeclaration> BuildToolSchema(ILlmInvocation invocation)
        {
            var toolSchemaList = new List<GoogleFunctionDeclaration>();

            var builder = new GoogleAIToolSchemaBuilder();

            if (invocation.Tools.RegisteredTools != null)
            {
                foreach (var tool in invocation.Tools.RegisteredTools)
                    toolSchemaList.Add(builder.Build(tool));
            }

            return toolSchemaList;
        }


        /// <summary>
        /// Gemini has no metadata echo like OpenAI -> surfaces finish and block reasons
        /// </summary>
        /// <param name="rawResponse"></param>
        /// <returns></returns>
        private Dictionary<string, object> ExtractMetadata(GenerateContentResponse rawResponse)
        {
            var metadata = new Dictionary<string, object>();

            if (rawResponse.Candidates != null && rawResponse.Candidates.Count > 0 && rawResponse.Candidates[0].FinishReason != null)
                metadata.Add("google.finishReason", rawResponse.Candidates[0].FinishReason);

            if (rawResponse.PromptFeedback != null && rawResponse.PromptFeedback.BlockReason != null)
                metadata.Add("google.blockReason", rawResponse.PromptFeedback.BlockReason);

            return metadata;
        }

    }
}
