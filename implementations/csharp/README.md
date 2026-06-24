# Loom - C# reference implementation

> Status: **implemented OpenAI provider**

This is the C# implementation of Loom and the one new ports should look at. The contracts (classes, methods, dependencies) are defined once in the [UML diagrams](../../docs/architecture), this port conforms to them, just expressed in modern C# idioms.

## Planned layout

```
implementations/csharp/
--- Loom.sln
--- Directory.Build.props          <- shared TargetFramework, Nullable, LangVersion
--- src/
    --- Loom.Core/
    --- Loom.Engine/
    --- Loom.Providers.OpenAI/     <- one .csproj per provider (NuGet-friendly)
    --- Loom.Providers.GoogleAI/
```

Dependency direction is strict and one-way: **Providers -> Engine -> Core**. Core knows nothing about HTTP, JSON, or who is going to bill your credit card.

## Quickstart 

Below is an example of an implementation with logging using function calling:

```csharp
string openaiApiKey = YOUR_OPENAI_KEY; // Pls don't put hardcoded :(

var client = new LoomClient();
client.RegisterProvider(new OpenAIAdapter(openaiApiKey, new PromptAssembler()));

// ExecutionPriority & MemoryMode they'll be updated as soon as possible

client.Invocation.Hints.Priority = ExecutionPriority.CostOptimized; 
client.Invocation.Hints.PreferredModel = "gpt-4.1-nano"; // Really efficient dude
client.Invocation.Memory.Mode = MemoryMode.None;

string systemPrompt = "You are a helpful assistant"; // Be more creative pls


client.Conversation.AddMessage(MessageRole.System, systemPrompt);

// Look the next section
client.Tool.RegisterTool(ToolClient.HoroscopeTool);
client.Tool.RegisterTool(ToolClient.WeatherTool);

while (true)
{
    System.Console.Write("User: ");
    string userInput = System.Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        System.Console.WriteLine("Goodbye my g!");
        break;
    }

    client.Conversation.AddUserRequest(userInput);

    var response = await client.SendAsync();

    System.Console.WriteLine($"Chatbot: {response.Content}");
}

```

## Examples of tool registration

Below is an example with our ever-present `get_weather` and `get_horoscope`

```csharp
internal static class ToolClient
    {
        public static ITool HoroscopeTool { get; } = new Horoscope();
        public static ITool WeatherTool { get; } = new Weather();

        private sealed class Horoscope : ITool
        {
            public string ToolName => "get_horoscope";
            public string Description => "Gets the horoscope from provided sign";
            public bool? Strictness => false;
            public List<string> Required => new List<string> { "sign" };
            public List<ToolParameters> Parameters => new List<ToolParameters>
            {
                new ToolParameters
                {
                    Name = "sign",
                    Type = "string",
                    Description = "Provided sign, e.g. 'Aquarius'"
                }
            };

            public Task<string> ExecuteAsync(Dictionary<string, object> rawInput)
                => Task.FromResult("Good news! Your coding skills are increasing!");
        }

        private sealed class Weather : ITool
        {
            public string ToolName => "get_weather";
            public string Description => "Gets the weather from provided city";
            public bool? Strictness => false;
            public List<string> Required => new List<string> { "city" };
            public List<ToolParameters> Parameters => new List<ToolParameters>
            {
                new ToolParameters
                {
                    Name = "city",
                    Type = "string",
                    Description = "Provided city, e.g. 'Milano, New York'"
                }
            };

            public Task<string> ExecuteAsync(Dictionary<string, object> rawInput)
                => Task.FromResult("Good news! You can continue to code, it'll rain!");
        }
    }

```

---

## `Loom.Core` - the contract layer

UML reference: [`docs/architecture/Core.txt`](../../docs/architecture/Core.txt)

This project defines the shape of every concept Loom manipulates. Nothing here knows how to talk to a model, it only knows what a conversation, a tool, a chunk and a validation result look like.

**Enums** ([Loom.Core/Enums](src/Loom.Core/Enums)) - the four canonical knobs of the framework:

- `MessageRole` -> `System | User | Assistant | Tool`
- `MemoryMode` -> `None | FullHistory | Summary | Extractive | Hybrid`
- `InjectionStrategy` -> how RAG context is woven into the prompt (`Sectioned`, `SystemXML`, `SystemJSON`)
- `ExecutionPriority` -> `Balanced | LatencyOptimized | CostOptimized | QualityOptimized`, used by the router when no explicit model is requested

**Interfaces** ([Loom.Core/Interfaces](src/Loom.Core/Interfaces)) - the contracts that keep the architecture honest:

- [`ILlmInvocation`](src/Loom.Core/Interfaces/ILlmInvocation.cs) -> the master object, exposing `Conversation`, `Rag`, `Memory`, `Tools`, `Options`, `Hints` and a `Validate()` method
- [`IProviderAdapter`](src/Loom.Core/Interfaces/IProviderAdapter.cs) -> what every provider must implement: a `ProviderName`, an `ExecuteAsync(invocation)` and a `SupportsCapability(name)`
- [`ITool`](src/Loom.Core/Interfaces/ITool.cs) -> the contract for any function the model can call: a name, a description, a parameter schema, a `Required` list, optional strictness, and an `ExecuteAsync(rawInput)`
- [`IAssembler`](src/Loom.Core/Interfaces/IAssembler.cs) -> abstracts how raw invocations are turned into the typed message list that adapters consume
- [`IValidationResult`](src/Loom.Core/Interfaces/IValidationResult.cs) -> the boring but essential success/error pair

**Models** ([Loom.Core/Models](src/Loom.Core/Models)) - the concrete data carriers, highlights:

- [`LlmInvocation`](src/Loom.Core/Models/LlmInvocation.cs) -> the single source of truth, implementing `ILlmInvocation` and delegating its `Validate()` to `InvocationValidator`
- [`ConversationState`](src/Loom.Core/Models/ConversationState.cs) -> `TraceId`, `SystemPrompt`, optional `TokenBudget`, the message buffer and a `TurnIndex`
- [`Message`](src/Loom.Core/Models/Message.cs) -> role, content, and the tool-call fields (`ToolCallId`, `ToolName`, `ToolArgs`) needed to reconstruct multi-turn function calling
- [`RagContext`](src/Loom.Core/Models/RagContext.cs) + [`RagChunk`](src/Loom.Core/Models/RagChunk.cs) -> the retrieval payload, with per chunk scores and a `MaxTokens` budget
- [`ToolContext`](src/Loom.Core/Models/ToolContext.cs), [`ToolDefinition`](src/Loom.Core/Models/ToolDefinition.cs), [`ToolParameters`](src/Loom.Core/Models/ToolParameters.cs), [`ToolCallInvocation`](src/Loom.Core/Models/ToolCallInvocation.cs) -> everything needed to declare a tool, expose it to the model, and parse the model's reply
- [`LlmResponse`](src/Loom.Core/Models/LlmResponse.cs) -> the normalised return type: id, model used, role, content, tool calls and token usage. Every provider adapter must converge here
- [`ModelMetaData`](src/Loom.Core/Models/ModelMetaData.cs) -> describes a model for the router: `ProviderName`, `ContextWindow`, and the `CostLevel` / `LatencyLevel` / `QualityLevel` ranks (1-5) used to pick a fallback
- [`ValidationResult`](src/Loom.Core/Models/ValidationResult.cs) -> implements `IValidationResult` with the conventional `Success()` / `Failure(errors)` factory pair

**Validation** ([`InvocationValidator`](src/Loom.Core/InvocationValidator.cs)) - runs the four sanity checks every developer forgets at least once:

1. The conversation is not empty
2. The RAG budget is strictly smaller than the total token budget
3. Every registered tool has a parameter schema
4. A system prompt (or system message) actually exists

If anything fails, you get a `ValidationResult.Failure(errors)`. If everything passes, the engine proceeds without a silent shrug.

---

## `Loom.Engine` - the orchestrator

UML reference: [`docs/architecture/Engine.txt`](../../docs/architecture/Engine.txt)

This is the brain, it composes Core models into actual behaviour.

**Assemblers** - [`PromptAssembler`](src/Loom.Engine/Assemblers/PromptAssembler.cs) implements `IAssembler` and is responsible for:

- `Assemble(invocation)` -> copies the raw message list, finds (or creates) the system message and injects the context block produced by `BuildContextBlock`
- `AssembleInputItems(invocation)` -> produces a provider-neutral `List<AssembledItem>`, tool messages are split into the canonical pair `function_call` + `function_call_output` so adapters don't have to re-derive the structure
- `BuildContextBlock(invocation)` -> currently emits a `### LONG-TERM MEMORY ###` and a `### CONTEXT ###` section, the `InjectionStrategy` enum is wired up to allow XML/JSON variants in future iterations
- `AddContent(original, context)` -> concatenation with a sane separator, that's it... no need to make it complicated

**Context managers** ([Loom.Engine/Context](src/Loom.Engine/Context)) - one per concern, each holding a reference to the same `LlmInvocation`:

- [`ConversationManager`](src/Loom.Engine/Context/ConversationManager.cs) -> `AddMessage`, `AddUserRequest`, `AddToolResult`, `ResetHistory(keepSystemMessage)`, increments `TurnIndex` on every message
- [`RagManager`](src/Loom.Engine/Context/RagManager.cs) -> `AddResults(chunks, clearExisting)` filters empty chunks and sorts by descending score, `SetStrategy` and `HasContext` round it out
- [`MemoryManager`](src/Loom.Engine/Context/MemoryManager.cs) -> `UpdateMemory(newData)` dispatches on `MemoryMode`... `None`, `FullHistory` and `Summary` are wired, `Extractive` and `Hybrid` deliberately throw `NotImplementedException` (open contribution)
- [`ToolRegistry`](src/Loom.Engine/Context/ToolRegistry.cs) -> `RegisterTool(ITool)` indexes the tool by name, builds its `ToolDefinition`, and pushes it into the invocation, `ExecuteAsync(name, args)` dispatches and returns a string including a structured error string if the tool is missing or throws, so the model can recover instead of the process crashing

**Infrastructure** ([Loom.Engine/Infrastructure](src/Loom.Engine/Infrastructure)):

- [`TokenCounter`](src/Loom.Engine/Infrastructure/TokenCounter.cs) -> heuristic `length / 4` count, summed over messages, RAG chunks and memory... yes, it's an approximation... yes, it should eventually be a real tokenizer... no, it won't accidentally let your context double its budget today
- [`ModelCatalog`](src/Loom.Engine/Infrastructure/ModelCatalog.cs) -> the in-memory list of known models with `ProviderName`, `ContextWindow` and the `CostLevel` / `LatencyLevel` / `QualityLevel` ranks. Provides `GetModelInfo(modelId)` and `GetFallBack(priority)`, which ranks the catalog on the signal that matches the requested `ExecutionPriority` (cheapest, fastest, most capable, or a balanced score). The #1 issue was opened "in his honor"
- [`ExecutionRouter`](src/Loom.Engine/Infrastructure/ExecutionRouter.cs) -> `RegisterAdapter(adapter)` keeps the list unique by provider name, `Route(invocation)` resolves the preferred model (or asks the catalog for a fallback by priority) and returns the matching adapter, raising if no provider is installed for it
- [`ProviderHttpClient`](src/Loom.Engine/Infrastructure/ProviderHttpClient.cs) -> a single shared `HttpClient` with `System.Text.Json` serialisation (`DefaultIgnoreCondition = WhenWritingNull`, case-insensitive on read), used by every adapter

**The client** - [`LoomClient`](src/Loom.Engine/LoomClient.cs) is the public entry point:

```csharp
public async Task<LlmResponse> SendAsync()
{
    // 1. Formal validation
    // 2. Route to a provider adapter
    // 3. Tool-calling loop ("semaforo"), each round:
    //       - Re-check the token budget
    //           (round 0 over budget -> throw; a later round over budget -> stop gracefully)
    //       - Execute adapter (retried up to MaxRetryCount with exponential backoff),
    //         append assistant content (if any)
    //       - GREEN: no tool calls -> return the answer
    //       - RED: round >= MaxToolDepth -> return the last response (depth ceiling)
    //       - YELLOW: execute every tool, append the results, next round
    //
    //    Early stops (depth ceiling / budget) tag the response with
    //    Metadata["loom.stopReason"] so the caller can tell them from a normal answer.
}
```

That loop is the whole "agent" pattern: no decorators, no callbacks-in-callbacks, no DSL. Just a `while`. If it really holds up, I'll buy everyone a drink.

---

## `Loom.Providers.*` - the adapters

UML reference: [`docs/architecture/Providers.txt`](../../docs/architecture/Providers.txt)

Each provider is its own NuGet-friendly project and implements `IProviderAdapter`, translating the neutral invocation into the provider's native dialect.

- [`Loom.Providers.OpenAI`](src/Loom.Providers.OpenAI) -> talks to `https://api.openai.com/v1/responses`, uses [`OpenAIToolSchemaBuilder`](src/Loom.Providers.OpenAI/OpenAIToolSchemaBuilder.cs) to convert `ToolDefinition` into the OpenAI [tool schema](src/Loom.Providers.OpenAI/Schemas), maps assembled items into the `function_call` / `function_call_output` shape OpenAI expects, parses [`ModelResponse`](src/Loom.Providers.OpenAI/ModelResponse.cs) back into a normalised `LlmResponse`
- [`Loom.Providers.GoogleAI`](src/Loom.Providers.GoogleAI) -> scaffolded project, the Gemini adapter is the next port from the VB reference (open contribution)

Every adapter converges on the same `LlmResponse` so the `LoomClient` loop never has to know which provider it just spoke to.

---

## The data flow in a nutshell

A caller builds an `LlmInvocation`, registers one or more `ITool`s and at least one `IProviderAdapter` on a `LoomClient`. 

On `SendAsync()`, the client validates the invocation, asks the `ExecutionRouter` for a provider, and enters the tool-call loop, which re-checks the token budget on every round before each adapter call. 

The chosen adapter delegates message preparation to the `PromptAssembler`, calls the provider over `ProviderHttpClient`, and returns a normalised `LlmResponse`. 

If the response contains tool calls, the `ToolRegistry` executes each tool, the `ConversationManager` records the results, and the loop continues until the model produces a tool-free reply or `MaxToolDepth` is reached. 

That's the whole story.

---

## Conventions for this port

- **Target:** `net8.0` (LTS)
- **Nullable reference types:** ON, project-wide
- **JSON:** `System.Text.Json`, not Newtonsoft
- **HTTP:** `IHttpClientFactory`, not a static `HttpClient`
- **Records** for immutable DTOs (`RagChunk`, `ToolParameters`, `LlmResponse`, ...)
- **File-scoped namespaces** and `switch` expressions where the VB code uses `If/ElseIf` chains
- **DI-ready:** an extension method `services.AddLoom()` for `Microsoft.Extensions.DependencyInjection`

## Want to help?

Read [`CONTRIBUTING`](../../CONTRIBUTING.md) for the contribution workflow, then open an issue before you start.
