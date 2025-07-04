感谢使用PixanKit启动库。这个库致力于提供跨平台，简单，高效，可用的启动器类库，方便开发者将更多精力放在UI的开发上

这篇文章假设您已经正确下载并导入PixanKit中的LaunchCore组件。它是核心项目，抽象了游戏，玩家和Java运行时等。它并不包含启动依赖与资源文件验证或是模组管理的功能。

整个文章包含四个部分，初始化，添加，启动运行和关闭。

第一步，初始化。这一部分的代码可以嵌入到例如Main或类似的启动方法中。
需要引用的命名空间:
```csharp
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.Extension;
```
首先，我们必须初始化配置文件。幸运的是，您不需要实际操作文件来进行配置文件操作。您只需要调用Files.Generate()方法即可生成最原始的配置文件。这个文件仅仅包含JSON的必须结构，不包含任何一个游戏/玩家/Java运行时。
您也可以通过自行生成JObject来自定义配置。只需要符合PixanKit/LaunchCore/JsoExamples中对应的结构即可。配置存放在Files.FolderJData, Files.RuntimeJData, Files.PlayerJData, Files.SettingsJData, 对应文件内容就是文件夹,Java虚拟机,玩家和启动器设置
若您已经生成了./Launcher/Config中的配置文件，调用`Files.Load()`即可通过默认JSON明文的方式加载配置。

加载配置文件后，直接调用`Launcher.Instance.Init()`对启动器进行初始化操作。`Launcher`是单例类，使用`Lazy<>`实现懒加载，您可以在启动器的任何地方通过`Lancher.Instance`访问启动器实例。`Launcher`实例化后不会做任何操作，因此需要`Init()`进行初始化。

祝贺，您已经完成了初始化操作，让我们回顾一下代码
```csharp
Files.Generate()//或者改为Files.Load()或自定义方法初始化配置文件
Launcher.Instance.Init(); // Initialize the launcher
```

第二步，添加。这一部分代码可以嵌入到按钮点击事件注册的方法中
1.添加玩家
需要引用的命名空间:
```csharp
using PixanKit.LaunchCore.Core;
using PixanKit.LaunchCore.PlayerModule.Player;
```
每个玩家都有自己的皮肤和UUID。由于Mojang在2024年要求第三方启动器需注册Azure软件并需要Mojang审批才能使用Microsoft账户，因此这里演示添加免费的离线账户。这种情况在大部分国家或地区都是非法的，因此不建议任何开发者实际给玩家提供离线注册通道。若依然提供，在玩家使用离线账户启动游戏时添加参数`--demo`对玩家游玩时间和可享受的服务进行限制。
调用`Launcher.Instance.PlayerManager.AddPlayer(new OfflinePlayer("<你想要添加的名字>"))`添加离线玩家。

2.添加文件夹与游戏
需要准备一个存在游戏的.minecraft文件夹。添加文件夹后会自动添加其中的游戏。其中添加文件夹代码
```csharp
Launcher.Instance.GameManager.AddFolder(new Folder("<folder path>"));//添加文件夹，需要以.minecraft结尾
```

3.添加Java虚拟机
```csharp
Launcher.Instance.JavaManager.AddJavaRuntime("<java path>");//添加jvm所在目录，需要/bin前一级目录
```

第四步，启动。这一部分代码可以嵌入到按钮点击事件注册的方法中
需要引用的命名空间:
```csharp
using PixanKit.LaunchCore.Core;
```
直接启动默认游戏。若未指定默认游戏，则为第一个初始化的游戏
```csharp
var tmp = launcher.Launch();
tmp.Start();
tmp.WaitForExit();
```
tmp是一个启动任务，也可以调用`.SaveToFile()`保存到.bat文件中。命令已经被替换为对应平台可运行的命令，只需要修改后缀即可。

第五步，关闭。这一部分代码可以嵌入到关闭方法中
需要引用的命名空间:
```csharp
using PixanKit.LaunchCore.Core;
```
关闭游戏，并保存配置。配置文件在运行时不起任何作用，修改配置文件不会改变任何东西。配置文件仅在初始化时起作用
```csharp
launcher.Close();
```
使用默认方法保存配置文件。目录不存在时，它会自动创建目录。
```csharp
Files.Save();
```
你也可以通过自己的方法保存配置文件，将其转换成别的例如toml或yaml甚至ini文本格式。

现在启动器已经被正确关闭，祝贺你，了解了如何初步使用`PixanKit.LaunchCore`！