## 🕹️ Final Step: Launching the Game

Now that you've configured the game folder and player, it's time to launch Minecraft!

`GameManager.TargetPlayer` and `GameManager.TargetGame` are automatically populated based on your previous setup. You can launch the game in two ways:

### ✅ Option 1: Launch with default target game
```csharp
Launcher.Instance.Launch();
```

### 🎯 Option 2: Launch a specific game version
```csharp
Launcher.Instance.Launch(specificGame);
```

By default, basic launch arguments (such as memory limits, Java path, and player identity) are added automatically. If you wish to customize them, you can modify them through:

```csharp
Launcher.Settings["argument"] = "--your-custom-args";
```

The `Launch()` method returns a `LaunchSession` object, which you can invoke like this:

```csharp
var session = Launcher.Launch();
session.Start();
```

Once `Start()` is called, the game process will begin with all necessary parameters configured.
