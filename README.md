# PugNetPaint

**PugNetPaint** is a modern, lightweight digital painting application built using **.NET 10** and **WPF**. It offers a seamless, high-performance drawing experience with advanced features like smart stroke snapping and native ink serialization.

## 🚀 Key Features

### 🎨 Drawing & Editing
- **High-Fidelity Inking**: Smooth, pressure-sensitive (hardware dependent) freehand drawing.
- **Smart Snap System**:
  - **Toggleable Snapping**: Connects lines automatically to create closed shapes.
  - **Adjustable Magnet Strength**: Use the slider to control how aggressively lines snap to each other (10px - 200px range).
- **Precision Eraser**: Switch to eraser mode to remove specific stroke segments.
- **Undo History**: Full `Ctrl+Z` support for stroke management.

### 🛠️ Tools
- **Dynamic Palette**: Quick access to essential colors (Blue, Green, Red, Orange, Black).
- **Brush Sizing**: Real-time slider adjustment for stroke thickness.
- **Canvas Control**: One-click "Clear" functionality to reset the workspace.
- **Printing**: Native print support for physical output or PDF generation.

### 💾 File Management
- **Project Persistence (.isf)**: Save your work in **Ink Serialized Format** to maintain stroke data and editability.
- **Image Export**: Export to **PNG** or **JPG** for sharing (note: rasterizes content).

## 🖥️ User Interface
- **Modern Dark Theme**: Designed with a sleek, dark-mode aesthetic for reduced eye strain.
- **Maximized View**: Application opens in full-screen mode for an immersive canvas experience.
- **Glassmorphism Elements**: Subtle UI touches for a premium feel.

## 🛠️ Prerequisites

- **OS**: Windows 10/11
- **Runtime**: [.NET 10.0](https://dotnet.microsoft.com/download)
- **IDE**: Visual Studio 2022 (for building)

## 📥 Build & Install

### Automated Installer Build
Run the provided batch file to build the app and generate an MSI installer:
```powershell
.\build_installer.bat
```

### Manual Build
1. Open `PugNetPaint.sln` in Visual Studio.
2. Build solution (`Ctrl+Shift+B`).
3. Run (`F5`).

## 🤝 Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## 📄 License

MIT License.
