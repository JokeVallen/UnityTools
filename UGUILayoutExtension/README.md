> 项目由 AI 和作者共同设计和开发，已进行基本的单元测试和功能测试，具体测试请查看 `Tests` 下相关文件。

# UGUI Layout Extension

![License](https://img.shields.io/badge/License-MIT-green) ![Unity](https://img.shields.io/badge/Unity-2020.3+-black?logo=unity) ![.NET](https://img.shields.io/badge/.NET_Standard-2.0-512BD4) ![](https://img.shields.io/badge/Unit%20Tests-passing-passing) ![UGUI](https://img.shields.io/badge/UGUI-1.0.0+-black?logo=unity)

基于 UGUI 布局系统扩展的布局组件库，通过动画曲线和参数配置实现复杂的非线性 UI 布局，无需编写代码。完整融入 UGUI 布局系统，与 `LayoutElement`、`ContentSizeFitter`、`LayoutGroup` 等官方组件兼容搭配。

---

## AutoLayout

通过两条独立的动画曲线（X 轴 / Y 轴）驱动布局元素的位置分布。**X 轴和 Y 轴的所有参数均可独立配置**，包括映射模式、位置计算模式及分组参数，两轴互不干扰，可以覆盖大多数非线性布局需求。

### 曲线语义

两条曲线均以关键帧的 **value 值**作为偏移因子，两轴语义对称：

- **X 轴曲线**：value 为正时元素偏向右方，value 为负时元素偏向左方
- **Y 轴曲线**：value 为正时元素偏向上方，value 为负时元素偏向下方

### 位置计算模式（X / Y 轴独立）

| 模式 | 公式 | 说明 |
|------|------|------|
| **ByElementSize**（默认）| `pos = effectiveSize × factor × scale` | 偏移量以布局元素自身尺寸为单位缩放，factor 为 1 时偏移恰好等于自身宽/高，适合尺寸一致的场景 |
| **ByPixel** | `pos = factor × scale` | 曲线直接描述像素偏移，与元素尺寸无关，不同尺寸的元素在相同关键帧下落在相同位置，适合精确像素定位的场景 |

---

### Direct 模式

每个布局元素与曲线关键帧**一一对应**，关键帧的 `value` 值决定该方向的偏移因子。元素数量超过关键帧数量时，按 `Post Wrap Mode` 处理。

**适合的场景**

- **精确控制每个位置**：手动在曲线编辑器中放置关键帧，每个元素独立定位，适合图标散布在特定坐标的不规则排布。
- **循环图案**：`Post Wrap Mode = Loop`，少量关键帧定义一个图案单元，元素数量无论多少都循环复用。例如 3 个关键帧描述三角形，几十个元素反复排成三角形组。
- **镜像来回**：`Post Wrap Mode = PingPong`，布局在关键帧序列正向和反向之间来回切换，产生对称镜像效果。
- **自定义路径**：X 轴与 Y 轴曲线各自独立设计，元素沿任意二维路径排布，如 S 形、螺旋形、折线形。

---

### Interpolated 模式

布局元素索引归一化后在曲线上**连续采样**，X 轴和 Y 轴均由各自曲线的 `Evaluate(t)` 决定位置。

- **未启用 Constrain By Group**：`t = i / (count - 1)`，所有元素均匀铺满曲线的 `[0, 1]` 区间，增删元素时整体形状不变，只有密度变化。
- **启用 Constrain By Group**：`t = i / Group Size`，每 `Group Size` 个元素走完一个曲线周期，超出部分按 `Post Wrap Mode` 处理。

X 轴和 Y 轴的 `Constrain By Group` 与 `Group Size` 相互独立，可以分别设置不同的分组粒度。

**适合的场景**

- **整体造型排列**：通过曲线形状控制整体高低起伏，制作拱形、波浪、抛物线等具有造型感的排列。
- **多周期波浪**：启用 `Constrain By Group` + `Post Wrap Mode = Loop`，实现连续多段波浪，适合大量卡牌、列表项等按波形铺开。
- **来回起伏**：启用 `Constrain By Group` + `Post Wrap Mode = PingPong`，产生山峰—山谷—山峰的来回起伏效果。
- **局部区段**：`Group Size` 设为大于元素总数的值，所有元素只覆盖曲线前半段，展示曲线的局部形状。
- **X / Y 轴不同频率**：X 轴 `Group Size = 4`，Y 轴 `Group Size = 6`，两轴以不同周期交替，产生李萨如图形般的二维分布。

> **注意**：`Post Wrap Mode` 在 Interpolated 模式下始终有效——即使未启用 `Constrain By Group`，当 `t` 超出曲线关键帧定义范围时，`Post Wrap Mode` 也会决定采样行为。

---

### Proportional 模式

布局元素**均匀映射**到关键帧索引上，多个元素可以共享同一关键帧位置，形成视觉分组效果。通过 `Distribute Mode` 控制分配策略。X 轴和 Y 轴的 `Distribute Mode` 相互独立。

| 分配策略 | 说明 |
|----------|------|
| **RoundToNearest**（默认）| 四舍五入映射到最近的关键帧，分组边界处数量差不超过 1 |
| **Uniform** | 基于 Bresenham 算法均匀分配，各组数量差不超过 1，分布最均匀 |
| **FloorBias** | 靠前的关键帧分配更多元素，前密后疏 |
| **CeilBias** | 靠后的关键帧分配更多元素，前疏后密 |

**适合的场景**

- **分组聚集**：10 个元素对应 3 个关键帧，每个关键帧附近聚集约 3—4 个元素，适合分类标签、技能图标分组。
- **重心偏移**：`FloorBias` / `CeilBias` 人为制造疏密对比，用于视觉引导或强调布局重心。
- **X / Y 轴不同分组**：X 轴 `Proportional` 产生水平分组，Y 轴 `Interpolated` 产生波形高度，两轴不同模式叠加实现复杂二维分布。

---

### Inspector 参数说明

Inspector 中参数分为**通用属性**和 **X Axis / Y Axis 两个独立折叠组**。

#### 通用属性

| 参数 | 说明 |
|------|------|
| Padding | 容器四边内边距（left / right / top / bottom 均生效） |
| Spacing Horizontal | 元素间水平固定间距（像素，≥ 0），叠加在曲线偏移之上 |
| Spacing Vertical | 元素间垂直固定间距（像素，≥ 0），叠加在曲线偏移之上（在 UGUI 坐标系中向下叠加；若 Y 轴曲线产生向上偏移，SpacingVertical 会部分抵消该偏移，此时应通过增大 Scale Y 来扩大元素间距） |
| Reverse Arrangement | 启用后元素按倒序与曲线对应，可快速翻转排列方向而无需修改曲线 |
| Child Alignment | 内容在容器内的整体对齐方式，容器有剩余空间时生效 |

#### X Axis / Y Axis 折叠组（各轴独立）

| 参数 | 说明 |
|------|------|
| 曲线编辑器 | 该轴的布局曲线，关键帧 `value` 值为对应方向的偏移因子 |
| Pre Wrap Mode | 曲线前端行为模式（当前版本对所有映射模式均无效，显示为只读） |
| Post Wrap Mode | 曲线超出末端时的行为（`Loop` / `PingPong` / `Clamp` 等）；`Proportional` 模式下无效，显示为只读 |
| Mapping Mode | 映射模式：`Direct` / `Interpolated` / `Proportional` |
| Position Mode | 位置计算模式：`ByElementSize` 或 `ByPixel` |
| ↳ Constrain By Group | （仅 `Interpolated`）启用后按 `Group Size` 控制每周期元素数量 |
| ↳ Group Size | （仅 `Interpolated` + `Constrain By Group`）每个曲线周期包含的元素数量 |
| ↳ Cycles | （仅 `Interpolated` + `Constrain By Group`，只读）当前元素序列覆盖的曲线周期数 |
| ↳ Distribute Mode | （仅 `Proportional`）分配策略：`RoundToNearest` / `Uniform` / `FloorBias` / `CeilBias` |
| Scale | 该轴偏移缩放系数 |

---

## CircleLayout

将布局元素等间距排列在一个圆周上，圆心固定在组件自身 `RectTransform` 的中心。

**适合的场景**

- **环形菜单 / 技能轮盘**：元素自动均匀分布在圆周上，增减元素时角度自动重新分配，无需手动调整。
- **钟表 / 仪表盘刻度**：通过 `Rotation` 控制起始角度，配合顺/逆时针方向，精确还原钟表刻度或雷达图顶点。
- **装饰性圆形布局**：图标、头像、装饰元素绕圆心排列，半径和旋转均可在运行时动态调整，便于制作展开/收起动画。

| 参数 | 说明 |
|------|------|
| Radius | 圆形半径（像素，≥ 0） |
| Rotation | 起始旋转角度（度），支持任意整数，自动归一化到 `[0, 360)` |
| Clock Wise | 是否顺时针排列 |

---

## LayoutElement 兼容性

`AutoLayout` 完整尊重子元素上挂载的 `LayoutElement` 组件声明的尺寸契约：

- **minWidth / minHeight**：硬约束，元素实际尺寸不会小于此值
- **preferredWidth / preferredHeight**：软约束，优先满足期望尺寸
- **flexibleWidth / flexibleHeight**：如实上报给父布局系统，不干预
- **ignoreLayout = true**：正确跳过，该元素不参与布局计算

`LayoutElement` 属性在 Inspector 中的修改会实时触发 `AutoLayout` 重新计算，行为与官方 `HorizontalLayoutGroup` / `VerticalLayoutGroup` 一致。

`ContentSizeFitter` 挂在子元素上时可正常配合使用——它会在布局前将子元素尺寸设为 preferred 值，`AutoLayout` 会正确读取并使用。

---

## ~~WaveLayout~~（已弃用）

> 已弃用。请改用 `AutoLayout` 的 **Interpolated** 模式，启用 **Constrain By Group** 并将 `Post Wrap Mode` 设为 `Loop`，可实现相同效果且灵活性更强。

## 许可证

本项目采用 [MIT](../LICENSE) 许可证。