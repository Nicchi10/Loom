# Loom

> A provider-agnostic framework for **RAG, Tool-calling and Agent orchestration** — built for the languages that the AI hype train forgot to stop at.

> Born because my boss was afraid of, and I quote, "integrating Python into .NET". :skull:

<p align="center">
  <img src="images/banner.png" alt="Loom — RAG, Tooling and Agent orchestration for the languages the AI hype train forgot" width="800" />
</p>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![VB.NET](https://img.shields.io/badge/VB.NET-5C2D91?logo=dotnet&logoColor=white)](implementations/vb)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](implementations/csharp)

---

## 1. What Loom is (and why it exists)

If you want to build an LLM-powered application today, the unwritten rule is simple: open a Python file, `pip install` half of PyPI, and pray that the next breaking change of your favourite framework doesn't land before Friday. The JavaScript/TypeScript world has its own well-stocked shelf too. Everyone else? Welcome to the wasteland.

**Loom is the alternative for the rest of us.**

The goal of the project is to provide a clean, opinionated, **framework-grade abstraction** for RAG, Tooling and Agent workflows in languages that the AI ecosystem has politely ignored - the "boring enterprise" languages that still pay the bills of half the planet.

Loom is **language-agnostic by design**: the architecture lives once in the UML diagrams under [`docs/architecture/`](docs/architecture), and every implementation under [`implementations/`](implementations) is required to conform to it. Same classes, same dependencies, same public surface - just expressed in the local idioms of each language.

What Loom gives you, regardless of which port you pick:

- A **single invocation model** (`LlmInvocation`) that holds conversation, RAG, memory, tools, generation options and execution hints in one coherent object.
- A **provider-agnostic engine** (`LoomClient`) that orchestrates validation, routing, tool-calling loops and response assembly.
- **Pluggable provider adapters** (OpenAI Responses API and Google Gemini ship in the box; many more are planned, all possible without touching the engine).
- A **tool registry** with depth control, so your agent can't accidentally enter `while True:` and bankrupt you in a single afternoon.
- **RAG ingestion** with score-based ranking and pluggable injection strategies.
- **Memory modes** ranging from "remember nothing" to "summarise everything", because not every chatbot needs the conversational equivalent of a hoarder's basement.

Loom is not trying to be LangChain. Loom is trying to be **the thing you would have written yourself, if you had three months and a strong opinion about layering.**

---

## 2. Repository layout

```
Loom/
├── README.md                       <- you are here (project overview)
├── images/                        
├── docs/
│   └── architecture/               <- UML diagrams - the single source of truth
│       ├── Core.txt
│       ├── Engine.txt
│       ├── Providers.txt
│       └── z_Loom.txt              <- consolidated diagram (Or at least I hope)
└── implementations/
    ├── vb/                         <- VB.NET (reference implementation)
    └── csharp/                     <- C# port (work in progress)
```

The architecture (UML) lives once at the top. Each `implementations/<language>/` folder owns a self-contained build, its own README and its own samples — but the public API and the dependency direction are dictated by the diagrams, not by whoever clones the repo first.

Inside every implementation, the three-layer split is mandatory:

```
Loom.<solution>
├── Loom.Core         -> Pure domain: enums, interfaces, models, validation
├── Loom.Engine       -> Orchestration: client, managers, infrastructure
└── Loom.Providers    -> Concrete adapters (one per LLM vendor)
```

Dependency direction is strict and one-way: **Providers -> Engine -> Core**. Core knows nothing about HTTP, JSON, or who is going to bill your credit card.

---

## 3. Implementations

| Language | Status | Location | Notes |
|---|---|---|---|
| **VB.NET** | :star: Reference | [`implementations/vb`](implementations/vb) | The original. The UML was drawn against this. `netstandard2.0`. |
| **C#** | :construction: Scaffolding | [`implementations/csharp`](implementations/csharp) | Layout decided, code coming. `net8.0`, `System.Text.Json`, DI-ready. |
| **Java** | :dart: Planned | - | Open issue if you want to claim it. |
| **Your language** | :moyai: You tell us | - | See *Contributing* below. |

Each implementation has its own README with quickstart, build instructions and code walkthrough. Start there if you want to actually run something.

---

## 4. The contracts — read these before writing code

The architecture is defined in PlantUML files under [`docs/architecture/`](docs/architecture):

- [`Core.txt`](docs/architecture/Core.txt) - enums, interfaces, models, validation.
- [`Engine.txt`](docs/architecture/Engine.txt) - orchestrator, context managers, infrastructure.
- [`Providers.txt`](docs/architecture/Providers.txt) - provider adapters and their schema builders.
- [`z_Loom.txt`](docs/architecture/z_Loom.txt) - the consolidated view of all three.

These diagrams are not decoration. **They are the source of truth.** Every implementation must match them; every change to the codebase must be reflected in them first. If the diagram and the code disagree, the diagram wins.

---

## 5. Contributing

Contributions are not just welcome - they are the reason this project exists. The "ignored language" list is long, and Loom's job is to shrink it.

### Step 1 — State the problem

Open an issue describing **what you want to solve or build**. One of three flavours is expected:

1. **A bug or a concrete pain point** in an existing implementation. Include a minimal reproduction and which `implementations/<language>/` is affected.
2. **A feature** that fits within the existing scope (a new memory mode, a new injection strategy, a new tool, a smarter router…). Explain *why* it belongs in Loom and not in user code, and which implementation(s) it targets.
3. **A new development direction** - typically a **new language port** (C#, Java, Kotlin, Rust, you name it). The VB.NET wing is taken; pick another. Explain the target ecosystem, the language idioms you want to respect, and confirm that the public API will match the UML.

If you can't write the problem statement in two paragraphs, the problem isn't ready yet.

### Step 2 — Update the UML *first*

The diagrams under [`docs/architecture/`](docs/architecture) are the **source of truth** across every language port. Every structural change must land there before (or alongside) the code:

- Adding a class? Add it to the matching diagram (`Core.txt`, `Engine.txt`, `Providers.txt`) - and to the consolidated [`z_Loom.txt`](docs/architecture/z_Loom.txt).
- Adding a method or a public property? Update the class block.
- Changing a relationship (implements, depends on, …)? Update the arrows.
- Adding a brand-new module (a new package, a new provider, a new language target)? Add a new PlantUML file and link it from the consolidated diagram.

A pull request that changes the code without updating the UML will be sent back. A pull request that updates the UML without touching the code is fine - sometimes design has to come first.

### Step 3 — Implement, respecting the layering rules

Inside every `implementations/<language>/` folder, the three-layer split is mandatory:

- `Loom.Core` depends on nothing but the language's base library. Do not import HTTP, JSON or any provider-specific type into Core.
- `Loom.Engine` depends on `Loom.Core`. Orchestration logic goes here.
- `Loom.Providers` depends on `Loom.Engine` and `Loom.Core`. Provider-specific code goes here and **only** here.
- A new port must mirror this same three-layer split. If your language calls them differently (modules, packages, crates, gems…), that's fine - but the boundary must be respected, and the public surface must match the UML.

### Step 4 — Open the PR

Include in the description:

- A link to the originating issue.
- A short summary of the change and the impacted implementation(s).
- The updated UML files in the same PR (no follow-ups, no "I'll do it later").
- A note for any `NotImplementedException` (or equivalent) you intentionally left behind, with a justification.

### A word for our **vibe-coding** friends

You are welcome here. Truly. But please:

- **Read the UML before writing the code.** It will save both of us a review round.
- **Do not paste 800 lines of generated code** into a PR with the message *"works on my machine"*. We can tell. Everyone can tell.
- **If you don't know what a class does, ask** — don't add a sibling class "just in case". Loom has a small surface on purpose; let's keep it that way.
- A model can write the code. It cannot write the *reason*. The reason is your job, and it's the part that actually matters.

Welcome to Loom. Now go open an issue.
