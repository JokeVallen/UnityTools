## 工具列表

- [CodeGenerator](/CodeGenerator/)  
    一个基于 **中介者模式** 与 **特性标记** 的轻量级代码生成框架。通过清晰的职责划分（模板提供、生成、写入），你可以快速搭建可扩展、可维护的代码生成管线。支持同步与异步操作，适用于工具链开发、定制化代码生成等场景。

- [Common](/Common/)（常用工具类）

- [EasyLogger.Unity](/EasyLogger.Unity/)  
    一个即拿即用、轻量低耗的 Unity 日志工具库。它统一了控制台输出与文件持久化，提供灵活的级别过滤与格式化扩展，同时几乎不产生运行时开销。

- [EditorCoroutines.Lit](/EditorCoroutines.Lit/)  
    一个轻量级、零依赖的 Unity 编辑器协程库，让你在编辑器中也能像运行时一样使用协程。支持嵌套协程、取消令牌、等待扩展、泛型返回值等功能，非常适合编辑器工具开发、资源导入流程、批处理任务等场景。

- [EventHub.Unity](/EventHub.Unity/)  
    一个面向 Unity 的通用事件系统，基于 UniTask 构建，旨在为游戏项目提供高易用性、高性能、可扩展的事件解决方案。事件系统将事件的订阅、发布与具体业务逻辑解耦，帮助开发者构建松耦合、易维护的代码架构。

- [FNV1A](/FNV1A/)  
    一个专为 Unity 和 .NET 设计的高性能、零分配、跨平台确定性哈希工具库。它基于 **FNV-1a 64 位算法**，支持多种基础类型、Unity 类型及集合类型的哈希组合，并提供灵活的扩展机制。通过泛型静态缓存与内联优化，该库在保持易用性的同时，将抽象开销降至最低。

- [NameModifier](/NameModifier/)  
    一个灵活、可扩展的 Unity 编辑器批量命名工具，支持撤销/恢复、分组管理、自定义命名策略，适用于场景对象和资产对象的批量重命名。

- [UGUILayoutExtension](/UGUILayoutExtension/)  
    基于 UGUI 布局系统扩展的布局组件库，通过动画曲线和参数配置实现复杂的非线性 UI 布局，无需编写代码。完整融入 UGUI 布局系统，与 `LayoutElement`、`ContentSizeFitter`、`LayoutGroup` 等官方组件兼容搭配。

- [Unity Project Templates](/Unity%20Project%20Templates/)（Unity项目模板）

- [EasyMapper](/EasyMapper/)  
    一个高性能、可组合的 **运行时 ID 映射框架**，专为 Unity 和 .NET Standard 2.0 设计。

- [Orchestrator](/Orchestrator/)  
    一个轻量级、高性能的异步工作流编排引擎，支持 Task、ValueTask、UniTask 三种异步基元。

- [FSM](/FSM/)  
    一个基于 .NET Standard 2.0 的通用有限状态机框架，采用接口与实现分离的架构设计，提供 Builder 模式的流式配置体验。

- [EasyAttributes](/EasyAttributes/)  
    轻量级、高性能的 .NET Attribute 驱动 AOP（面向切面编程）内核，提供缓存、重试、事务、日志等横切关注点的声明式处理。

- [EasyProgress](/EasyProgress/)  
    一个通用、高性能、线程安全的进度管理工具库，通过叶子节点、组合节点和可插拔规则实现单一任务、并行加权、顺序串行、动态子任务及树形嵌套进度，并内置对象池与快捷扩展方法以降低 GC 压力。

- [ViewPipeline.Unity](/ViewPipeline.Unity/)  
    一个为 Unity 游戏引擎设计的高性能、可扩展的视图生命周期管理框架。它采用 **管道-中间件（Pipeline-Middleware）** 架构模式，将视图的打开（Open）和关闭（Close）操作抽象为可编排的执行管道，开发者可以通过中间件机制灵活地插入横切关注点（如权限校验、数据缓存、加载动画、埋点上报等），而无需修改核心视图逻辑。