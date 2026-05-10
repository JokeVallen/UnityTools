> 内容由 AI 根据核心代码生成，已通过人工审核。

# 🧩 Unity MonoSingleton

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个轻量级、开箱即用的 Unity MonoBehaviour 单例基类库，支持**非持久化单例**与**跨场景持久化单例**，并提供接口访问变体，帮助你快速搭建结构清晰、生命周期可控的全局管理器。

---

## 📖 简介

在 Unity 项目中，经常需要以单例形式创建音频管理器、场景加载器、配置管理器等全局对象。本库通过抽象泛型基类封装了最通用的单例创建、重复实例销毁、场景卸载清理以及可选的对象持久化（`DontDestroyOnLoad`）。

**核心特性：**
- 非持久化单例：随场景创建，场景卸载时自动销毁。
- 持久化单例：标记为跨场景不销毁，适用于真正全局唯一的对象。
- 接口访问变体：可将实例以接口形式暴露，方便依赖倒置与单元测试。
- 安全保障：自动检测并销毁多余实例，确保唯一性。

---

## ⚙️ 安装环境要求

- **Unity** 2020.3 及以上（仅依赖 `UnityEngine` 基础 API）
- **.NET Standard 2.0** 或更高

---

## 📦 安装方式

### 1. 源码导入
将仓库中的 `MonoSingleton.cs` 和 `MonoSingletonPersistant.cs` 复制到 Unity 项目的 `Assets/Scripts/`（或任意脚本目录）即可。

### 2. DLL 导入
将项目编译为 `.dll` 后放入 `Assets/Plugins/`。推荐在需要版本控制或隔离环境时使用。

---

## 🧠 设计理念

- **抽象基类**：所有单例必须继承自明确的抽象类，统一规范 `Awake` 与 `OnDestroy` 行为。
- **唯一性保障**：在 `Awake` 内自动检测重复实例并立即销毁多余对象。
- **最小侵入**：仅需在子类重写时调用 `base.Awake()` 和 `base.OnDestroy()` 即可接入。
- **接口分离**：通过 `MonoSingleton<T, I>` 将内部实现与对外接口解耦，便于扩展和模拟。

---

## 🧩 具体功能说明

### `MonoSingleton<T>` 非持久化单例
- 适用于仅在单个场景内唯一的对象（如 UI 管理器、关卡逻辑控制器）。
- 场景卸载时自动置空静态实例，不会污染后续场景。
- 场景中若意外存在多个同类型组件，仅保留第一个，其余立即销毁。

### `MonoSingleton<T, I>` 非持久化 + 接口
- 与普通版本行为一致，但静态属性 `Instance` 返回类型为接口 `I`。
- 调用者只能访问接口暴露的成员，具体实现被封装。
- 适合需要切换实现或进行单元测试模拟的场景。

### `MonoSingletonPersistant<T>` 持久化单例
- 继承自 `MonoSingleton<T>`，额外在 `Awake` 中调用 `DontDestroyOnLoad`。
- 适用于全局唯一的对象（音频管理器、存档管理器等），跨场景切换时保持存活。
- 需要手动销毁时直接 `Destroy(gameObject)` 即可。

### `MonoSingletonPersistant<T, I>` 持久化 + 接口
- 结合持久化与接口访问，对外提供稳定的接口实例，内部可随时替换而无需修改调用方。

---

## ❓ 常见问题

**Q：为什么切换场景后单例变成了 null？**  
A：请检查是否继承了持久化版本 `MonoSingletonPersistant<T>`，普通版本会跟随场景销毁。

**Q：忘记在子类 `Awake` 中调用 `base.Awake()` 会怎样？**  
A：将失去唯一性检测与实例注册，导致 `Instance` 为 null 或出现多个实例。

**Q：如何手动销毁持久化单例？**  
A：直接调用 `Destroy(gameObject)`，静态引用会在 `OnDestroy` 中自动置空。

**Q：编辑器退出 Play Mode 后静态实例会残留吗？**  
A：不会，Unity 会重置所有静态变量。构建后的程序在进程结束时自然清理。

---

## 📚 其它文档

- [API 文档](./DOCUMENT.md) – 公共 API 详情与使用示例  
- [测试报告](./TEST_REPORT.md) – 测试报告

---

## 📜 许可证

本项目基于 [MIT License](https://opensource.org/licenses/MIT) 开源，可自由使用、修改、分发。