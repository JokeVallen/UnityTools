> 内容由 AI 根据核心代码生成，已通过人工审核。

# DraggableItem

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com)

Unity 拖拽交互组件库，提供 UI ↔ UI、World ↔ UI、UI ↔ World 三种拖拽场景的统一解决方案。基于 Unity EventSystem 构建，采用模板方法模式实现坐标更新算法的灵活扩展。

## 安装环境要求

- **Unity 版本**：2020.3 LTS 或更高
- **输入系统**：Unity Legacy Input Manager（EventSystem 默认），未依赖 Input System Package

## 安装方式

### 源码导入
直接将 [DraggableItem](../DraggableItem/) 目录下的所有 `.cs` 文件复制到项目的任意 `Scripts` 目录中即可。

## 设计理念

- **模板方法模式**：`BaseDraggableItem<TPosition>` 抽象基类定义了拖拽生命周期（OnBeginDrag / OnDrag / OnEndDrag）和事件流，子类只需实现 `CaptureCurrentPosition()` 和 `UpdatePosition()` 两个抽象方法即可完成不同坐标系的适配。
- **泛型坐标抽象**：通过 `TPosition` 泛型参数支持 `Vector2`（UI 局部坐标）和 `Vector3`（世界坐标）两种位置类型，在编译期保证类型安全。
- **相机自动回退**：当未手动指定参考相机时，自动从 `PointerEventData.pressEventCamera` 获取触发事件的相机，降低使用门槛。
- **事件驱动扩展**：提供 `DraggableItemEvent` 序列化事件，支持在 Inspector 中直接绑定拖拽开始/进行中/结束的回调。

## 具体功能说明

| 组件 | 适用场景 | 坐标类型 | 位置更新策略 |
|------|----------|----------|--------------|
| **UI2UIDraggableItem** | UI 元素在 UGUI 容器内部拖拽 | `Vector2`（anchoredPosition） | 将屏幕坐标转换为父 RectTransform 局部坐标，适用于 ScrollView、Panel 等容器内拖拽排序。 |
| **UI2WorldDraggableItem** | UI 元素拖拽到 3D 世界空间 | `Vector3`（世界坐标） | 通过 `ScreenPointToWorldPointInRectangle` 将屏幕点投射到世界空间，适用于 UI 映射到 3D 物体的场景。 |
| **World2UIDraggableItem** | 3D 物理物体跟随鼠标拖拽 | `Vector3`（世界坐标） | 将鼠标屏幕坐标转换为世界坐标，带有 Z 轴偏移保留，适用于 3D 场景中物体的拖拽移动。 |

所有组件均内置：
- **拖拽事件回调**：Inspector 面板可视化绑定
- **启用/禁用控制**：`isActiveAndEnabled` 生效检查
- **初始位置备份**：通过 `OriginalPosition` 属性获取拖拽起始位置

## 许可证

本项目采用 [MIT License](../../LICENSE) 授权，允许自由使用、修改、分发，包括商业用途。请保留原始版权声明。