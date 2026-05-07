# 常用类库

![License](https://img.shields.io/badge/license-MIT-blue.svg)

该部分主要包括项目开发中满足常见需求的工具类，该部分会持续更新，并且每个工具类也可能迭代更新，每个工具类都会进行单元测试（或可能进行基准测试等其它测试），以确保基本的功能稳定，但这依旧不代表其在生产环境中绝对稳定，如果遇到问题，你可以提交 ISusse，作为维护者进行修复的依据，你也可以自行阅读源码进行修复。

## 工具类目录

- [ComparerUtility](/ComparerUtility/)  
    一个轻量级的 C# 比较器全局缓存工具，支持 `IEqualityComparer`/`IEqualityComparer<T>` 和 `IComparer`/`IComparer<T>` 的统一注册、获取与适配，适用于需要在应用中集中管理比较逻辑的场景。
- [EditorObjectFieldUtility](/EditorObjectFieldUtility/)  
    一个为 Unity 编辑器扩展提供增强型 `ObjectField` 绘制的轻量工具库。它弥补了原生 `EditorGUI.ObjectField` / `EditorGUILayout.ObjectField` 在只读展示、移除选择器按钮以及自定义选择器行为等方面的不足，让 Inspector 和编辑器窗口的 UI 交互更加灵活。
- [HashCodeUtility](/HashCodeUtility/)  
    高性能、顺序敏感的哈希码工具类。
- [MonoSingleton](/MonoSingleton/)  
    一个轻量级、开箱即用的 Unity MonoBehaviour 单例基类库，支持**非持久化单例**与**跨场景持久化单例**，并提供接口访问变体，帮助你快速搭建结构清晰、生命周期可控的全局管理器。
- [ObjectFactory](/ObjectFactory/)  
    一个轻量级、可扩展的 Unity 对象工厂框架，为 `GameObject` 和 `Component` 的创建提供统一入口，并支持在运行时无缝替换底层实现（如对象池、测试替身等）。