## 📂 What is `Folder`?

`Folder` is a class used to represent a game directory — essentially an abstraction of a `.minecraft` folder.

Typically, a valid game folder contains subdirectories like:

- `.minecraft/libraries` — where core libraries are stored
- `.minecraft/versions` — where individual game versions and launch configurations are kept

You can either generate such a folder yourself or import one that was created by the official Minecraft launcher or a third-party launcher.

To load a folder, use the following code:

```csharp
var folder = new Folder("your/path/to/.minecraft");
GameManager.AddFolder(folder);
```

Once instantiated, the `Folder` class will automatically scan the `.minecraft/versions` directory, initialize all detected game versions, and register them into the `Launcher`.

This makes it easy to manage multiple game setups or reuse existing environments.
