# PixanKit

PixanKit is a `.NET 8`-based library designed to simplify the development of custom Minecraft Launchers. The library is structured into three separate C# projects:

-   **LaunchCore**: The core library that handles player, game, and JRE (Java Runtime Environment) management.
-   **ResourceDownloader**: A set of specialized task classes for managing interactions with resource servers. This project depends on `LaunchCore`.
-   **ModModule**: A dedicated library for mod management, also relying on `LaunchCore`.

Whether you're creating a custom launcher or automating tasks related to Minecraft, PixanKit offers a modular solution tailored to your needs.

# Features
### LaunchCore

-   Player management:
    -   Offline and Microsoft account support.
    -   Skin caching and embedding player information.
-   Game management:
    -   Handle vanilla, OptiFine, and modded versions.
    -   Manage game directories and startup parameters.
-   Runtime management:
    -   Automatically detect and manage suitable JRE versions.

### ResourceDownloader

-   Task-based resource server interactions.
-   Flexible and customizable downloader implementations.

### ModModule

-   Simplified mod installation and management.
-   Dependency handling for mod loaders like Forge or Fabric.

# How to use?
### Option 1: Add as Git Submodule

1.  Add PixanKit to your project:
`git submodule add https://github.com/your-repo/PixanKit.git`
2.  Open your solution in Visual Studio.
3.  Right-click the solution -> `Add -> Existing Project` -> Select `PixanKit/LaunchCore.csproj` and other necessary projects.
4.  Add a reference to the PixanKit projects in your main application.
### Option 2: Build and Reference DLLs

1.  Clone the repository:
2.

# Documentation?
### DocFX instruction
1. Go to bash or cmd and run `dotnet tool install -g docfx`
2. Install Node.js
3. Set up a special directory for the server and run `docfx init` under the directory
4. Change the docfx.json file and add keys under /metadata/src
- **ResourceDownloader**
  - src：`<git pull directory>/PixanKit/ResourceDownloader`
  - files：`**/*.csproj`
- **ModModule**
  - src：`<git pull directory>/ModModule`
  - files：`**/*.csproj`
- **LaunchCore**
  - src：`<git pull directory>/Pixankit/LaunchCore`
  - files：`**/*.csproj`


## Something else?

Well, due to the author's limited programming ability, some designs may be cumbersome, especially in the context-dependent code. Properties naming may not be uniform. I hope you can be more tolerant and help improve it.
