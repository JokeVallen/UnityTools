> 内容由 AI 根据核心代码生成，已通过人工审核。

---

# UGUI 扩展布局组件 API 文档

本文档涵盖以下自定义 UGUI 布局组件：
- `AutoLayout` — 基于 AnimationCurve 的可编程自适应布局
- `WaveLayout` — 波形布局（继承自 `AutoLayout`，提供波状排列）
- `CircleLayout` — 圆形布局

---

## 1. AutoLayout

### 命名空间
`GameAssistant.Core.UI.Layout`（Editor 部分） / 全局（运行时）

### 继承关系
`AutoLayout` → `LayoutGroup` → `UIBehaviour`

### 说明
`AutoLayout` 是一个基于 `AnimationCurve` 驱动的布局组件。它将子物体按照曲线上的 **关键点（Keyframe）** 进行位置映射，实现沿 X 轴和 Y 轴的可控非均匀排列。同时支持曲线末端模式（WrapMode），控制超出关键点范围时的行为。

---

### 公共属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Keys` | `IReadOnlyList<Keyframe>` | 当前曲线的所有关键点（只读）。 |
| `Length` | `int` | 当前曲线的关键点数量。 |
| `CurveEndMode` | `WrapMode` | 曲线末端行为模式（如 Clamp、Loop、PingPong 等）。 |
| `XMultiplier` | `float` | 曲线水平轴（时间）的缩放乘数，影响子物体间的水平间距。 |
| `YMultiplier` | `float` | 曲线垂直轴（值）的缩放乘数，影响子物体间的垂直间距。 |

---

### 公共方法

| 方法签名 | 说明 |
|----------|------|
| `int AddKey(float time, float value)` | 添加一个关键点，返回其索引；若失败返回 -1。 |
| `int AddKey(Keyframe key)` | 添加一个关键点（完整结构），返回索引；若失败返回 -1。 |
| `float Evaluate(float time)` | 根据时间值计算曲线上的对应值。 |
| `int MoveKey(int index, Keyframe key)` | 将指定索引的关键点替换为新关键点，返回新索引。 |
| `void RemoveKey(int index)` | 删除指定索引的关键点。 |
| `void SmoothTangents(int index, float weight)` | 平滑指定关键点的进出切线。 |
| `void ClearKeys()` | 清空所有关键点。 |

> 上述方法均会调用 `SetDirty()` 触发布局刷新。

---

### 布局行为说明

- **水平布局（CalculateLayoutInputHorizontal）**：  
  根据曲线关键点之间的时间差（`time`）和 `XMultiplier` 计算子物体间的水平间距。

- **垂直布局（CalculateLayoutInputVertical）**：  
  根据曲线关键点之间的值差（`value`）和 `YMultiplier` 计算子物体间的垂直间距。

- **子物体定位（SetLayoutHorizontal / Vertical）**：  
  每个子物体的位置根据其在子列表中的顺序，映射到曲线上的对应关键点，再乘以对应轴乘数。

- **WrapMode 控制**：  
  当子物体数量超过关键点数量时，`CurveEndMode` 决定如何循环或截断关键点索引。

---

### 使用示例

```csharp
using UnityEngine;
using GameAssistant.Core.UI.Layout; // 若 Editor 部分需要，请额外 using

public class ExampleAutoLayout : MonoBehaviour
{
    public AutoLayout autoLayout;

    void Start()
    {
        // 添加三个关键点，形成正弦波形
        autoLayout.AddKey(0.0f, 0.0f);
        autoLayout.AddKey(0.5f, 1.0f);
        autoLayout.AddKey(1.0f, 0.0f);

        // 设置末端循环
        autoLayout.CurveEndMode = WrapMode.Loop;

        // 水平拉伸系数为 2，垂直拉伸系数为 1.5
        autoLayout.XMultiplier = 2f;
        autoLayout.YMultiplier = 1.5f;
    }
}
```

---

## 2. WaveLayout

### 继承关系
`WaveLayout` → `AutoLayout` → `LayoutGroup` → `UIBehaviour`

### 说明
`WaveLayout` 是 `AutoLayout` 的扩展，专门用于生成 **波形排列**。它内部维护了一个曲线模板（默认三个关键点：`(0,0)`、`(0.5,0.5)`、`(1,0)`），并随着子物体数量的变化 **动态增删关键点**，保持波形在子物体数量变化时自动延续。

---

### 重要特性

- **动态曲线更新**：  
  当子物体数量变化时，`WaveLayout` 会自动根据模板曲线补充或移除关键点，确保每个子物体都对应一个关键点。

- **关键点模板**：  
  内部使用 `CurveTemplateGetter` 返回默认的三个关键点，新添加的关键点会根据模板中相邻关键点的时间差和值差自动生成。

- **禁用手动关键点操作**：  
  `AddKey`、`ClearKeys`、`MoveKey`、`RemoveKey`、`SmoothTangents` 等方法被重写为空操作或返回 -1，防止用户手动干预关键点。

---

### 公共属性（继承自 AutoLayout）

| 属性 | 说明 |
|------|------|
| `CurveEndMode` | 曲线末端模式（继承自 `AutoLayout`）。 |
| `XMultiplier` | 水平乘数（继承）。 |
| `YMultiplier` | 垂直乘数（继承）。 |

> 注：`WaveLayout` 未新增公共属性或方法。

---

### 使用示例

```csharp
using UnityEngine;
using GameAssistant.Core.UI.Layout;

public class ExampleWaveLayout : MonoBehaviour
{
    public WaveLayout waveLayout;

    void Start()
    {
        // 设置波形参数
        waveLayout.XMultiplier = 3f;
        waveLayout.YMultiplier = 2f;
        waveLayout.CurveEndMode = WrapMode.PingPong;

        // 子物体数量变化时，波形会自动调整
        // 例如：动态添加或删除子物体，无需额外代码
    }
}
```

---

## 3. CircleLayout

### 继承关系
`CircleLayout` → `LayoutGroup` → `UIBehaviour`

### 说明
`CircleLayout` 是一个将子物体 **均匀分布在圆周上** 的布局组件。支持设置半径、起始旋转角度和排列方向（顺时针/逆时针）。

---

### 公共属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Radius` | `float` | 圆形布局的半径，不可为负。 |
| `Rotation` | `int` | 起始旋转角度（0～360 度）。 |
| `ClockWise` | `bool` | 是否按顺时针方向排列子物体（`true` 为顺时针，`false` 为逆时针）。 |

---

### 布局行为说明

- 所有子物体均匀分布在圆周上，角度间隔 = `360° / 子物体数量`。
- 角度偏移起始于 `Rotation` 指定的角度。
- 位置计算基于圆形的三角函数（`Cos` 用于 X 轴，`Sin` 用于 Y 轴）。
- 布局的最小/首选/灵活尺寸由 `padding` + `半径 * 2` 决定。

---

### 使用示例

```csharp
using UnityEngine;
using GameAssistant.Core.UI.Layout;

public class ExampleCircleLayout : MonoBehaviour
{
    public CircleLayout circleLayout;

    void Start()
    {
        // 设置半径为 200
        circleLayout.Radius = 200f;

        // 从 45 度开始排列
        circleLayout.Rotation = 45;

        // 逆时针排列
        circleLayout.ClockWise = false;
    }
}
```

---

## Editor 支持

所有组件均提供对应的自定义 Inspector 编辑器（位于 `GameAssistant.Core.UI.Layout` 命名空间）：
- `AutoLayoutEditor`
- `WaveLayoutEditor`
- `CircleLayoutEditor`

编辑器在 Inspector 中显示所有可序列化字段，并将 `m_Script` 字段设为只读，避免误操作。