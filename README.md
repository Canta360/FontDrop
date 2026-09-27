# FontDrop

FontDrop is a small Windows utility for reviewing and batch-installing fonts.

- Accepts individual font files, ZIP archives, and folders (including subfolders)
- Shows a live preview and lets the user select individual fonts
- Skips fonts already registered for the selected scope
- Supports current-user installation by default and an elevated all-users mode
- Japanese and English UI; the initial language follows Windows, and can be changed in the app

Supported formats: `.ttf`, `.otf`, `.ttc`, `.fon`, `.fnt`.

The source targets the .NET Framework WinForms APIs available on Windows. The repository build used the framework C# compiler; no third-party packages are required.
