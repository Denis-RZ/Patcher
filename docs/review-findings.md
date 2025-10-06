# Universal Code Patcher – Initial Audit

## Repository Structure Overview
- **UniversalCodePatcher** – Core library that provides service container, module loading, diff engine helpers, workflow scaffolding, and shared models.
- **UniversalCodePatcher.GUI** – WinForms desktop client wiring UI widgets to the core services. `MainForm` wires up module loading and patch/diff operations.
- **UniversalCodePatcher.Avalonia** – Cross-platform UI prototype with Avalonia XAML windows and dialog implementations.
- **UniversalCodePatcher.CLI** – Proof-of-concept command-line entry point that applies canned patches to sample files.
- **UniversalCodePatcher.Tests** – xUnit test suite that targets service registration, diff parsing/applying, module behavior, and patch workflows.

## Key Logic Highlights
- The **module system** is centered on `ModuleManager`, which discovers implementations of `IModule`, instantiates them via `Activator`, and lets modules register themselves in the shared `ServiceContainer` (a thin wrapper around `Microsoft.Extensions.DependencyInjection`).【F:UniversalCodePatcher/Core/ModuleManager.cs†L1-L200】【F:UniversalCodePatcher/Core/ServiceContainer.cs†L1-L88】
- The **JavaScript module** parses source code with *Esprima*, extracts `CodeElement` metadata, and applies structural patches by rewriting the captured node spans.【F:UniversalCodePatcher/Modules/JavaScriptModule/JavaScriptModule.cs†L1-L238】
- The **C# module** leverages Roslyn APIs to analyze syntax trees, build semantic signatures, and perform transformations (replace, insert, signature change, etc.) while re-validating the compilation after edits.【F:UniversalCodePatcher/Modules/CSharpModule/CSharpModule.cs†L1-L341】
- The **diff subsystem** contains lightweight line-by-line differencing and a `UnifiedDiffParser`, but `DiffModule` currently exposes only rudimentary plumbing around those helpers.【F:UniversalCodePatcher/Modules/DiffModule/DiffModule.cs†L1-L75】【F:UniversalCodePatcher/DiffEngine/UnifiedDiffParser.cs†L1-L76】
- The **WinForms GUI** bootstraps services, loads built-in modules, and maps them onto UI actions (project tree, rule grid, patch preview).【F:UniversalCodePatcher.GUI/Forms/MainForm.cs†L1-L164】

## Notable Issues & Recommended Fix Tasks
1. **Duplicate module-load errors during startup** – `ModuleManager.LoadModulesFromPath` calls `LoadModulesFromAssembly(Assembly.GetExecutingAssembly())` *inside* the search-path loop. With multiple search paths (the default configuration adds two), modules defined in the main assembly are loaded twice, triggering the “Module … is already loaded” error callback even though nothing is wrong. Move the executing-assembly scan outside the loop or guard it so it runs only once.【F:UniversalCodePatcher/Core/ModuleManager.cs†L41-L116】
2. **Unified diff generation lacks proper hunks/line metadata** – `DiffModule.CreateUnifiedDiff` hardcodes a single `@@ -1 +1 @@` header and simply dumps the raw algorithm output. It ignores actual line offsets, omits context lines, and breaks when changes exceed one chunk. Refactor to compute real hunk headers from the diff algorithm (or adopt an existing diff formatter) so downstream tooling receives valid unified diffs.【F:UniversalCodePatcher/Modules/DiffModule/DiffModule.cs†L20-L44】
3. **Diff application logic ignores inserts/deletes** – `DiffModule.ApplyUnifiedDiff` filters diff lines down to `-`/`+` prefixes and assumes they arrive as strictly alternating replacement pairs. Real unified diffs include standalone deletions, insertions, context, and chunk headers, so the current logic silently drops many edits. Replace the custom loop with a parser-driven approach (reusing `UnifiedDiffParser`) that respects hunks and applies additions/removals at the indicated line positions.【F:UniversalCodePatcher/Modules/DiffModule/DiffModule.cs†L46-L69】
4. **File-diff metadata stubbed out** – `DiffModule.CreateFileDiff` always returns a `DiffStatus.Modified` record without populating `Hunks`, line content, or even distinguishing added/deleted files. Flesh this out by comparing the inputs and filling the `FileDiff` structure (status, hunks, diff lines) so the GUI and reports can display meaningful results.【F:UniversalCodePatcher/Modules/DiffModule/DiffModule.cs†L71-L75】【F:UniversalCodePatcher/Models/DiffModels.cs†L1-L89】
5. **Project comparison API is unimplemented** – `DiffModule.CompareProjects` currently returns an empty sequence, preventing multi-file diff workflows. Implement directory traversal and per-file diff generation (honoring new/deleted files) so callers receive actionable project-level diffs.【F:UniversalCodePatcher/Modules/DiffModule/DiffModule.cs†L77-L83】

These findings can be converted into task tickets for subsequent remediation.
