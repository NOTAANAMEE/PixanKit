# 🚀 开始使用 PixanKit

感谢你选择 **PixanKit**！本指南将帮助你在自己的项目中快速集成并初始化此工具包。

## 前置条件
- 已安装 .NET 8 SDK
- 建议使用 Visual Studio 2022 或更高版本

## 获取源码
### 方法一：Git 子模块
1. 在你的仓库中执行：
   ```bash
   git submodule add https://github.com/your-repo/PixanKit.git
   ```
2. 在 Visual Studio 打开解决方案，右键解决方案选择 `Add -> Existing Project` 引入 `PixanKit` 下的对应 `.csproj` 项目。

### 方法二：编译 DLL
1. 克隆仓库并在 IDE 中打开。
2. 通过 `Build -> Publish Selection` 生成库文件。
3. 在你的项目中引用生成的 DLL。

## 初始化配置
首次运行前请生成并加载默认配置：
```csharp
Files.Generate();
Files.Load();
Launcher.Instance.Init();
```

完成以上步骤后，PixanKit 就已准备就绪，可以开始管理游戏资源或直接启动游戏。

## 进一步阅读
更多示例和说明请查看 `Docs` 目录下的其他文档。
