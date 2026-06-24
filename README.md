# Loom

> A provider-agnostic framework for **RAG, Tool-calling and Agent orchestration**, built for the languages that the AI hype train forgot to stop at

> Born because my boss was afraid of, and I quote, "integrating Python into .NET" :skull:

<p align="center">
  <img src="images/banner.png" alt="Loom - RAG, Tooling and Agent orchestration for the languages the AI hype train forgot" width="800" />
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![VB.NET](https://img.shields.io/badge/VB.NET-5C2D91?logo=dotnet&logoColor=white)](implementations/vb)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](implementations/csharp)

---

## 1. What Loom is (and why it exists)

If you want to build an LLM-powered application today, the unwritten rule is simple: open a Python file, `pip install` half of PyPI, and pray that the next breaking change of your favourite framework doesn't land before Friday. The JavaScript/TypeScript world has its own well-stocked shelf too. Everyone else? Welcome to the wasteland.

**Loom is the alternative for the rest of us.**

The goal of the project is to provide a clean, opinionated, **framework-grade abstraction** for RAG, Tooling and Agent workflows in languages that the AI ecosystem has politely ignored, the "boring enterprise" languages that still pay the bills of half the planet.

Loom is **language-agnostic by design**: the architecture lives once in the UML diagrams under [`docs/architecture/`](docs/architecture), and every implementation under [`implementations/`](implementations) is required to conform to it. Same classes, same dependencies, same public surface it's just expressed in the local idioms of each language.

What Loom gives you, regardless of which port you pick:

- A **single invocation model** (`LlmInvocation`) that holds conversation, RAG, memory, tools, generation options and execution hints in one coherent object
- A **provider-agnostic engine** (`LoomClient`) that orchestrates validation, routing, tool-calling loops and response assembly
- **Pluggable provider adapters** (OpenAI Responses API and Google Gemini ship in the box, many more are planned, all possible without touching the engine)
- A **tool registry** with depth control, so your agent can't accidentally enter `while True:` and bankrupt you in a single afternoon
- **RAG ingestion** with score-based ranking and pluggable injection strategies
- **Memory modes** ranging from "remember nothing" to "summarise everything", because not every chatbot needs the conversational equivalent of a hoarder's basement

Loom is not trying to be LangChain, Loom is trying to be the thing you would have written yourself, if you had three months and a strong opinion about layering.

---

## 2. Repository layout

```
Loom/
--- README.md                       <- you are here (project overview)
--- images/                        
--- docs/
    --- architecture/               <- UML diagrams - the single source of truth
        --- Core.txt
        --- Engine.txt
        --- Providers.txt
        --- z_Loom.txt              <- consolidated diagram (Or at least I hope)
--- implementations/
    --- vb/                         <- VB.NET (soon to be deprecated, was the reference implementation but now switched to C#)
    --- csharp/                     <- C# port (reference implementation coming soon)
```

The architecture (UML) lives once at the top. Each `implementations/<language>/` folder owns a self-contained build, its own README and its own samples, but the public API and the dependency direction are dictated by the diagrams, not by whoever clones the repo first.

Inside every implementation, the three-layer split is mandatory:

```
Loom.<solution>
--- Loom.Core         -> Pure domain: enums, interfaces, models, validation
--- Loom.Engine       -> Orchestration: client, managers, infrastructure
--- Loom.Providers    -> Concrete adapters (one per LLM vendor)
```

Dependency direction is strict and one-way: **Providers -> Engine -> Core**.

---

## 3. Implementations

| Language | Status | Location | Notes |
|---|---|---|---|
| **VB.NET** | :star: Reference | [`implementations/vb`](implementations/vb) | The original, the UML was drawn against this |
| **C#** | :fork_and_knife: Future Reference | [`implementations/csharp`](implementations/csharp) | At the end of the porting |
| **Java** | :dart: Planned | - | Open issue if you want to claim it |
| **Your language** | :moyai: You tell us | - | See *Contributing* below |

Each implementation has its own README with quickstart, build instructions and code walkthrough. Start there if you want to actually run something.

---

## 4. Mapped providers

| Providers | Status | Notes |
| --- | --- | --- |
| **OpenAI** | :fire: Implemented | First dropped |
| **GoogleAI** | :exclamation: Implemented | It gave meaning to the project | 
| **Claude** | :dart: Planned | <-- the dream, the problem --> :money_with_wings: | 
| **Ollama** | :dart: Planned | And all his henchLLM | 
| **Your provider** | :moyai: You tell us | - | See *Contributing* below |

---

## 5. The contracts, read these before writing code

The architecture is defined in PlantUML files under [`docs/architecture/`](docs/architecture):

- [`Core.txt`](docs/architecture/Core.txt) -> enums, interfaces, models, validation
- [`Engine.txt`](docs/architecture/Engine.txt) -> orchestrator, context managers, infrastructure
- [`Providers.txt`](docs/architecture/Providers.txt) -> provider adapters and their schema builders
- [`z_Loom.txt`](docs/architecture/z_Loom.txt) -> the consolidated view of all three

These diagrams are not decoration, **They are the source of truth**: every implementation must match them, every change to the codebase must be reflected in them first. 

If the diagram and the code disagree, the diagram wins.

---

## 6. Contributing

Contributions are not just welcome, they are the reason this project exists. The "ignored language" list is long, and Loom's job is to shrink it. 
For every details please read [`CONTRIBUTING`](CONTRIBUTING.md)

