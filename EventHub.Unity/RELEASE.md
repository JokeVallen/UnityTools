## 1.0.1-beta

### 句柄监视器（SubscriptionMonitor）

- 补充释放状态监测，当监视器被释放后，继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
- 由于 UniTask 依赖 Unity PlayerLoop，而 Unity PlayerLoop 的可用时机须在第一个场景加载完成后，无法提供可靠的懒加载支持，故移除 Instance 属性访问中的懒加载机制，在内部完成初始化之前访问需注意空引用问题。
- 句柄监视器的释放不受到 EventDispatcher 的 `Dispose()` 管控，但内部注册了 Unity 相关回调以确保释放，但使用者也可以自行决定提前释放的时机。

### 句柄监视器配置（SubscriptionMonitorConfig）

- 添加 API
	- `public static void Dispose()`：释放资源，释放后继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
- - 句柄监视器配置的释放不受到 EventDispatcher 的 `Dispose()` 管控，但内部注册了 Unity 相关回调以确保释放，但使用者也可以自行决定提前释放的时机。

### Unity 静态扩展方法（UnityExtension）

- 在每个公共 API 中添加对句柄监视器实例的检测，若句柄监视器未初始化则进行日志输出，避免触发空引用异常。
- 新增 Try 系列 API。
- 非 Try 系列方法自动降级调用静态 API 并返回事件订阅句柄，此时句柄将不受到句柄监视器管控，需要使用者自行在句柄监视器初始化完成后注册。
- Try 系列方法不会降级调用静态 API，而是返回是否成功订阅的结果。

### 事件分发器日志工具类（EventDispatcherLog）

- 添加 API
	- `public static void Dispose()`：释放资源，释放后继续访问相关公共 API 不触发异常，但是会转接到空白实现。 

### 异常捕获工具类（ExceptionCatcher）

- 添加 API
	- `public static void Dispose()`：释放资源，释放后继续访问相关公共 API 不触发异常，但是会转接到空白实现。 

### 事件分发器内部实现类（EventDispatcherInternal）

- 添加释放所有资源的 API
	- `public static void Dispose()`：释放所有资源。外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。释放后继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
	- `public static void SafeDispose()`：安全释放所有资源。外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。安全释放会避免在其它线程访问事件集合过程中触发释放而导致异常，但这意味着可能会比 `EventDispatcher.Dispose()` 方法更慢更久。释放后继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
- 添加批量取消订阅的 API
	- `public static int UnsubscribeAsyncEvents<TEvent>()`：取消订阅指定异步事件类型的所有事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllAsyncEvents()`：取消订阅所有异步事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeSyncEvents<TEvent>()`：取消订阅指定同步事件类型的所有事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllSyncEvents()`：取消订阅所有同步事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllEvents()`：取消订阅所有事件并返回取消订阅的事件数量。

### 事件分发器入口类（EventDispatcher）

- 添加释放所有资源的 API
	- `public static void Dispose()`：释放所有资源。外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。释放后继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
	- `public static void SafeDispose()`：安全释放所有资源。外部注入的可替换组件的生命周期不受本工具库管控，工具库只会置空对它们的引用，可替换组件的释放由使用者自行负责。安全释放会避免在其它线程访问事件集合过程中触发释放而导致异常，但这意味着可能会比 `EventDispatcher.Dispose()` 方法更慢更久。释放后继续访问相关公共 API 将触发异常 `ObjectDisposedException`。
- 添加批量取消订阅的 API
	- `public static int UnsubscribeAsyncEvents<TEvent>()`：取消订阅指定异步事件类型的所有事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllAsyncEvents()`：取消订阅所有异步事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeSyncEvents<TEvent>()`：取消订阅指定同步事件类型的所有事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllSyncEvents()`：取消订阅所有同步事件并返回取消订阅的事件数量。
	- `public static int UnsubscribeAllEvents()`：取消订阅所有事件并返回取消订阅的事件数量。

### 全局特性

- 对引用类型和值类型作为事件类型的优化，避免了值类型装箱和引用类型的类型转换开销，进一步提升工具库整体性能。