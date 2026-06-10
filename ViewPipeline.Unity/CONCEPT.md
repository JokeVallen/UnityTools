# ViewPipeline - 核心概念

## 架构概览

ViewPipeline 将视图的打开/关闭流程抽象为双向管道，每个管道由一系列中间件组成。

```
┌────────────────────────────────────────────────────────┐  
│                       ViewSession                      │  
│  ┌─────────────────────┐      ┌─────────────────────┐  │  
│  │   Open Pipeline     │      │   Close Pipeline    │  │  
│  │  ┌───────────────┐  │      │  ┌───────────────┐  │  │  
│  │  │ Middleware 1  │  │      │  │ Middleware 1  │  │  │  
│  │  │ Middleware 2  │  │      │  │ Middleware 2  │  │  │  
│  │  │      ...      │  │      │  │      ...      │  │  │  
│  │  │ Middleware N  │  │      │  │ Middleware N  │  │  │  
│  │  └───────────────┘  │      │  └───────────────┘  │  │  
│  └─────────────────────┘      └─────────────────────┘  │  
└────────────────────────────────────────────────────────┘
```

## 核心概念

### 1. 中间件（Middleware）

中间件是管道中的处理单元，每个中间件可以：
- 在视图显示前执行逻辑（权限检查、数据预加载）
- 决定是否继续执行（`NextAsync`）
- 中断整个管道（`Abort`）
- 在视图显示后执行逻辑（后置处理）

```csharp
public class ExampleMiddleware : IViewMiddleware
{
    public async UniTask InvokeAsync(IView view, ViewPipelineExecutor executor, CancellationToken token)
    {
        // 前置逻辑
        await executor.NextAsync(view, token);  // 继续执行
        // 后置逻辑
    }
}
```

### 2. 扩展包（Extension）

扩展包是将多个中间件、动态供应器、验证器打包成独立模块的机制。

**为什么需要扩展包？**
- 复用：一次编写，多项目使用
- 隔离：扩展包之间互不干扰
- 配置：通过 `IValidatable` 前置检查，确保使用条件满足

**典型扩展包结构：**
```csharp
public class MyExtension : IExtension, IValidatable
{
    // 1. 提供中间件
    public IEnumerable<IViewMiddleware> GetMiddlewares(PipelineDirection direction) { ... }
    
    // 2. 提供动态供应器
    public IEnumerable<IDynamicMiddlewareProvider> GetDynamicProviders(...) { ... }
    
    // 3. 前置验证
    public IValidator GetValidator() { ... }
}
```

### 3. 强类型上下文（ITypedPipelineContext）

用于在中间件之间传递数据，支持零装箱的键值存储。

**使用场景：**
- 登录中间件将用户信息传递给后续中间件
- 权限中间件将权限检查结果共享
- 数据预加载中间件将数据存入上下文供视图使用

```csharp
executor.SetData("userId", 12345);
var userId = executor.GetData<int>("userId");
```

### 4. 快照系统（Snapshot）

快照记录框架各组件在某一时刻的状态，用于：
- **调试**：查看中间件执行到哪一步
- **扩展包验证**：检查构建配置是否满足要求
- **监控**：统计活跃会话数、执行耗时

```csharp
var snapshot = SnapshotCache.Get<ViewSessionSnapshot>(session.Key);
Debug.Log($"活跃操作: {snapshot.ActiveOpenedOperations}");
```

### 5. 验证器（Validator）

扩展包通过实现 `IValidatable` 在 `AddExtension` 时进行前置条件检查。

**验证时机：** `AddExtension` 时自动调用，失败时阻止扩展包添加。

**典型验证场景：**
- 检查是否调用了 `WithTypedContext()`
- 检查依赖的其他扩展包是否已添加
- 检查中间件是否有冲突

### 6. 流程控制

| 机制 | 接口 | 说明 |
|------|------|------|
| 跳过中间件 | `ISkippableView` | 视图决定跳过哪个中间件 |
| 跳过视图 | `ISkippableMiddleware` | 中间件决定跳过哪个视图 |
| 终止执行 | `ITerminable` | 视图或中间件决定终止整个管道 |
| 外部策略 | `IExecutionPolicy` | 集中管理跳过/终止规则 |

## 设计理念

### 为什么是管道模式？

传统基类方式的问题：
- 重复逻辑散落在每个视图
- 新增横切功能需要修改所有视图
- 难以动态调整执行流程

管道模式的优势：
- 横切逻辑集中在中间件
- 新增功能只需添加中间件
- 支持运行时动态调整

### 为什么有双向管道？

Open 和 Close 是两个不同的方向：
- Open 需要加载数据、检查权限、播放动画
- Close 需要清理资源、保存状态

分开管理更清晰，互不干扰。

### 为什么需要强类型上下文？

简单场景可以用字段或参数传递，但中间件之间传递数据需要：
- 类型安全（避免字符串 key 拼错）
- 零装箱（性能敏感）
- 生命周期管理（随执行自动清理）

`ITypedPipelineContext` 解决了这些问题。

### 为什么需要快照？

管道是黑盒，你不知道：
- 当前执行到哪一步
- 哪些中间件被跳过了
- 会话状态是什么

快照把黑盒变成白盒。

## 工作流程

```
1. 构建阶段
   ViewSessionBuilder.Create()
       .WithTypedContext()
       .AddOpenMiddleware(new AuthMiddleware())
       .AddExtension(new AnalyticsExtension())
       .Build()
   
   ├── 验证器执行（扩展包自检）
   ├── 创建 Open/Close 引擎
   └── 创建 ViewSession

2. 执行阶段
   session.OpenViewAsync(view)
   
   ├── 从池获取 Context
   ├── 从池获取 PipelineSession
   ├── 动态供应器贡献中间件
   ├── 静态中间件 + 动态中间件 合并
   ├── 创建 Executor
   ├── 执行中间件链
   │   ├── 检查跳过（ISkippable*）
   │   ├── 检查终止（ITerminable）
   │   ├── 执行 InvokeAsync
   │   └── 循环直到所有中间件执行完毕
   ├── 执行视图 ShowAsync/HideAsync
   └── 归还 Context 和 PipelineSession

3. 释放阶段
   session.DisposeAsync()
   
   ├── 等待进行中的操作完成
   ├── 释放引擎资源
   ├── 释放上下文池
   └── 从注册表移除
```