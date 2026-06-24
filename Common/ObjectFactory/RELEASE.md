## 1.0.1-beta

### 移除

- 移除 `IGameObjectFactory`、`IComponentFactory`。
- 移除 `IObjectFactory` 中的 `ThrowOnError` 属性。
- 移除 `ObjectFactory` 中获取默认的 `ComponentFactory`、`GameObjectFactory` 的方法。

### 添加

- 添加 `IFactoryCreator` 和 `IFactoryCreator<T>` 接口。
- `ComponentFactory` 和 `GameObjectFactory` 分别添加传入额外自定义回调参数的方法。
- `ComponentFactory`、`GameObjectFactory` 开放构造函数。
- `ObjectFactory` 补充 `IFactoryCreator` 构建工厂实例的机制。

### 修改

- `ObjectFactory` 不再默认嵌入 `ComponentFactory`、`GameObjectFactory`，也不提供降级处理机制。
- 公开 `ComponentFactory`、`GameObjectFactory`。

### 其它

- 提供了类型安全保障，完善泛型和非泛型版本。
- 对内存分配和耗时进行优化。

### 特别说明

不兼容 1.0.0-beta 版本。