# Universal Code Patcher

Intelligent multi-language code patcher with modular architecture.

## Features

- **Modular Architecture**: Easy to extend with new language modules
- **Semantic Code Analysis**: Advanced AST-based parsing (not regex!)
- **Multiple Languages**: JavaScript, TypeScript, C# support
- **Diff Engine**: Create and apply unified diffs without Git dependency
- **Visual Interface**: Professional WinForms GUI
- **Cross Platform UI**: Reference Avalonia version for Linux
- **Backup System**: Automatic backups before patching

## Modules

### Core Modules
- **JavaScriptModule**: JavaScript/TypeScript analysis and patching
- **CSharpModule**: C# analysis and patching (to be added)
- **DiffModule**: Unified diff creation and application (to be added)
- **BackupModule**: Backup management (to be added)

### Adding New Modules

1. Create a class that inherits from BaseModule
2. Implement required interfaces (ICodeAnalyzer, IPatcher, etc.)
3. Place in Modules folder
4. The application will auto-discover and load the module

## Building

1. Open UniversalCodePatcher.sln in Visual Studio
2. Build solution (Ctrl+Shift+B)
3. Run (F5)
4. Alternatively run the Avalonia UI with:
   ```bash
   dotnet run --project UniversalCodePatcher.Avalonia
   ```

## Usage

1. File → New Project (select project folder)
2. Check files to analyze in project tree
3. Project → Scan Files
4. Create patch rules in Rules tab
5. Apply patches with visual preview

## Requirements

- .NET 6.0 or higher
- Windows OS
- Visual Studio 2022 (for development)
