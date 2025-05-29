# 🚀 First Step with PixanKit

We assume you've already integrated **PixanKit** into your project. Before diving into game launching or mod handling, you'll want to generate and load the default configuration files.

Start by running:

```csharp
Files.Generate();
Files.Load();
Launcher.Instance.Init();
```

These methods will set up the default directory structure and load all necessary configuration files for you.

After that, you're ready to use the launcher directly:

```csharp
Launcher.Instance
```

`Launcher.Instance` is ready to go out of the box — no need for explicit initialization. It's designed to be immediately usable after loading configurations.
