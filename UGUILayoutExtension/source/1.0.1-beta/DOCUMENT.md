> 内容由 AI 根据核心代码生成，已通过人工审核。

# UGUI.Layout.Extension API 文档

## 概述

`UGUI.Layout.Extension` 是一个基于 Unity UGUI 布局系统扩展的布局工具库，提供了基于动画曲线的自动布局和圆形布局等高级布局组件。

**特别说明：**
- 所有布局组件均继承自 `UIBehaviour`，支持 `ExecuteAlways`，可在编辑器中实时预览。
- 布局组件会自动驱动子节点的 `RectTransform` 属性（通过 `DrivenRectTransformTracker`），确保与 UGUI 布局系统兼容。
- 命名空间：`UGUI.Layout.Extension`

---

## 公共 API

### 1. AutoLayoutGroup

基于 `AnimationCurve` 的自动布局组件，X 轴和 Y 轴可独立配置曲线、映射模式、位置计算模式及分组参数。

```csharp
public sealed class AutoLayoutGroup : BaseAutoLayoutGroup
```

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PreWrapModeX` | `WrapMode` | X 轴曲线前端的行为模式 |
| `PostWrapModeX` | `WrapMode` | X 轴曲线末端的行为模式（Direct/Interpolated 模式生效，Proportional 模式无效） |
| `LengthX` | `int` | X 轴曲线关键帧数量（只读） |
| `KeysX` | `Keyframe[]` | X 轴曲线的关键帧集合 |
| `MappingModeX` | `KeyframeMappingMode` | X 轴关键帧映射模式 |
| `PositionModeX` | `PositionMode` | X 轴位置计算模式（ByElementSize / ByPixel） |
| `ConstrainByGroupX` | `bool` | 是否启用 X 轴 GroupSize 约束（仅在 Interpolated 模式下有效） |
| `GroupSizeX` | `int` | X 轴每个曲线周期包含的布局元素数量（Interpolated + ConstrainByGroupX 生效） |
| `CyclesX` | `float` | X 轴当前覆盖的曲线周期数（只读；未启用 ConstrainByGroupX 时固定返回 1） |
| `DistributeModeX` | `ProportionalDistributeMode` | X 轴 Proportional 模式的分配策略 |
| `ScaleX` | `float` | X 轴偏移缩放系数 |
| `PreWrapModeY` | `WrapMode` | Y 轴曲线前端的行为模式 |
| `PostWrapModeY` | `WrapMode` | Y 轴曲线末端的行为模式 |
| `LengthY` | `int` | Y 轴曲线关键帧数量（只读） |
| `KeysY` | `Keyframe[]` | Y 轴曲线的关键帧集合 |
| `MappingModeY` | `KeyframeMappingMode` | Y 轴关键帧映射模式 |
| `PositionModeY` | `PositionMode` | Y 轴位置计算模式 |
| `ConstrainByGroupY` | `bool` | 是否启用 Y 轴 GroupSize 约束 |
| `GroupSizeY` | `int` | Y 轴每个曲线周期包含的布局元素数量 |
| `CyclesY` | `float` | Y 轴当前覆盖的曲线周期数（只读） |
| `DistributeModeY` | `ProportionalDistributeMode` | Y 轴 Proportional 模式的分配策略 |
| `ScaleY` | `float` | Y 轴偏移缩放系数 |
| `SpacingHorizontal` | `float` | 水平方向固定间距（像素），与曲线偏移叠加 |
| `SpacingVertical` | `float` | 垂直方向固定间距（像素），与曲线偏移叠加 |
| `ReverseArrangement` | `bool` | 是否反序排列布局元素（倒序与曲线对应） |

#### 曲线 API — X 轴

| 方法 | 说明 |
|------|------|
| `Keyframe GetKeyX(int index)` | 获取 X 轴曲线指定索引的关键帧（索引越界时抛出 `IndexOutOfRangeException`） |
| `int AddKeyX(float time, float value)` | 向 X 轴曲线添加关键帧，返回添加的关键帧索引 |
| `int AddKeyX(Keyframe key)` | 向 X 轴曲线添加关键帧，返回添加的关键帧索引 |
| `float EvaluateX(float time)` | 获取 X 轴曲线在指定时间的值 |
| `int MoveKeyX(int index, Keyframe key)` | 移动 X 轴曲线的指定关键帧，返回移动后的索引 |
| `void RemoveKeyX(int index)` | 移除 X 轴曲线的指定关键帧 |
| `void SmoothTangentsX(int index, float weight)` | 平滑 X 轴曲线的指定关键帧切线 |
| `void ClearKeysX()` | 清空 X 轴曲线所有关键帧 |

#### 曲线 API — Y 轴

| 方法 | 说明 |
|------|------|
| `Keyframe GetKeyY(int index)` | 获取 Y 轴曲线指定索引的关键帧（索引越界时抛出 `IndexOutOfRangeException`） |
| `int AddKeyY(float time, float value)` | 向 Y 轴曲线添加关键帧，返回添加的关键帧索引 |
| `int AddKeyY(Keyframe key)` | 向 Y 轴曲线添加关键帧，返回添加的关键帧索引 |
| `float EvaluateY(float time)` | 获取 Y 轴曲线在指定时间的值 |
| `int MoveKeyY(int index, Keyframe key)` | 移动 Y 轴曲线的指定关键帧，返回移动后的索引 |
| `void RemoveKeyY(int index)` | 移除 Y 轴曲线的指定关键帧 |
| `void SmoothTangentsY(int index, float weight)` | 平滑 Y 轴曲线的指定关键帧切线 |
| `void ClearKeysY()` | 清空 Y 轴曲线所有关键帧 |

#### 布局接口实现

| 方法 | 说明 |
|------|------|
| `void CalculateLayoutInputHorizontal()` | 计算水平布局输入（重写自 `BaseAutoLayoutGroup`） |
| `void CalculateLayoutInputVertical()` | 计算垂直布局输入（重写自 `BaseAutoLayoutGroup`） |
| `void SetLayoutHorizontal()` | 设置水平布局（重写自 `BaseAutoLayoutGroup`） |
| `void SetLayoutVertical()` | 设置垂直布局（重写自 `BaseAutoLayoutGroup`） |

---

### 2. BaseAutoLayoutGroup

自动布局组件的抽象基类，提供布局元素收集、脏标记、对齐等通用功能。

```csharp
public abstract class BaseAutoLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup, ILayoutController
```

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `minWidth` | `float` | 最小宽度（ILayoutElement 实现） |
| `minHeight` | `float` | 最小高度（ILayoutElement 实现） |
| `preferredWidth` | `float` | 首选宽度（ILayoutElement 实现） |
| `preferredHeight` | `float` | 首选高度（ILayoutElement 实现） |
| `flexibleWidth` | `float` | 弹性宽度（ILayoutElement 实现） |
| `flexibleHeight` | `float` | 弹性高度（ILayoutElement 实现） |
| `layoutPriority` | `int` | 布局优先级（固定返回 0，ILayoutElement 实现） |
| `Padding` | `RectOffset` | 容器内边距 |
| `ChildAlignment` | `TextAnchor` | 布局元素在容器内的对齐方式（容器大于内容时生效） |

#### 方法

| 方法 | 说明 |
|------|------|
| `abstract void CalculateLayoutInputVertical()` | 计算垂直布局输入（子类实现） |
| `abstract void SetLayoutHorizontal()` | 设置水平布局（子类实现） |
| `abstract void SetLayoutVertical()` | 设置垂直布局（子类实现） |
| `float GetAlignmentOnAxis(int axis)` | 获取指定轴上的对齐系数（0=靠前，0.5=居中，1=靠后） |
| `float GetStartOffset(int axis, float requiredSpaceWithoutPadding)` | 获取布局内容在容器内的起始坐标（考虑 ChildAlignment） |
| `void SetChildAlongAxis(RectTransform rect, int axis, float pos)` | 沿指定轴设置子元素位置，保持尺寸不变（自动驱动 anchor） |
| `void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)` | 沿指定轴设置子元素位置和尺寸（自动驱动 anchor） |
| `void SetDirty()` | 标记布局为脏，触发布局重建 |

---

### 3. CircleLayoutGroup

圆形布局组件，将子元素均匀分布在圆周上。

```csharp
public sealed class CircleLayoutGroup : LayoutGroup
```

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Radius` | `float` | 圆形半径（设置负值无效） |
| `Rotation` | `int` | 起始旋转角度（度），自动归一化到 [0, 360) |
| `ClockWise` | `bool` | 是否顺时针布局 |

#### 布局接口实现

| 方法 | 说明 |
|------|------|
| `void CalculateLayoutInputHorizontal()` | 计算水平布局输入（重写自 `LayoutGroup`） |
| `void CalculateLayoutInputVertical()` | 计算垂直布局输入（重写自 `LayoutGroup`） |
| `void SetLayoutHorizontal()` | 设置水平布局（重写自 `LayoutGroup`） |
| `void SetLayoutVertical()` | 设置垂直布局（重写自 `LayoutGroup`） |

---

### 4. CurveIndexStepper

基于 `WrapMode` 的曲线关键帧索引步进器，封装了 `Loop`、`PingPong` 和 `Clamp` 模式下的索引步进逻辑。

```csharp
public struct CurveIndexStepper
```

> **注意：** `PingPong` 模式持有可变方向状态，每次布局计算需通过 `Create` 工厂方法获取新实例，不可跨布局复用。

#### 方法

| 方法 | 说明 |
|------|------|
| `static CurveIndexStepper Create(WrapMode mode, int minIndex, int maxIndex, int step = 1)` | 创建步进器实例（`minIndex` > `maxIndex` 或 `step` = 0 时抛出 `ArgumentException`） |
| `int Next(int current)` | 根据当前索引步进到下一个索引（`current` 超出范围时抛出 `ArgumentOutOfRangeException`） |
| `static int Resolve(int childIndex, int keyCount, WrapMode mode, int step = 1)` | 无状态地将布局元素序号直接映射到关键帧索引（Direct 模式专用） |

---

### 5. 枚举类型

#### KeyframeMappingMode

| 值 | 说明 |
|----|------|
| `Direct` | 关键帧与布局元素一一对应，超出时按 `PostWrapMode` 循环 |
| `Interpolated` | 布局元素索引归一化后在曲线上连续采样 |
| `Proportional` | 布局元素均匀映射到关键帧索引上，取对应关键帧的坐标值 |

#### PositionMode

| 值 | 说明 |
|----|------|
| `ByElementSize` | `pos = effectiveSize × factor × scale`，偏移与元素自身尺寸成比例 |
| `ByPixel` | `pos = factor × scale`，曲线直接描述像素偏移，与元素尺寸无关 |

#### ProportionalDistributeMode

| 值 | 说明 |
|----|------|
| `RoundToNearest` | 四舍五入映射到最近关键帧（默认） |
| `Uniform` | 基于 Bresenham 算法均匀分配，各组数量差不超过 1 |
| `FloorBias` | 前密后疏（靠前的关键帧分配更多元素） |
| `CeilBias` | 前疏后密（靠后的关键帧分配更多元素） |

---

## 使用示例

### 示例 1：AutoLayoutGroup —— 波浪布局

```csharp
using UnityEngine;
using UGUI.Layout.Extension;

public class WaveLayoutExample : MonoBehaviour
{
    [SerializeField] private AutoLayoutGroup layoutGroup;

    private void Start()
    {
        // X 轴：线性递增，实现水平排列
        layoutGroup.KeysX = new Keyframe[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 1f)
        };
        layoutGroup.MappingModeX = KeyframeMappingMode.Interpolated;
        layoutGroup.ScaleX = 200f;

        // Y 轴：正弦波，实现上下起伏
        layoutGroup.KeysY = new Keyframe[]
        {
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1f),
            new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -1f),
            new Keyframe(1f, 0f)
        };
        layoutGroup.MappingModeY = KeyframeMappingMode.Interpolated;
        layoutGroup.ScaleY = 50f;

        layoutGroup.SpacingHorizontal = 10f;
    }
}
```

### 示例 2：AutoLayoutGroup —— 直接映射模式

```csharp
using UnityEngine;
using UGUI.Layout.Extension;

public class DirectMappingExample : MonoBehaviour
{
    [SerializeField] private AutoLayoutGroup layoutGroup;

    private void Start()
    {
        // Direct 模式：每个元素对应一个关键帧，超出时 Loop
        layoutGroup.KeysX = new Keyframe[]
        {
            new Keyframe(0f, 0f),   // 第 1 个元素 → 0
            new Keyframe(0f, 0.5f), // 第 2 个元素 → 0.5
            new Keyframe(0f, 1f)    // 第 3 个元素 → 1
        };
        layoutGroup.MappingModeX = KeyframeMappingMode.Direct;
        layoutGroup.PostWrapModeX = WrapMode.Loop;
        layoutGroup.ScaleX = 100f;
    }
}
```

### 示例 3：CircleLayoutGroup —— 环形菜单

```csharp
using UnityEngine;
using UGUI.Layout.Extension;

public class CircleMenuExample : MonoBehaviour
{
    [SerializeField] private CircleLayoutGroup circleLayout;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int itemCount = 8;

    private void Start()
    {
        // 清空现有子元素
        foreach (Transform child in circleLayout.transform)
            Destroy(child.gameObject);

        // 创建菜单项
        for (int i = 0; i < itemCount; i++)
        {
            GameObject item = Instantiate(itemPrefab, circleLayout.transform);
            item.name = $"Item_{i}";
        }

        // 配置圆形布局
        circleLayout.Radius = 200f;
        circleLayout.Rotation = -90; // 从顶部开始
        circleLayout.ClockWise = true;
    }
}
```

### 示例 4：CurveIndexStepper —— Direct 模式索引解析

```csharp
using UnityEngine;
using UGUI.Layout.Extension;

public class IndexResolverExample : MonoBehaviour
{
    private void Start()
    {
        // 5 个关键帧，Loop 模式，步长为 1
        // 第 7 个元素对应的关键帧索引: 7 % 5 = 2
        int index = CurveIndexStepper.Resolve(
            childIndex: 7,
            keyCount: 5,
            mode: WrapMode.Loop,
            step: 1
        );
        Debug.Log($"第 7 个元素映射到关键帧索引: {index}"); // 输出: 2

        // PingPong 模式，5 个关键帧
        // 周期 = 2 * (5 - 1) = 8
        // 第 7 个元素: 7 % 8 = 7，反向段 => 8 - 7 = 1
        int pingPongIndex = CurveIndexStepper.Resolve(
            childIndex: 7,
            keyCount: 5,
            mode: WrapMode.PingPong,
            step: 1
        );
        Debug.Log($"PingPong 映射结果: {pingPongIndex}"); // 输出: 1
    }
}
```

### 示例 5：CurveIndexStepper —— 步进器使用

```csharp
using UnityEngine;
using UGUI.Layout.Extension;

public class StepperExample : MonoBehaviour
{
    private void Start()
    {
        // 创建步进器：5 个关键帧，PingPong 模式，步长 2
        var stepper = CurveIndexStepper.Create(
            mode: WrapMode.PingPong,
            minIndex: 0,
            maxIndex: 4,
            step: 2
        );

        int current = 0;
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"步进 {i}: {current}");
            current = stepper.Next(current);
        }
        // 输出: 0 → 2 → 4 → 2 → 0 → 2 → 4 → 2 → 0 → 2
    }
}