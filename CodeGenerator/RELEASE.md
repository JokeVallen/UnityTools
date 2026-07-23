## 1.0.1-beta

### 添加

- 添加 `GeneratorAttribute`：用于标注代码生成器；
- 添加 `IAsyncGeneratorWithContext`：带上下文的异步代码生成器；
- 添加 `ISyncGeneratorWithContext`：带上下文的同步代码生成器；
- 添加 `ITypedContext`：强类型上下文接口；
- 添加 `TypedContext`：强类型上下文默认实现；
- 添加 `Optional<T>`：可选值包装器；
- 添加 `IResettable`：重置能力接口；
- 添加 `IMediatorWithContext`：同步代码生成器中介者的带上下文能力接口；
- 添加 `IAsyncMediatorWithContext`：异步代码生成器中介者的带上下文能力接口；

### 修改

- `IGeneratorAsync`、`ITemplateProviderAsync`、`IWriterAsync` 分别改名为 `IAsyncGenerator`、`IAsyncTemplateProvider`、`IAsyncWriter`；
- `IGenerator<TTemplate, TContent>` 改名为 `ISyncGenerator<TTemplate, TContent>`；
- `IGeneratorMediatorAsync` 改名为 `IAsyncMediator`；
- `IGeneratorMediator` 改名为 `IMediator`；
- `BaseGeneratorMediator<TGenerator>` 改名为 `BaseMediator<TGenerator>`；