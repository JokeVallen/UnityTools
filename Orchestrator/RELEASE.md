## 1.0.1-beta

### 修改

- `IStep` 接口引入泛型 `Tkey`，改为 `IStep<TKey>`。
- `IStep` 接口成员 `string Name{ get; }` 改为 `TKey Key{ get; }`。
- 其它引用 `IStep` 的成员同步修改。
- 所有涉及输入和输出语义的成员都对两种语义进行移除并同步相关修改。  
- 改用上下文通信和进行输入输出等数据传递。

### 移除

- 移除接口 `IExecutionResult`。
- 移除接口 `IStepExecutionResult`。
- `ExecutionResult`、`StepExecutionResult`、`StepResult` 等移除对数据对象的直接或间接引用，改为上下文传递机制。
- `TaskOrchestratorUtility` 被移除。
- `ITaskContextStep` 被移除。
- `UniTaskOrchestratorUtility` 被移除。
- `IUniTaskContextStep` 被移除。
- `ValueTaskOrchestratorUtility` 被移除。
- `IValueTaskContextStep` 被移除。

### 添加

- 引入集合对象池机制：`DictionaryPool`、`ListPool`、`ArrayPool`。
- 引入类型安全的上下文机制：`ITypedPipelineContext`、`TypedPipelineContext`、`Optional<T>`。
- 添加 `TaskBehaviorStepper` 行为步进器。
- 添加 `UniTaskBehaviorStepper` 行为步进器。
- 添加 `ValueTaskBehaviorStepper` 行为步进器。
- 各异步版本编排器的构建器添加了更易用的添加行为的 API，并且可以对任意步骤进行行为的定制化添加。

### 优化

- 所有涉及使用临时集合的代码逻辑统一改用集合池进行优化，以降低GC分配。
- 行为步进器替换原本的委托，采用结构体设计，以避免GC分配。
- 对部分使用Linq的代码进行进一步优化，改用普通循环来避免迭代器分配。

### 特别说明

该版本不兼容 `1.0.0-beta` 版本。