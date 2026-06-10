### v1.0.1-beta (当前)
- 新增 `ITypedPipelineContext`：强类型上下文，零装箱键值存储
- 新增 `Optional<T>`：可选值包装器，区分「无值」和「值为默认值」
- 新增 `SnapshotCache` / `SnapshotCache<TTag>`：快照存储和查询系统
- 新增 `IValidatable`：扩展包前置验证机制
- 新增 `ViewSessionBuilder.WithTypedContext()`：一行启用强类型上下文
- 新增 `ViewSessionRegistry`：全局会话注册表
- 优化：`ViewSessionBuilderSnapshot` 包含 `ContextType` 信息
- 优化：`ViewPipelineExecutor` 扩展方法（`GetTypedContext`、`SetData`、`GetData`）