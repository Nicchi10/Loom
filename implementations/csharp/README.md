# Loom - C#

> Status: **implemented OpenAI provider**

This folder will host the C# implementation of Loom. The contracts (classes, methods, dependencies) are defined once in the [UML diagrams](../../docs/architecture); this port will conform to them, just expressed in modern C# idioms.

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

## How to use 

Below is an example of an implementation with logging using function calling:

```
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

```
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

## Conventions for this port

- **Target:** `net8.0` (LTS)
- **Nullable reference types:** ON, project-wide
- **JSON:** `System.Text.Json`, not Newtonsoft
- **HTTP:** `IHttpClientFactory`, not a static `HttpClient`
- **Records** for immutable DTOs (`RagChunk`, `ToolParameters`, `LlmResponse`, ...)
- **File-scoped namespaces** and `switch` expressions where the VB code uses `If/ElseIf` chains
- **DI-ready:** an extension method `services.AddLoom()` for `Microsoft.Extensions.DependencyInjection`

## Want to help?

Read the [root README](../../README.md) for the contribution workflow, then open an issue before you start.
