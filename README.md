![PixanKit Logo](https://img.shields.io/badge/C%23-.NET%208-blueviolet)
![LICENSE](https://img.shields.io/badge/License-MIT-blue)
![No Contributions Yet](https://img.shields.io/badge/Contributions-None%20yet-lightgrey)
![Stand With Ukraine](https://img.shields.io/badge/Stand%20With-Ukraine-0057B7?logo=united-nations&logoColor=white)
> A modular .NET 8 library for building Minecraft launchers.

# PixanKit

PixanKit is a modular, .NET 8-based toolkit crafted to streamline the development of custom Minecraft launchers and automation tools. The library is structured into three separate C# projects:

### 🧱 LaunchCore
- Core library handling player accounts, game versions, and Java runtime selection.

### 🌐 ResourceDownloader
- Modular download task system supporting progress tracking and flexible implementation.

### 🧩 ModModule
- Plug-and-play mod installer and manager with support for Forge/Fabric mod loaders.

Whether you're creating a custom launcher or automating tasks related to Minecraft, PixanKit offers a modular solution tailored to your needs.

# ✨ Features
### 🧱 LaunchCore

-   Player management:
    -   Offline and Microsoft account support.
    -   Skin caching and embedding player information.
-   Game management:
    -   Handle vanilla, OptiFine, and modded versions.
    -   Manage game directories and startup parameters.
-   Runtime management:
    -   Automatically detect and manage suitable JRE/JDK versions.

### 🌐 ResourceDownloader

-   Task-based resource server interactions.
-   Flexible and customizable downloader implementations.

### 🧩 ModController

-   Simplified mod installation and management.
-   Dependency handling for mod loaders like Forge or Fabric.

# 🚀 Getting Started
### Option 1: Add as Git Submodule

1.  Add PixanKit to your project:
`git submodule add https://github.com/your-repo/PixanKit.git`
2.  Open your solution in Visual Studio.
3.  Right-click the solution -> `Add` -> `Existing Project` -> Select `PixanKit/LaunchCore.csproj` and other necessary projects.
4.  Add a reference to the PixanKit projects in your main application.
### Option 2: Build and Reference DLLs

1.  Clone the repository:
2.  Open the solution in Visual Studio
3.  Choose `Build` -> `Publish Selection`
4.  Choose a proper configuration and publish
5.  Open your own project
6.  Right-click the solution -> `Add` -> `Existing Project` -> Browse and choose the .dll files

# 📝 Documentation Generation with DocFX
Generate a full API reference site using DocFX.

### DocFX instruction
1. Go to bash or cmd and run `dotnet tool install -g docfx`
2. Install Node.js
3. Set up a special directory for the server and run `docfx init` under the directory
4. Change the docfx.json file and add keys under /metadata/src
- **ResourceDownloader**
  - src：`<git pull directory>/PixanKit/ResourceDownloader`
  - files：`**/*.csproj`
- **ModController**
  - src：`<git pull directory>/ModController`
  - files：`**/*.csproj`
- **LaunchCore**
  - src：`<git pull directory>/Pixankit/LaunchCore`
  - files：`**/*.csproj`
  
5. Finally, Run `docfx docfx.json --serve` and visit `localhost:8080` in your web browser

## 💡 Notes
This project is a continuous work-in-progress. While some designs or naming conventions may evolve, your feedback and contributions are always appreciated!

# 🤝 Contributing
Contributions are welcome!

# 📄 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. Feel free to use it!

# 🙌 Acknowledgments
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON framework for .NET
- [Tomlyn](https://github.com/xoofx/Tomlyn) - TOML parser and writer for .NET
- [HtmlAgilityPack](https://html-agility-pack.net/) - HTML parser for .NET
