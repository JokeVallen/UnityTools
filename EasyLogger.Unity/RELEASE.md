## 1.0.1-beta

### 移除

- 移除 `ThrowOnError`、`OnError` 相关配置，所有异常显式抛出，调用层可以进一步封装提供异常捕获版本。
- `ILogger` 仅保留 `Log` 方法，其它均移除。
- 移除 `LogDriver`。
- 在 `LogUtility` 中弃用了 `Debug` 中的 `HelloWorld()` 和 `DisposeOnUnityThread()` API。
- `UnityDebugHandler` 不再提供手动预热，将自动在 `RuntimeInitializeLoadType.SubsystemRegistration` 阶段自动预热实现接管 Unity 原生 Debug 的日志流。

### 添加

- `LogUtility` 添加自动刷入消息缓冲区的机制，基于 `Monobehavior` 的协程实现，默认不启用，可通过 `LogUtility.EnableAutoFlush(float)` 启用以及通过 `LogUtility.DisableAutoFlush()` 禁用。
- 接口 `ILoggerWithContext`：用于扩展需要附带调用信息上下文的日志记录器。
- `LogContext` 日志上下文，可通过其提供的静态方法快速捕获调用信息。
- `LogMessage` 添加日志上下文属性。
- 添加 `ICoroutineProxy` 接口。
- `FileLoggerConfig` 增加 `ICoroutineProxy` 配置。

### 修改

- `Debug` 入口类更名为 `LogUtility`。
- `ConsoleLogger` 改写为更加贴合 Unity 原生控制台的输出。
- `LogUtility` 开放 `Logger` 的访问。
- 修复 `FileLogger` 已知问题。
- 日志工具释放时机也改为了采用自动机制，通过 `UnityEngine.Application.quitting` 的事件自动释放。