# LimbusCompanyLocalizeCustomer

简要说明
---
一个基于 .NET 8 的 Windows 桌面工具，提供GUI界面用于编辑并导出《边狱巴士公司》（Limbus Company）中文本地化文件。支持创建/打开项目（.lclcp）、修改人格名称与技能名称、将修改应用到本地汉化文件或导出为 JSON / ZIP 包。
让你不用每次汉化更新都手动修改
本项目使用AI辅助开发

主要功能
---
- 浏览并加载游戏本地化源（LLC_zh-CN）
- 编辑人格名称与技能名称
- 将修改应用到本地汉化文件
- 导出修改后的 JSON 文件到指定文件夹（可以直接覆盖游戏内汉化）
- 生成可直接替换游戏汉化的 ZIP（在边狱巴士公司根目录解压即可覆盖汉化）
- 项目文件（.lclcp）保存/加载，自定义覆盖保存在项目内

先决条件
---
- Windows 系统
- 安装 .NET 8 运行时（https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-desktop-8.0.29-windows-x64-installer）

本地构建
---
1. 克隆仓库：
   ```bash
   git clone https://github.com/Taullarkura/LimbusCompanyLocalizeCustomer.git
   ```
2. 使用 Visual Studio 打开 `LimbusCompanyLocalizeCustomer.slnx` 或在命令行下：
   ```bash
   dotnet build LimbusCompanyLocalizeCustomer.slnx
   ```
3. 在 IDE 中运行或直接运行输出目录下的可执行文件。

使用说明（用户）
---
** 0.首先确保应用目录下有零协会汉化文件，本应用读取零协汉化文本，缺失将无法正常运行!**
     - 若没有，应用会尝试从Github上下载零协会发布的Release文件，你也可以从边狱巴士安装路径\Limbus Company\LimbusCompany_Data\Lang\下手动复制LLC_zh-CN文件夹到应用目录下
1. 启动程序并配置游戏安装路径（程序可尝试自动定位）。
2. 新建或打开项目（.lclcp）：
   - 新建：文件 -> 创建新项目
   - 打开：文件 -> 打开现有项目
3. 编辑：
   - 左侧树选择人格，编辑“人格名”输入框。
   - 在技能表中双击修改技能名。
4. 保存项目：点击“保存项目”或使用 Ctrl+S。
5. 应用到本地文件：点击“应用项目修改到文件”会将修改写入 汉化路径 下的对应 JSON 文件。
6. 导出：
   - 导出为 JSON：选择目标文件夹，程序会将对应 JSON 文件复制并应用修改后保存。
   - 导出为 ZIP：选择保存路径（默认以项目名命名），生成包含 LimbusCompany_Data 根目录的压缩包，结构为 `LimbusCompany_Data\Lang\LLC_zh-CN\*.json`。

注意事项
---
- 应用到本地文件前请备份游戏目录；程序不会自动备份目标文件。
- 项目文件仅保存用户自定义修改的名称

项目结构（概要）
---
- `MainForm.cs` / `Form1.Designer.cs`：主界面与交互逻辑
- `JsonClass.cs`：Json数据模型（LCLCFileJson、IdtInfo、SkillInfo）
- `Util.cs`：工具（日志等）

许可证
---
本项目采用 AGPL-3.0（GNU Affero General Public License）协议。
更多信息和许可证文本请参见：https://www.gnu.org/licenses/agpl-3.0.html

第三方许可与致谢
---
本工具所使用的汉化基础文件 **`Localize Limbus Company`**  
由 **`Localize Limbus Company(都市零协会)`** 创作，并以 [知识共享署名-非商业性使用-相同方式共享 4.0 国际许可协议 (CC BY-NC-SA 4.0)](https://creativecommons.org/licenses/by-nc-sa/4.0/) 提供。

| 项目 | 信息 |
|------|------|
| **原作品** | `Localize Limbus Company` |
| **原作者** | `Localize Limbus Company(都市零协会)` |
| **许可协议** | [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/) |
| **原许可文件** | 随下载文件一同提供，见 `LICENSE` 文件 |

> 本工具输出的任何改编/衍生汉化文件，均是基于上述原作品的改编作品，  
> **同样强制采用 CC BY-NC-SA 4.0 协议发布**。  
> 修改内容主要包括：人格名、技能名等。

使用须知
---
- 禁止将本工具产出的改编文件用于**商业目的**。
- 如需分享改编文件，必须**保留原作者署名**、**声明修改内容**，并且**使用相同的 CC BY-NC-SA 4.0 许可证**。
- 原始 `LICENSE` 文件和完整的许可信息会随下载文件一同保留，切勿删除。

免责声明
---
本工具按“原样”提供，原作者及开发者不对汉化文本的准确性、适用性做任何担保。  
使用本项目即视为你已知悉并同意上述许可条款。

贡献
---
欢迎 Issues 与 PR。提交时请包含变更说明与复现步骤。
