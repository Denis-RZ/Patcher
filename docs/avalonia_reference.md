# Avalonia Reference Implementation

This directory contains a minimal Avalonia UI for **Universal Code Patcher**. The project is located at `UniversalCodePatcher.Avalonia` and can be started on Linux with:

```bash
cd UniversalCodePatcher.Avalonia
dotnet run
```

## Features

- Menu with basic project actions (New, Open, Save, Exit, Undo/Redo, About).
- Tree view for project files.
- Tab control with **Source**, **Preview** and **Rules** pages.
- Simple results panel and status text.

The UI hooks into the existing business logic through the shared `UniversalCodePatcher` library. File selection uses the built-in storage provider and the tree is populated recursively from the chosen directory.

## Differences from WinForms

- Uses Avalonia XAML layouts instead of WinForms designers.
- Status bar and group boxes are implemented using simple `Border` elements because Avalonia does not provide built‑in equivalents.
- Data grid control comes from the `Avalonia.Controls.DataGrid` package and is referenced using the `dg:` namespace prefix.
- Cross‑platform storage dialogs (`OpenFolderPickerAsync`) are used in place of `FolderBrowserDialog`.

## Notes for WinForms Fixes

- Decouple business logic from `System.Windows.Forms` specific classes so both UIs can reuse the same services.
- Ensure path handling uses `Path.Combine` and `Path` APIs for Linux compatibility.
- Review dialog usage: replace hard coded Windows dialogs with abstractions to allow alternative implementations.

This Avalonia project serves as a lightweight reference for the desired layout and integration points.
