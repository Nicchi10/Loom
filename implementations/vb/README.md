# Loom - VB.NET reference implementation

The original Loom implementation. This is the one the UML was drawn against, if a future port disagrees with this code on a behaviour, this is the version that "wins" until the UML says otherwise.

> Looking for the project overview, contribution rules or UML diagrams? Go back to the [root README](../../README.md) and the [`docs/architecture/`](../../docs/architecture) folder.

---

## Build & run

- **Target:** `netstandard2.0` for the libraries (max compatibility)
- **Solution:** [`Loom.sln`](Loom.sln) -> open in Visual Studio 2022 or build from the CLI:
  ```bash
  dotnet build Loom.sln
  ```
- **Dependencies:** only `Newtonsoft.Json 13.x`, pulled via NuGet

The three library projects:

```
implementations/vb/
--- Loom.sln
--- Loom.Core         -> Pure domain: enums, interfaces, models, validation
--- Loom.Engine       -> Orchestration: client, managers, infrastructure
--- Loom.Providers    -> Concrete adapters: OpenAI, GoogleAI
```

Dependency direction is strict and one-way: **Providers -> Engine -> Core**. Core knows nothing about HTTP, JSON, or who is going to bill your credit card.

---

## Quickstart - implementing a Tool

Before walking through every package, here's the shortest possible end-to-end example: **declaring a tool and feeding the agent a RAG context built from Loom's own source code**, so the framework can be tested by asking it questions about itself. 
The demo is the docs. The docs are the test bench. You only have to write one set of fixtures.

A tool is just a class implementing [`ITool`](Loom.Core/Interfaces/ITool.vb). Two minimal examples, both answering questions about Loom itself:

```vbnet
Imports Loom.Core.Interfaces
Imports Loom.Core.Models

' Tool #1 - given a class name, returns which Loom project owns it
Public Class ClassLocatorTool
    Implements ITool

    Public ReadOnly Property ToolName As String Implements ITool.ToolName
        Get
            Return "class_locator"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements ITool.Description
        Get
            Return "Returns the Loom project (Loom.Core / Loom.Engine / Loom.Providers) that owns a given class name"
        End Get
    End Property

    Public ReadOnly Property Parameters As List(Of ToolParameters) Implements ITool.Parameters
        Get
            Return New List(Of ToolParameters) From {
                New ToolParameters With {
                    .Name = "className",
                    .Type = "string",
                    .Description = "The Loom class to locate, e.g. 'PromptAssembler'"
                }
            }
        End Get
    End Property

    Public ReadOnly Property Required As List(Of String) Implements ITool.Required
        Get
            Return New List(Of String) From {"className"}
        End Get
    End Property

    Public ReadOnly Property Strictness As Boolean? Implements ITool.Strictness
        Get
            Return True
        End Get
    End Property

    Public Function ExecuteAsync(rawInput As Dictionary(Of String, Object)) As Task(Of String) Implements ITool.ExecuteAsync
        Dim name = rawInput("className").ToString()
        ' Replace this with a real lookup against the codebase / index
        Return Task.FromResult("Loom.Engine")
    End Function
End Class

' Tool #2 - given an interface name, returns the file path that defines it
Public Class InterfaceFinderTool
    Implements ITool

    Public ReadOnly Property ToolName As String Implements ITool.ToolName
        Get
            Return "interface_finder"
        End Get
    End Property

    Public ReadOnly Property Description As String Implements ITool.Description
        Get
            Return "Returns the relative file path where a given Loom interface is defined."
        End Get
    End Property

    Public ReadOnly Property Parameters As List(Of ToolParameters) Implements ITool.Parameters
        Get
            Return New List(Of ToolParameters) From {
                New ToolParameters With {
                    .Name = "interfaceName",
                    .Type = "string",
                    .Description = "Interface name, e.g. 'IProviderAdapter'"
                }
            }
        End Get
    End Property

    Public ReadOnly Property Required As List(Of String) Implements ITool.Required
        Get
            Return New List(Of String) From {"interfaceName"}
        End Get
    End Property

    Public ReadOnly Property Strictness As Boolean? Implements ITool.Strictness
        Get
            Return True
        End Get
    End Property

    Public Function ExecuteAsync(rawInput As Dictionary(Of String, Object)) As Task(Of String) Implements ITool.ExecuteAsync
        Dim name = rawInput("interfaceName").ToString()
        ' Replace this with a real lookup against the codebase / index
        Return Task.FromResult("implementations/vb/Loom.Core/Interfaces/IProviderAdapter.vb")
    End Function
End Class
```

Wiring these into a `LoomClient` is then a five-line affair:

```vbnet
Dim client As New LoomClient()
client.RegisterProvider(New OpenAIAdapter(apiKey, New PromptAssembler()))
client.Tool.RegisterTool(New ClassLocatorTool())
client.Tool.RegisterTool(New InterfaceFinderTool())

client.Conversation.AddUserRequest("Which project owns PromptAssembler, and where is IProviderAdapter declared?")
Dim response = Await client.SendAsync()
```

Yes, the idea is to ask Loom something about Loom. As soon as someone has the patience to chunk the source tree, it will become a proper "testing dataset".

---

## `Loom.Core` - the contract layer

UML reference: [`docs/architecture/Core.txt`](../../docs/architecture/Core.txt)

This project defines the shape of every concept Loom manipulates. Nothing here knows how to talk to a model, it only knows what a conversation, a tool, a chunk and a validation result look like.

**Enums** ([Loom.Core/Enums](Loom.Core/Enums)) - the four canonical knobs of the framework:

- `MessageRole` -> `System | User | Assistant | Tool`
- `MemoryMode` -> `None | FullHistory | Summary | Extractive | Hybrid`
- `InjectionStrategy` -> how RAG context is woven into the prompt (`Sectioned`, `SystemXML`, `SystemJSON`)
- `ExecutionPriority` -> `Balanced | LatencyOptimized | CostOptimized | QualityOptimized`, used by the router when no explicit model is requested

**Interfaces** ([Loom.Core/Interfaces](Loom.Core/Interfaces)) - the contracts that keep the architecture honest:

- [`ILlmInvocation`](Loom.Core/Interfaces/ILlmInvocation.vb) -> the master object, exposing `Conversation`, `Rag`, `Memory`, `Tools`, `Options`, `Hints` and a `Validate()` method
- [`IProviderAdapter`](Loom.Core/Interfaces/IProviderAdapter.vb) -> what every provider must implement: a `ProviderName`, an `ExecuteAsync(invocation)` and a `SupportsCapability(name)`
- [`ITool`](Loom.Core/Interfaces/ITool.vb) -> the contract for any function the model can call: a name, a description, a parameter schema, a `Required` list, optional strictness, and an `ExecuteAsync(rawInput)`
- [`IAssembler`](Loom.Core/Interfaces/IAssembler.vb) -> abstracts how raw invocations are turned into the typed message list that adapters consume
- [`IValidationResult`](Loom.Core/Interfaces/IValidationResult.vb) -> the boring but essential success/error pair

**Models** ([Loom.Core/Models](Loom.Core/Models)) - the concrete data carriers, highlights:

- [`LlmInvocation`](Loom.Core/Models/LlmInvocation.vb) -> the single source of truth, implementing `ILlmInvocation` and delegating its `Validate()` to `InvocationValidator`
- [`ConversationState`](Loom.Core/Models/ConversationState.vb) -> `TraceId`, `SystemPrompt`, optional `TokenBudget`, the message buffer and a `TurnIndex`
- [`Message`](Loom.Core/Models/Message.vb) -> role, content, and the tool-call fields (`ToolCallId`, `ToolName`, `ToolArgs`) needed to reconstruct multi-turn function calling
- [`RagContext`](Loom.Core/Models/RagContext.vb) + [`RagChunk`](Loom.Core/Models/RagChunk.vb) -> the retrieval payload, with per chunk scores and a `MaxTokens` budget
- [`ToolContext`](Loom.Core/Models/ToolContext.vb), [`ToolDefinition`](Loom.Core/Models/ToolDefinition.vb), [`ToolParameters`](Loom.Core/Models/ToolParameters.vb), [`ToolCallInvocation`](Loom.Core/Models/ToolCallInvocation.vb) -> everything needed to declare a tool, expose it to the model, and parse the model's reply
- [`LlmResponse`](Loom.Core/Models/LlmResponse.vb) -> the normalised return type: id, model used, role, content, tool calls and token usage. Every provider adapter must converge here
- [`ValidationResult`](Loom.Core/Models/ValidationResult.vb) -> implements `IValidationResult` with the conventional `Success()` / `Failure(errors)` factory pair

**Validation** ([`InvocationValidator`](Loom.Core/InvocationValidator.vb)) - runs the four sanity checks every developer forgets at least once:

1. The conversation is not empty
2. The RAG budget is strictly smaller than the total token budget
3. Every registered tool has a parameter schema
4. A system prompt (or system message) actually exists

If anything fails, you get a `ValidationResult.Failure(errors)`. If everything passes, the engine proceeds without a silent shrug.

---

## `Loom.Engine` - the orchestrator

UML reference: [`docs/architecture/Engine.txt`](../../docs/architecture/Engine.txt)

This is the brain, it composes Core models into actual behaviour.

**Assemblers** - [`PromptAssembler`](Loom.Engine/Assemblers/PromptAssembler.vb) implements `IAssembler` and is responsible for:

- `Assemble(invocation)` -> copies the raw message list, finds (or creates) the system message and injects the context block produced by `BuildContextBlock`
- `AssembleInputItems(invocation)` -> produces a provider-neutral `List(Of AssembledItem)`, tool messages are split into the canonical pair `function_call` + `function_call_output` so adapters don't have to re-derive the structure
- `BuildContextBlock(invocation)` -> currently emits a `### LONG-TERM MEMORY ###` and a `### CONTEXT ###` section, the `InjectionStrategy` enum is wired up to allow XML/JSON variants in future iterations
- `AddContent(original, context)` -> concatenation with a sane separator, that's it... no need to make it complicated

**Context managers** ([Loom.Engine/Context](Loom.Engine/Context)) - one per concern, each holding a reference to the same `LlmInvocation`:

- [`ConversationManager`](Loom.Engine/Context/ConversationManager.vb) -> `AddMessage`, `AddUserRequest`, `AddToolResult`, `ResetHistory(keepSystemMessage)`, increments `TurnIndex` on every message
- [`RagManager`](Loom.Engine/Context/RagManager.vb) -> `AddResults(chunks, clearExisting)` filters empty chunks and sorts by descending score, `SetStrategy` and `HasContext` round it out
- [`MemoryManager`](Loom.Engine/Context/MemoryManager.vb) -> `UpdateMemory(newData)` dispatches on `MemoryMode`... `None`, `FullHistory` and `Summary` are wired, `Extractive` and `Hybrid` deliberately throw `NotImplementedException` (open contribution)
- [`ToolRegistry`](Loom.Engine/Context/ToolRegistry.vb) -> `RegisterTool(ITool)` indexes the tool by name, builds its `ToolDefinition`, and pushes it into the invocation, `ExecuteAsync(name, args)` dispatches and returns a string including a structured error string if the tool is missing or throws, so the model can recover instead of the process crashing

**Infrastructure** ([Loom.Engine/Infrastructure](Loom.Engine/Infrastructure)):

- [`TokenCounter`](Loom.Engine/Infrastructure/TokenCounter.vb) -> heuristic `length / 4` count, summed over messages, RAG chunks and memory... yes, it's an approximation... yes, it should eventually be a real tokenizer... no, it won't accidentally let your context double its budget today
- [`ModelCatalog`](Loom.Engine/Infrastructure/ModelCatalog.vb) -> the in-memory list of known models with `ProviderName`, `ContextWindow`, `CostLevel`. Provides `GetModelInfo(modelId)` and `GetFallBack(priority)`, the #1 issue was opened "in his honor"
- [`ExecutionRouter`](Loom.Engine/Infrastructure/ExecutionRouter.vb) -> `RegisterAdapter(adapter)` keeps the list unique by provider name, `Route(invocation)` resolves the preferred model (or asks the catalog for a fallback by priority) and returns the matching adapter, raising if no provider is installed for it
- [`ProviderHttpClient`](Loom.Engine/Infrastructure/ProviderHttpClient.vb) -> a single shared `HttpClient` with `NullValueHandling.Ignore` JSON serialisation, used by every adapter

**The client** - [`LoomClient`](Loom.Engine/LoomClient.vb) is the public entry point:

```vbnet
Public Async Function SendAsync() As Task(Of LlmResponse)
    ' 1. Formal validation
    ' 2. Route to a provider adapter
    ' 3. Tool-calling loop ("semaforo"), each round:
    '       - Re-check the token budget
    '           (round 0 over budget -> throw; a later round over budget -> stop gracefully)
    '       - Execute adapter, append assistant content (if any)
    '       - GREEN: no tool calls -> return the answer
    '       - RED: round >= MaxToolDepth -> return the last response (depth ceiling)
    '       - YELLOW: execute every tool, append the results, next round
End Function
```

That loop is the whole "agent" pattern: no decorators, no callbacks-in-callbacks, no DSL. Just a `Do...Loop`. If it really holds up, I'll buy everyone a drink.

---

## `Loom.Providers` - the adapters

UML reference: [`docs/architecture/Providers.txt`](../../docs/architecture/Providers.txt)

Each adapter implements `IProviderAdapter` and is responsible for translating the neutral invocation into the provider's native dialect.

- [`OpenAI/OpenAIAdapter`](Loom.Providers/OpenAI/OpenAIAdapter.vb) -> talks to `https://api.openai.com/v1/responses`, uses [`OpenAIToolSchemaBuilder`](Loom.Providers/OpenAI/OpenAIToolSchemaBuilder.vb) to convert `ToolDefinition` into [`ProviderToolSchema`](Loom.Providers/Core/ProviderToolSchema.vb), maps assembled items into the `function_call` / `function_call_output` shape OpenAI expects, parses [`ModelResponse`](Loom.Providers/OpenAI/ModelResponse.vb) back into a normalised `LlmResponse`
- [`GoogleAI/GoogleAIAdapter`](Loom.Providers/GoogleAI/GoogleAIAdapter.vb) -> talks to `generativelanguage.googleapis.com/v1beta`, translates the same neutral input into Gemini's `contents` / `system_instruction` / `function_declarations` layout, deserialising the reply into [`GenerateContentResponse`](Loom.Providers/GoogleAI/GenerateContentResponse.vb) before normalising

Both adapters converge on the same `LlmResponse` so the `LoomClient` loop never has to know which provider it just spoke to.

---

## The data flow in a nutshell

A caller builds an `LlmInvocation`, registers one or more `ITool`s and at least one `IProviderAdapter` on a `LoomClient`. 

On `SendAsync()`, the client validates the invocation, asks the `ExecutionRouter` for a provider, and enters the tool-call loop, which re-checks the token budget on every round before each adapter call. 

The chosen adapter delegates message preparation to the `PromptAssembler`, calls the provider over `ProviderHttpClient`, and returns a normalised `LlmResponse`. 

If the response contains tool calls, the `ToolRegistry` executes each tool, the `ConversationManager` records the results, and the loop continues until the model produces a tool-free reply or `MaxToolDepth` is reached. 

That's the whole story.
