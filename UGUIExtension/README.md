# UGUI 扩展组件

> 内容由 AI 根据核心代码生成，已通过人工审核。

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![UGUI](https://img.shields.io/badge/UGUI-Compatible-brightgreen.svg)](https://docs.unity3d.com/Manual/UISystem.html)

## 扩展组件列表

- [ExtendDropdown](./ExtendDropdown/README.md)  
    一个基于 UGUI 原生 `Dropdown` 控件的功能扩展组件。它在保持与原生组件完全兼容的基础上，提供了**对象池复用**、**手动初始化**、**菜单项扩展**、**回池生命周期回调**等核心优化能力，有效解决了原生 Dropdown 在高频使用场景下的性能瓶颈问题。

- [ButtonLongPress]()  
    为 UGUI Button 增加长按交互能力，支持长按开始、长按持续触发、按下/抬起等事件。采用 `Update` 轮询计时器实现，零 GC 分配，使用 `Time.unscaledTime` 计时，不受 `Time.timeScale` 影响。

- [DraggableItem](./DraggableItem/README.md)  
    拖拽组件的抽象基类，定义了拖拽生命周期（`IBeginDragHandler` / `IDragHandler` / `IEndDragHandler`）与事件系统。子类只需实现 `CaptureCurrentPosition()` 和 `UpdatePosition()` 即可完成不同坐标系下的拖拽逻辑，避免重复代码。

- [MultiScrollRectDrag]()  
    处理嵌套/重叠 ScrollRect 拖拽冲突的组件。通过检测拖拽方向与角度阈值，决定拖拽事件应由当前 ScrollRect 处理还是转发给重叠的 ScrollRect，解决了两个 ScrollRect 重叠时滚动行为混乱的问题。

- [RectTransformSender]()  
    RectTransform 状态发射器，实时侦听并向外发射 `AnchoredPosition` 与 `Rect` 的变化事件。采用按需轮询策略，仅在存在有效事件监听者时激活 `LateUpdate` 监测，减少无用 CPU 开销。支持 `ExecuteAlways`，在 Editor 模式下也可工作。

- [RectAnchorUtility]()  
    RectTransform 锚点布局工具类，提供 `anchorMin` / `anchorMax` 与 `AnchorMode` 枚举之间的双向转换与快速设置。支持 14 种常用锚点布局模式（中心、左中、右上、四角填充等），零 GC 分配。

## 📄 许可证

本项目采用 [MIT License](../LICENSE) 开源许可证。