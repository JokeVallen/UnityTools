> 内容由 AI 根据核心代码生成，已通过人工审核。

# ComparerUtility

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Unity](https://img.shields.io/badge/Unity-2020.3%2B-green.svg)
![](https://img.shields.io/badge/Unit%20Tests-passing-passing)

一个轻量级的 C# 比较器全局缓存工具，支持 `IEqualityComparer`/`IEqualityComparer<T>` 和 `IComparer`/`IComparer<T>` 的统一注册、获取与适配，适用于需要在应用中集中管理比较逻辑的场景。

## 简介

`ComparerUtility` 允许你在应用启动时为任意类型注册自定义相等性或排序比较器，之后通过简洁的泛型或非泛型接口获取同一实例。即使你的比较器仅实现了泛型或非泛型接口中的一个，工具也会自动适配，保证两种调用方式均能正常工作。它内部使用 `ConcurrentDictionary` 保证线程安全，并通过反射缓存默认比较器以减少重复开销。提供完整的移除与清空功能，便于动态重置或测试清理。

**主要功能**：
- 全局替换任一类型的相等性/排序比较器
- 同时支持泛型 API（`<T>`）和非泛型 API（`Type`）
- 自动适配仅实现单一接口的比较器
- 默认比较器的反射创建与缓存
- 线程安全的并发访问
- 简洁的缓存清除机制

## 安装环境要求

- .NET Standard 2.0 或更高
- Unity 2020.3 及以上版本
- C# 7.0+

## 安装方式

### 1. 源码导入
将 `ComparerUtility.cs` 文件放入 Unity 项目的 `Assets/Scripts/Utility`（或其他脚本目录）中即可。

### 2. DLL 文件导入
将编译好的 `EasyBinder.dll`（或包含该类的程序集）复制到 `Assets/Plugins` 目录下。

## 设计理念

在许多框架或大型应用中，比较规则（如字符串忽略大小写、实体按某一字段排序）常常需要在全局范围内保持统一。直接使用 `EqualityComparer<T>.Default` 无法被外部替换。`ComparerUtility` 提供一个中央注册点，让调用者只需 `GetComparer<T>` 就能获得当前全局约定的比较器，而具体的比较策略可在启动配置时动态设定，实现了“比较逻辑”与“业务代码”的解耦。

工具内部采用适配器模式，解决了 C# 中 `IEqualityComparer<T>` 与 `IEqualityComparer` 无继承关系的痛点，保证泛型和非泛型操作均能获得相同的比较行为。

## 具体功能说明

- **相等性比较器管理**  
  通过 `SetEqualityComparer<T>(...)` 或 `SetEqualityComparer(Type, IEqualityComparer)` 注册自定义相等比较器；之后在任何地方调用 `GetEqualityComparer<T>()` 或 `GetEqualityComparer(type)` 即可获得该实例。若从未设置，返回系统默认比较器。

- **排序比较器管理**  
  同样提供 `SetComparer` 和 `GetComparer` 系列方法，用于全局替换类型的排序规则。

- **接口适配**  
  如果你有一个只实现了 `IEqualityComparer`（非泛型）的比较器，依然可以通过泛型 `GetEqualityComparer<T>` 获得一个等效的比较器对象（内部自动适配）。反之亦然。

- **默认比较器智能缓存**  
  当通过非泛型 API 获取从未设置过的类型的比较器时，工具会通过反射访问 `EqualityComparer<T>.Default` 或 `Comparer<T>.Default` 并将结果缓存，避免二次反射开销。

- **缓存移除与清空**  
  使用 `TryRemoveEqualityComparer<T>()` / `TryRemoveComparer<T>()` 可移除单个类型的所有缓存（自定义和反射默认缓存）。`ClearEqualityComparers()` / `ClearComparers()` 则一次性清空所有缓存。

- **线程安全**  
  所有缓存字典均为 `ConcurrentDictionary`，支持多线程并发读写。

## 常见问题

**Q：我设置了一个比较器，为什么非泛型 `GetComparer(Type)` 返回的对象不是原始实例？**  
A：当你设置的比较器只实现了 `IComparer<T>`（未实现 `IComparer`）时，工具内部会创建一个适配器包装它以同时满足非泛型接口要求。此时非泛型 `GetComparer` 返回的是这个适配器实例，但它的比较行为与原始实例完全一致。这是为了保证接口完整性的必要设计。

**Q：是否支持可释放的比较器？**  
A：当前版本的 `ComparerUtility` 不接管用户比较器的生命周期。如果你需要释放实现了 `IDisposable` 的比较器，请自行管理，工具在移除缓存时不会自动调用 `Dispose`。

## 文档导航

- [API 文档](./DOCUMENT.md)
- [测试报告](./TEST_REPORT.md)

## 许可证

本项目基于 MIT 许可证开源。