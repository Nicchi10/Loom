## Contributing



### Step 1: State the problem

Open an issue describing what you want to solve or build, one of three flavours is expected:

1. **A bug or a concrete pain point** in an existing implementation. Include a minimal reproduction and which `implementations/<language>/` is affected
2. **A feature** that fits within the existing scope (a new memory mode, a new injection strategy, a new tool, a smarter router...), explain why it belongs in Loom and not in user code, and which implementation(s) it targets
3. **A new development direction**: typically a **new language port** (C#, Java, Kotlin, Rust, you name it), the VB.NET wing is taken, pick another. Explain the target ecosystem, the language idioms you want to respect, and confirm that the public API will match the UML

If you can't write the problem statement in two paragraphs, the problem isn't ready yet.

### Step 2: Update the UML first

The diagrams under [`docs/architecture/`](docs/architecture) are the **source of truth** across every language port. Every structural change must land there before (or alongside) the code:

- Adding a class? Add it to the matching diagram (`Core.txt`, `Engine.txt`, `Providers.txt`) and to the consolidated [`z_Loom.txt`](docs/architecture/z_Loom.txt)
- Adding a method or a public property? Update the class block
- Changing a relationship (implements, depends on, ...)? Update the arrows
- Adding a brand-new module (a new package, a new provider, a new language target)? Add a new PlantUML file and link it from the consolidated diagram

A pull request that changes the code without updating the UML will be sent back. A pull request that updates the UML without touching the code is fine, sometimes design has to come first.

### Step 3: Implement, respecting the layering rules

Inside every `implementations/<language>/` folder, the three-layer split is mandatory:

- `Loom.Core` depends on nothing but the language's base library. Do not import HTTP, JSON or any provider-specific type into Core
- `Loom.Engine` depends on `Loom.Core`. Orchestration logic goes here
- `Loom.Providers` depends on `Loom.Engine` and `Loom.Core`. Provider-specific code goes here and **only** here
- A new port must mirror this same three-layer split. If your language calls them differently (modules, packages, crates, gems...), that's fine, but the boundary must be respected, and the public surface must match the UML

### Step 4: Open the PR

Include in the description:

- A link to the originating issue
- A short summary of the change and the impacted implementation(s)
- The updated UML files in the same PR (no follow-ups, no *I'll do it later*)
- A note for any `NotImplementedException` (or equivalent) you intentionally left behind, with a justification

### A word for our **vibe-coding** friends

You are welcome here, truly, but please:

- **Read the UML before writing the code**: it will save both of us a review round
- **Do not paste 800 lines of generated code** into a PR with the message *"works on my machine"*. We can tell. Everyone can tell
- **If you don't know what a class does, ask**: don't add a sibling class "just in case". Loom has a small surface on purpose, let's keep it that way
- A model can write the code, but it cannot write the reason, the reason is your job, and it's the part that actually matters

> We only look at a PR opened directly by Claude if the model is Aeon 5.9

Welcome to Loom. Now go open an issue.