> 内容由 AI 根据核心代码生成，已通过人工审核。

# EditorObjectFieldUtility

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity](https://img.shields.io/badge/Unity-2020.3%2B-black.svg)](https://unity.com/)
[![Test Framework](https://img.shields.io/badge/Tests-NUnit%2FUTF-blue.svg)](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

一个为 Unity 编辑器扩展提供增强型 `ObjectField` 绘制的轻量工具库。它弥补了原生 `EditorGUI.ObjectField` / `EditorGUILayout.ObjectField` 在只读展示、移除选择器按钮以及自定义选择器行为等方面的不足，让 Inspector 和编辑器窗口的 UI 交互更加灵活。

## 简介

在编写 Unity 编辑器工具时，我们经常需要：
- 展示一个对象引用但不允许用户随意拖拽修改；
- 隐藏右侧的小圆点选择器（Object Picker）以获得更简洁的界面；
- 自定义选择器按钮的逻辑（例如做额外的验证、弹出自己的选择窗口等）。

`EditorObjectFieldUtility` 将这些需求封装为一组静态方法，完全模仿 Unity 原生 API 的风格，提供 `Rect` / `Layout` 两个版本，并支持泛型与非泛型重载，使用方式几乎与原生方法一致。

## 安装环境要求

- Unity 2020.3 或更高版本（需要支持 C# 8.0 及以上）
- 仅用于 Editor 脚本（需放置在 `Editor` 文件夹内，或使用 `#if UNITY_EDITOR` 包裹）

## 安装方式

### 1. 源码导入
- 下载本仓库的 `EditorObjectFieldUtility.cs` 文件，放置于项目的 `Editor` 文件夹下即可。

### 2. DLL 导入
- 可将项目编译为 DLL，或直接下载已编译的 DLL 文件，放入 `Assets/Plugins/Editor` 目录。

## 设计理念

原生 Unity 的 `ObjectField` 始终带有对象选择器按钮，且只读模式需要手动禁用交互，代码分散且不直观。本工具库通过统一内部绘制逻辑，将“只读”、“无选择器”和“自定义选择器按钮”三种模式提炼为不同的公共方法，使用者无需关心底层事件处理（拖拽验证、类型过滤、Ping 对象等），只需调用对应的 API 即可。

## 功能说明

### 只读对象字段
- 完全禁用拖拽赋值，用户无法通过拖拽修改值。
- 单击字段仍会通过 `EditorGUIUtility.PingObject` 定位到对应资源或场景对象。
- 适用于仅展示引用关系的只读 UI。

### 无选择器按钮的对象字段
- 保留拖拽赋值能力，但右侧的小圆点选择器按钮被完全隐藏。
- 布局与风格与原生字段高度一致，通过自定义 `GUIStyle` 调整了右侧边距。
- 适用于不需要选择器窗口、希望减少界面视觉干扰的场景。

### 自定义选择器按钮的对象字段
- 右侧的选择器按钮会正常绘制，但点击后不打开 Unity 的原生选择器，而是触发用户提供的回调 `Action<T>`。
- 可在回调中实现任意自定义逻辑（如打开自定义搜索窗口、记录日志等）。
- 保留完整的拖拽校验、类型过滤和 Ping 定位功能。

### 灵活的 API 设计
- 每个功能均提供 `Rect`（手动布局）和 `Layout`（自动布局）两组静态方法。
- 支持泛型版本（自动推断 `Type`）和非泛型版本（手动指定 `Type`）。
- `allowSceneObject` 参数控制是否允许接受场景中的对象，与原生行为保持一致。
- 内置类型不匹配保护：当拖入对象的类型与要求不符时，自动拒绝并返回 `null`，避免操作失误。

## 常见问题

**Q：为什么字段显示的值偶尔变成 `null`？**  
A：当你拖入的对象类型与字段要求不匹配时，工具会自动将值置为 `null`。请检查拖入对象的类型是否正确。

**Q：能否在只读字段上保留 Ping 定位功能？**  
A：可以。本库默认在单击任何类型的字段时都会 Ping 对象，包括只读字段。

**Q：自定义选择器按钮的回调中可以访问当前字段值吗？**  
A：可以。回调中的参数就是点击按钮时字段当前持有的对象引用。

**Q：这些方法只能在 `OnGUI` 中使用吗？**  
A：是的，所有绘制方法必须在 `Editor` 的 Immediate Mode GUI 上下文中调用（如 `EditorWindow.OnGUI` 或 `Editor.OnInspectorGUI`）。

## 其它文档

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目基于 [MIT 许可证](https://opensource.org/licenses/MIT) 发布。