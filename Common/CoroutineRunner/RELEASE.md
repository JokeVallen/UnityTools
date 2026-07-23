## 1.0.1-beta

### 添加

- 异常或日志信息添加 `[CoroutineRunner]` 前缀。
- `InternalCoroutineRunner` 中添加内置泛型存储桶，以提供通道标识的泛型支持。

### 修改

- `CoroutineHandleToken` 的扩展方法 `GetAwaiter` 移除 `in` 关键字的修饰。
- `CoroutineHandleToken` 的静态只读字段 `NullToken` 改名为 `None`。
- `IGlobalCoroutineRunner` 的接口方法 `RunQueued` 和 `ConfigureChannel` 分别改名为 `CoroutineHandleToken RunQueued<T>(IEnumerator routine, T channelKey)` 和 `void ConfigureChannel<T>(T channelKey, int maxConcurrent)`。