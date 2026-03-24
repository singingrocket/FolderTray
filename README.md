# FolderTray

A lightweight Windows system tray application for quick access to your favorite folders and files.

## Features

- 📁 **Quick Folder Access** - Add your frequently used folders to the system tray
- 🔍 **Browse Subfolders** - Navigate through folder hierarchies directly from the tray menu
- 📄 **File Icons** - Visual file type indicators with Windows shell icons
- 🚀 **Lightweight** - Minimal resource usage, runs silently in the background
- 💾 **Persistent Configuration** - Your folder list is saved and restored on restart

## Screenshots

![FolderTray Menu](screenshot.png)

## Installation

### Option 1: Download Release
1. Download the latest release from the [Releases](../../releases) page
2. Extract the files to a folder of your choice
3. Run `FolderTray.exe`

### Option 2: Build from Source
Requirements:
- .NET 8.0 SDK or later
- Windows OS

```bash
git clone https://github.com/YOUR_USERNAME/FolderTray.git
cd FolderTray/FolderTray
dotnet build -c Release
```

## Usage

1. **Launch the application** - Run `FolderTray.exe`
2. **Add folders** - Right-click the tray icon and select "Add Folder..."
3. **Browse folders** - Click on any folder to expand and see its contents
4. **Open files** - Click on any file to open it with the default application
5. **Open in Explorer** - Right-click any folder and select "Open in Explorer"

## Auto-Start on Windows Boot

To make FolderTray start automatically when Windows boots:

1. Press `Win + R`
2. Type `shell:startup` and press Enter
3. Create a shortcut to `FolderTray.exe` in the Startup folder

## Configuration

Folder paths are stored in:
```
%LocalAppData%\FolderTray\roots.txt
```

You can manually edit this file to add or remove folders.

## Building

### Debug Build
```bash
dotnet build -c Debug
```

### Release Build (Self-Contained)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The published executable will be in:
```
FolderTray/bin/Release/net8.0-windows/win-x64/publish/
```

## Technologies

- .NET 8.0
- Windows Forms
- Windows Shell API (for file icons)

## License

MIT License - feel free to use and modify as needed.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

Created with ❤️ for quick folder access

