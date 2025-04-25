## 👤 Setting Up a Player

Before launching a game, you need to configure a player account.

Due to recent policy changes, using Mojang accounts now requires registering an Azure application. Since the developer (me) has not implemented the updated method, the `MicrosoftPlayer` class still uses the older login flow and may not function correctly.

To keep things simple for demonstration purposes, we'll use an **offline account**. Please note that depending on your region, using offline accounts may violate local laws — proceed with caution.

To create an offline player, use:

```csharp
var player = new OfflinePlayer("YourPlayerName");
```

Then add the player to the launcher using:

```csharp
PlayerManager.AddPlayer(player);
```

Once added, this player will be used in subsequent launch operations unless changed explicitly.
