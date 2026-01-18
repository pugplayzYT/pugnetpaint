# PugNetPaint

**PugNetPaint** is a lightweight, Windows-based digital painting application built using **.NET 10** and **WPF (Windows Presentation Foundation)**. It provides a straightforward interface for freehand drawing with support for native ink serialization and standard image export formats.

## 🚀 Features

- **Freehand Drawing**: High-performance, low-latency inking using the WPF InkCanvas.
- **Color Selection**: Built-in palette supporting primary colors (Blue, Green, Red, Orange) and default Black.
- **Dynamic Brush Sizing**: Adjustable stroke thickness slider.
- **Stroke History**: Full Undo capabilities to revert recent actions (`Ctrl + Z` supported).
- **Project Persistence**:
  - **Save Project (.isf)**: Saves drawing data in **Ink Serialized Format**, preserving individual strokes for future editing.
  - **Load Project**: Open previously saved `.isf` files to continue working.
- **Image Export**:
  - Export drawings as flattened **PNG** or **JPG** files for sharing.
  - *Note: Exporting to image formats rasterizes the vector strokes, rendering them non-editable.*
- **Canvas Management**: One-click functionality to clear the entire workspace.
- **Printing**: Native support for printing drawings directly from the application.

## 🛠️ Prerequisites

To build and run this application, you need the following installed on your development machine:

- **OS**: Windows 10 or Windows 11
- **IDE**: [Visual Studio 2022](https://visualstudio.microsoft.com/) (latest version recommended)
- **Framework**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- **Workload**: ".NET Desktop Development" workload installed in Visual Studio.

## 📥 Installation & Build

1. **Clone the Repository**
   ```bash
   git clone https://github.com/pugplayzYT/pugnetpaint.git
   cd pugnetpaint
   ```

2. **Open the Project**
   - Navigate to the cloned directory.
   - Open `PugNetPaint.sln` in Visual Studio 2022.

3. **Build the Solution**
   - Ensure the solution configuration is set to `Debug` or `Release`.
   - Press `Ctrl + Shift + B` to build the solution.
   - Verify that there are no build errors in the "Error List" window.

4. **Run the Application**
   - Press `F5` to start debugging or `Ctrl + F5` to run without the debugger.

## 📦 Installer (Optional)

The solution includes a `PugNetPaintInstaller` project for generating an MSI installer.

1. Ensure the **WiX Toolset** extension is installed for Visual Studio.
2. Right-click the `PugNetPaintInstaller` project in the Solution Explorer.
3. Select **Build**.
4. The generated `.msi` file will be located in the `bin/Debug` or `bin/Release` folder of the installer project.

## 🤝 Contributing

Contributions are welcome! Please ensure that you adhere to the existing code style and strictly type your C# code.

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/NewFeature`).
3. Commit your changes.
4. Push to the branch.
5. Open a Pull Request.

## 📄 License

This project is open-source and available for educational and personal use.
