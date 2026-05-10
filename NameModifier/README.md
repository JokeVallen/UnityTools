> 项目由 AI 和作者共同设计和开发，已进行基本的单元测试和功能测试，具体测试请查看 `Tests` 下相关文件。

# Unity 编辑器名称修改器 (Name Modifier)

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE) [![Unity](https://img.shields.io/badge/Unity-2020.3+-blue)](https://unity.com/) [![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) ![](https://img.shields.io/badge/Unit%20Tests-passing-passing) 

一个灵活、可扩展的 Unity 编辑器批量命名工具，支持撤销/恢复、分组管理、自定义命名策略，适用于场景对象和资产对象的批量重命名。  

---

## ✨ 特性

- **批量重命名**：支持对任意选中的场景游戏对象或项目资产对象一次性修改名称。
- **撤销/恢复系统**：两种存储模式可选：
  - `SessionState`：跨编辑器会话保留历史记录（通过 Unity SessionState 存储）。
  - `Memory`：高性能内存存储，关闭编辑器后历史清空。
  - `None`：完全禁用撤销功能。
- **分组管理**：将多次操作归为一组，可一次性撤销或恢复整组操作。分组名支持动态占位符（日期、时间、日期时间），并支持容量限制（达到上限自动结束分组）。
- **可扩展的处理器**：通过继承 `NameModifierHandler` 并创建 ScriptableObject 资产，即可自由实现任何命名规则。工具自动扫描指定目录下的所有处理器资产并加载。
- **智能缓存清理**：支持自动清理无效缓存（对象已删除时）及手动清理全部历史记录。
- **进度显示**：批量处理时显示进度条和耗时，避免编辑器假死。
- **日志系统**：内置可替换的日志接口，默认输出至 Unity Console，支持开关控制。
- **配置持久化**：所有设置（处理器目录、撤销参数、分组模板等）以 ScriptableObject 资产形式保存，可导出/导入 JSON 配置。

---

## 📦 安装

### 源码方式

将 `EditorTools/NameModifier` 文件夹（含所有 `.cs` 文件）复制到你的 Unity 项目的任意 `Editor` 目录中（例如 `Assets/Editor/NameModifier`）。确保所有文件位于 `UNITY_EDITOR` 条件下。

### DLL 方式

若提供 DLL 文件，将其放入 `Assets/Plugins` 目录，并确保 `EditorTools.NameModifier` 命名空间可访问。

---

## 🚀 快速开始

### 1. 打开工具窗口

通过菜单栏 `EditorTools > NameModifier` 打开窗口。

### 2. 创建配置资产

如果项目中尚未存在配置资产，工具会临时运行于内存模式，配置不会被保存。  
可通过菜单 `Assets/Create/EditorTools/NameModifierConfig` 创建配置资产，创建后工具窗口将自动识别并持久化路径。  
打开配置资产，在 Inspector 中设置：
- **处理器目录**：放置自定义 `NameModifierHandler` 资产的文件夹路径（相对于 `Assets`）。
- **撤销系统类型** 及容量等参数。
- **分组模板**（如 `分组_{DateTime}`）和默认容量。

### 3. 使用处理器

工具会自动扫描处理器目录下的所有 `NameModifierHandler` 资产，并在下拉列表中显示其 `OptionName`。选择一个处理器，即可在下方绘制其自定义 GUI（如有）。  
选择要重命名的对象（场景对象或资产），点击 **修改** 按钮执行批量命名。

### 4. 分组管理

点击窗口中的“分组管理”折叠面板：
- 设置组名模板（支持 `{Date}`、`{Time}`、`{DateTime}`）和组容量。
- 点击 **激活分组**，此后所有修改操作均属于该分组。
- 点击 **结束分组** 关闭当前分组。
- 在分组激活期间，**撤销** 和 **恢复** 按钮将操作整个分组。

### 5. 撤销与恢复

- 当分组处于活动状态时，点击 **撤销** 可回退到上一个分组步骤，点击 **恢复** 可前进到下一个分组步骤。
- 如果未激活分组，每次修改会临时创建一个以当前模板命名的分组，操作完成后自动结束（相当于单步分组）。

### 6. 扩展自定义处理器

1. 创建一个继承自 `NameModifierHandler` 的类，并标记 `[CreateAssetMenu]` 以便在资源菜单中创建资产。
2. 实现 `OptionName` 属性（显示在下拉列表中的名称）和 `Modify` 方法（具体的命名逻辑）。
3. 可选重写 `DrawGUI`、`Reset`、`OnSelected`、`Tip` 等成员。
4. 在 `Modify` 中调用 `ApplyRename(obj, newName)` 完成重命名并记录历史。
5. 将生成的资产文件放入配置的“处理器目录”中，工具窗口将自动加载。

示例（简单序号后缀）：

```csharp
[CreateAssetMenu(menuName = "EditorTools/Handlers/NumberSuffix")]
public class NumberSuffixHandler : NameModifierHandler
{
    public override string OptionName => "序号后缀";

    public override void Modify(Object obj, int index, int count)
    {
        ApplyRename(obj, $"{obj.name}_{index:D3}");
    }
}
```

---

## 📄 许可证

本项目采用 MIT 许可证。详情请参见 [LICENSE](LICENSE) 文件。

---

## 🤝 贡献

欢迎提交 Issue 或 Pull Request！如果你有好的建议或发现了 bug，请随时反馈。