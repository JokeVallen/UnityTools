> 内容由 AI 根据核心代码生成，已通过人工审核。

# HashCodeUtility

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![Unity Version](https://img.shields.io/badge/Unity-2020.3.48f1-blue)
![Test Framework](https://img.shields.io/badge/Test%20Framework-1.1.33-passing)

高性能、顺序敏感的哈希码工具库，适用于 Unity 项目（兼容 .NET Standard 2.0，C# 7.0）。

## 简介

`HashCodeUtility` 提供了一组静态方法，用于快速计算单个对象或合并多个对象的哈希码。  
算法采用经典的种子-乘数混合（seed=17，multiplier=31），所有运算在 `unchecked` 环境中进行，避免溢出异常。  
特点：
- 提供 **1~5 个参数** 的非分配重载，避免 `params` 数组的堆内存分配。
- 支持 **任意数量** 的参数合并（含泛型与非泛型版本）。
- 支持 **顺序依赖哈希**，元素顺序改变必然导致哈希值变化。
- 可注入 **自定义相等比较器**，灵活控制哈希行为。

## 安装环境要求

- Unity 2020.3 或更高版本（或任何支持 .NET Standard 2.0 的运行时）
- C# 7.0 或更高

## 安装方式

### 方式一：源码导入

1. 下载本仓库中的 `HashCodeUtility.cs` 文件。
2. 将其放置于项目的 `Assets` 目录下的任意脚本文件夹中。
3. 无需额外配置，即可在代码中直接调用。

### 方式二：DLL 文件导入

1. 将 `HashCodeUtility.dll` 放入 `Assets/Plugins` 文件夹。
2. 若使用 Assembly Definition Files，请确保测试程序集能够访问该 DLL（必要时添加 `InternalsVisibleTo` 或直接引用）。
3. 重新生成项目后即可使用。

## 设计理念

- **零分配快速合并**：对于 2~5 个参数的场景，使用显式展开的方法签免去 `params` 临时数组，提升热路径性能。
- **顺序敏感**：所有合并方法均考虑元素插入顺序，适用于需要区分排列的场景（如坐标组合、状态哈希等）。
- **统一 null 处理**：`null` 引用在哈希计算中贡献值为 0，不引发异常；`null` 集合直接返回 0。
- **默认比较器优先**：泛型方法全程使用 `EqualityComparer<T>.Default`，确保行为与 .NET 标准集合一致。
- **可扩展自定义比较**：为顺序依赖哈希提供 `IEqualityComparer<T>` 注入点，满足不同相等语义需求。

## 具体功能说明

### 1. 单对象哈希
直接获取任意对象的哈希码，null 值为 0。内部已优化为单次乘加。

### 2. 多对象合并
提供 2~5 个参数的强类型 `Combine` 重载，无额外分配；对于更多参数可使用 `CombineAll`。混合不同类型的对象时顺序至关重要。

### 3. 顺序依赖集合哈希
针对数组或 `IEnumerable<T>` 计算哈希，元素顺序直接影响结果。适合用于校验集合内容不变性，或快速比较有序列表。

### 4. 自定义比较器
在计算集合哈希时可以传入 `StringComparer.OrdinalIgnoreCase` 等实现，轻松实现不区分大小写的哈希，或自定义相等逻辑下的哈希生成。

### 5. 非泛型备用
非泛型 `CombineAll` 接受 `object[]`，但值类型会引发装箱，且可能返回与泛型版本不同的哈希值。仅推荐在类型混杂且无法使用泛型的低频率场景下使用。

## 其它文档

- [API 文档](./DOCUMENT.md) - 完整的公共 API 签名与参数说明。
- [测试报告](./TEST_REPORT.md) - 单元测试覆盖情况与结果。

## 许可证

本项目基于 [MIT 许可证](LICENSE) 发布。您可以自由使用、修改和分发。