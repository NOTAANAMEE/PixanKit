# PixanKit 简体中文文档

PixanKit 是一个基于 .NET 8 的模块化工具包，旨在帮助你快速构建自定义的 Minecraft 启动器以及相关自动化工具。该库主要包含三个项目：

### 🧱 LaunchCore
- 处理玩家账户、游戏版本以及 Java 运行时选择的核心库。

### 🌐 ResourceDownloader
- 模块化下载任务系统，支持进度跟踪，并可灵活扩展实现。

### 🧩 ModController
- 提供即插即用的模组安装与管理功能，兼容 Forge/Fabric 等加载器。

无论你是编写启动器还是需要自动化管理 Minecraft 资源，PixanKit 都可以作为可靠的基础。 

## ✨ 功能概览
### 🧱 LaunchCore
- 支持离线与微软账户登录，可缓存皮肤信息。
- 管理原版、OptiFine 和模组版本，以及游戏目录和启动参数。
- 自动检测并管理适用的 JRE/JDK。

### 🌐 ResourceDownloader
- 基于任务的资源下载与服务器交互。
- 下载器实现可按需自定义。

### 🧩 ModController
- 简化模组安装和依赖处理。

## 🚀 快速开始
### 方式一：作为 Git 子模块引入
1. 在项目中添加子模块：
   ```bash
   git submodule add https://github.com/your-repo/PixanKit.git
   ```
2. 在 Visual Studio 中打开解决方案，右键解决方案选择 `Add -> Existing Project`，引入 `PixanKit/LaunchCore.csproj` 等项目。
3. 在主项目中添加对 PixanKit 项目的引用。

### 方式二：构建并引用 DLL
1. 克隆仓库后在 Visual Studio 中打开解决方案。
2. 在 `Build -> Publish Selection` 选择合适的配置发布。
3. 在你的项目中引用生成的 DLL 文件。

有关详细的初始化步骤，请参阅 [Docs/Get-Started.zh-CN.md](Docs/Get-Started.zh-CN.md)。

## 📝 使用 DocFX 生成文档
1. 安装 DocFX：`dotnet tool install -g docfx`。
2. 安装 Node.js 并在指定目录运行 `docfx init`。
3. 修改 `docfx.json` 中 `/metadata/src` 部分，配置各项目路径及 `*.csproj` 文件。
4. 运行 `docfx docfx.json --serve` 后在浏览器访问 `localhost:8080` 查看 API 文档。

## 💡 说明
项目仍在持续开发中，命名或设计可能会发生变化，欢迎提出反馈或贡献。

## 🤝 参与贡献
欢迎通过 Pull Request 或 Issue 参与项目建设。

## 📄 许可证
该项目基于 MIT 许可证发布，详情见 [LICENSE](LICENSE)。
