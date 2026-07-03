## 1.0.1-beta

### 修改

- 所有涉及自定义输出的信息（异常、日志）统一添加 `[GlobalTimer]` 前缀。 
- 修复内部计时器核心类部分 API 缺少已释放检测。
- 同时存活的计时任务数量上限上调至 `2048`。
- 修改 `TimerJob` 中的 `groupId` 字段类型为 `Optional<int>`。

### 移除

- 移除 `TimeSource` 枚举。
- 移除 `GlobalTimer` 的过多重载，改用可选参数，减少重载方法数量。
- 移除 `TimerJob` 中的 `hasGroup` 字段。

### 添加

- `GlobalTimer` 添加 `RegisterManual`、`RegisterWallClock`、`RegisterIndependentFrame`、`RegisterIndependent`、`Register`、`RegisterMonoFixedUnscaled`、`RegisterCoroutineWaitForFixedUpdate`、`CancelAll` 方法。
- 添加 `TimeDelta` 和 `TimeSchedule` 枚举。
- 添加 `TimeSource` 结构体。
- 添加 `Optional<T>` 结构体。

### 特别说明

- 兼容 1.0.0-beta 版本，但时间源机制改为了更灵活的 `原子级增量计算方式 + 原子级驱动调度时机` 的组合策略，该版本提供了35种组合可能，1.0.0-beta 版本的时间源已纳入其中。

## 1.0.2-beta

### 修改

- `InnerTimer` 改名为 `InnerRuntimeTimer`。
- `Register` 方法提升 `interval` 参数检验错误等级：由抛出警告信息改为抛出异常。
- 修改部分日志输出和异常信息。
- `TimeDelta.Scaled` 的时间间隔计算补充乘以 `TimeSource.Scale`。
- `TimerHandle` 隐藏 `SlotIndex` 和 `Generation` 成员。
- `InnerTimer` 传入 `groupId` 的方法参数类型改为 `Optional<int>`。
- `InnerTimer` 的 `TickTimerSlots` 方法参数改为 `TimeSchedule currentSchedule`，相关调用的地方也同步修改。

### 添加

- 添加 `InnerEditorTimer`、`EditorTimer`、`EditorTimerHandle` 用于 Unity 编辑器非运行时。
- `Extension` 补充 `EditorTimerHandle` 的静态扩展方法。

### 特别说明

- 补充对 Unity 编辑器的支持。