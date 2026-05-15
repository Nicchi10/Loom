# Loom - C# port

> Status: :construction: **work in progress** - scaffolding only, no code yet.

This folder will host the C# implementation of Loom. The contracts (classes, methods, dependencies) are defined once in the [UML diagrams](../../docs/architecture); this port will conform to them, just expressed in modern C# idioms.

## Planned layout

```
implementations/csharp/
├── Loom.sln
├── Directory.Build.props          <- shared TargetFramework, Nullable, LangVersion
├── src/
│   ├── Loom.Core/
│   ├── Loom.Engine/
│   ├── Loom.Providers.OpenAI/     <- one .csproj per provider (NuGet-friendly)
│   └── Loom.Providers.GoogleAI/
├── tests/
│   ├── Loom.Core.Tests/
│   └── Loom.Engine.Tests/
└── samples/
    └── Loom.Sample.Console/
```

## Conventions for this port

- **Target:** `net8.0` (LTS).
- **Nullable reference types:** ON, project-wide.
- **JSON:** `System.Text.Json`, not Newtonsoft.
- **HTTP:** `IHttpClientFactory`, not a static `HttpClient`.
- **Records** for immutable DTOs (`RagChunk`, `ToolParameters`, `LlmResponse`, ...).
- **File-scoped namespaces** and `switch` expressions where the VB code uses `If/ElseIf` chains.
- **DI-ready:** an extension method `services.AddLoom()` for `Microsoft.Extensions.DependencyInjection`.

## Want to help?

Read the [root README](../../README.md) for the contribution workflow, then open an issue before you start.
