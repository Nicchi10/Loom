# Loom - VB.NET implementation

The original Loom implementation, the one the UML was first drawn against. It is being superseded by the [C# reference implementation](../csharp/README.md): for the full architecture walkthrough (Core / Engine / Providers and the data flow) read that one, this README keeps only what is specific to building and running the VB port.

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
