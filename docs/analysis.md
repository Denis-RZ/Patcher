1️⃣ Modular .NET framework for patching code across languages. Modules (JavaScriptModule, CSharpModule, DiffModule, BackupModule) integrate via ModuleManager and DI container.

2️⃣ Typical workflow: select project in GUI, scan files, define PatchRule objects, preview modifications, apply patches with backups and diff generation.

3️⃣ BaseModule defines initialization/shutdown; ModuleManager handles loading; ServiceContainer provides services; JavaScriptModule and CSharpModule implement ICodeAnalyzer and IPatcher; DiffModule uses IDiffAlgorithm; PatchResult reports outcomes.

4️⃣ Integrates Roslyn for C# parsing, WinForms for GUI, and MSTest for unit testing.

5️⃣ Requires .NET 6+, Windows OS, Visual Studio 2022; ensures backups before modifying code.
