# ☕ Setting Up Java Runtime

Before launching the game, you must specify a Java runtime environment (JRE) compatible with Minecraft.

PixanKit provides automatic detection for common JREs, but you can also manually set the Java path using the following method:

```csharp
Launcher.Instance.AddJavaRuntime("path/to/jdk");
```

Make sure the selected JRE is compatible with the game version you're launching (typically Java 8 or Java 17 depending on Minecraft version).