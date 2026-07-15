# ExtendDropdown

> 内容由 AI 根据核心代码生成，已通过人工审核。

[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Unity 2020.3+](https://img.shields.io/badge/Unity-2020.3%2B-blueviolet.svg)](https://unity.com)
[![UGUI](https://img.shields.io/badge/UGUI-Compatible-brightgreen.svg)](https://docs.unity3d.com/Manual/UISystem.html)

## 📖 简介

**ExtendDropdown** 是一个基于 Unity UGUI 原生 `Dropdown` 控件的功能扩展组件。它在保持与原生组件完全兼容的基础上，提供了**对象池复用**、**手动初始化**、**菜单项扩展**、**回池生命周期回调**等核心优化能力，有效解决了原生 Dropdown 在高频使用场景下的性能瓶颈问题。

主要特性：
- ✅ **菜单项池化** — 避免频繁创建销毁，大幅降低 GC 压力
- ✅ **下拉列表复用** — 可选复用列表对象，减少实例化开销
- ✅ **手动初始化** — 支持延迟初始化，灵活控制组件生命周期
- ✅ **菜单项扩展** — 通过 `IExtendDropdownItem` 接口自由扩展菜单项组件
- ✅ **双向生命周期回调** — 覆盖创建（`onCreateDropdownItem`）和回池（`onReleaseDropdownItem`）全流程，支持成对的资源分配与清理

## 🎯 适用场景

- 选项数量较多（10+ 项）的下拉菜单
- 频繁打开/关闭的下拉菜单（如游戏内设置、聊天频道切换）
- 需要动态更新选项列表的场景
- 需要对菜单项进行自定义 UI 扩展的项目
- 需要在菜单项回池时执行清理逻辑（如销毁动态添加的组件）的项目

## ⚙️ 环境要求

- **Unity 版本**：2020.3 或更高
- **UI 系统**：UGUI
- **依赖**：无外部依赖

## 📦 安装

### 方式一：源码导入

直接将 [ExtendDropdown](../ExtendDropdown/) 目录放入项目的 `Scripts` 目录下即可。

## 🚀 快速开始

### 基础用法

1. 在场景中创建或找到 `Canvas` 对象
2. 右键点击 `Canvas` → `UI` → `Extend` → `Dropdown`，自动创建 ExtendDropdown 实例
3. 在 Inspector 中配置选项数据，与原生 Dropdown 操作完全一致

### 启用性能优化

在 ExtendDropdown 组件的 Inspector 中勾选：
- **Pooling Items** — 开启菜单项对象池
- **Reuse Dropdown List** — 开启下拉列表复用

## ExtendDropdown 使用示例

下面从简单到高级，提供几个实际可用的代码示例。

---

### 示例一：基础用法（替换原生 Dropdown）

**使用场景**：在现有项目中用 ExtendDropdown 直接替换原生 Dropdown，无需改动代码。

```csharp
using UnityEngine;
using UIAssistant.Core.Elements;

public class SimpleDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Start()
    {
        // 用法与原生 Dropdown 完全一致
        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "选项一",
            "选项二",
            "选项三",
            "选项四"
        });

        // 监听选中事件
        dropdown.onValueChanged.AddListener(index =>
        {
            Debug.Log($"选中了第 {index + 1} 项：{dropdown.options[index].text}");
        });
    }
}
```

---

### 示例二：开启性能优化（池化 + 列表复用）

**使用场景**：下拉菜单有 20+ 个选项，且玩家会频繁点击打开/关闭。

```csharp
public class OptimizedDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Start()
    {
        // 在 Inspector 中勾选 PoolingItems + ReuseDropdownList
        // 或运行时开启（两者等效）
        dropdown.PoolingItems = true;
        dropdown.ReuseDropdownList = true;

        // 添加大量选项（模拟城市列表、服务器列表等）
        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 50; i++)
        {
            options.Add($"城市_{i:00}");
        }
        dropdown.AddOptions(options);

        // 正常使用即可，内部自动池化
        dropdown.onValueChanged.AddListener(OnCitySelected);
    }

    private void OnCitySelected(int index)
    {
        Debug.Log($"选择了城市：{dropdown.options[index].text}");
    }
}
```

**效果**：第一次打开正常新建，之后每次打开都从池中取用，关闭时回池，GC 归零。

---

### 示例三：动态更新选项列表（beforeDropdownShown 回调）

**使用场景**：聊天频道的在线用户列表、游戏服务器状态列表，内容实时变化，需要在打开下拉菜单时刷新。

```csharp
public class DynamicDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    // 模拟实时数据源
    private List<string> onlinePlayers = new List<string> { "玩家A", "玩家B", "玩家C" };

    private void Start()
    {
        // 手动初始化，注入显示前回调
        dropdown.Initialize(
            onDropdownShown: options =>
            {
                // 每次下拉菜单显示前动态更新选项
                options.Clear();

                // 模拟从服务器获取最新在线玩家列表
                RefreshPlayerList();

                foreach (var player in onlinePlayers)
                {
                    options.Add(new UnityEngine.UI.Dropdown.OptionData(player));
                }

                Debug.Log($"已更新选项列表，共 {options.Count} 人");
            }
        );

        // 开启手动初始化后，组件不会自动初始化，需要调用 Initialize 后才可用
        // 如果不需要手动初始化，也可以直接在 Inspector 中取消勾选 ManualInitialize
    }

    // 模拟刷新玩家列表
    private void RefreshPlayerList()
    {
        // 实际项目中这里会调用网络请求
        // 这里随机增减几个玩家模拟变化
        if (Random.value > 0.5f)
        {
            onlinePlayers.Add($"玩家_{Random.Range(100, 999)}");
        }
        else if (onlinePlayers.Count > 3)
        {
            onlinePlayers.RemoveAt(onlinePlayers.Count - 1);
        }
    }
}
```

---

### 示例四：菜单项自定义扩展（添加图标）

**使用场景**：菜单项需要显示图标 + 文字，原生 Dropdown 不支持。

```csharp
using UnityEngine;
using UnityEngine.UI;
using UIAssistant.Core.Elements;

public class IconDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;
    [SerializeField] private Sprite[] itemIcons; // 预设图标

    private void Start()
    {
        dropdown.Initialize(
            // 设置菜单项模板：为每个菜单项添加一个额外的 Image 显示图标
            onSetDropdownItemTemplate: item =>
            {
                // 为菜单项添加额外图标（原生只有 text 和 toggle，没有额外 Image）
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(item.RectTransform, false);

                // 设置图标位置（左边）
                var rt = iconGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0.5f);
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.sizeDelta = new Vector2(30, 30);
                rt.anchoredPosition = new Vector2(5, 0);

                // 存储图标引用到 item 的额外数据中（具体实现可根据需要扩展）
                // 这里通过 GetComponent 获取图标组件
            },
            onCreateDropdownItem: item =>
            {
                // 每次创建菜单项时设置对应的图标
                // 假设通过 item 的索引或数据来决定显示哪个图标
                var icon = item.GetComponentInChildren<Image>();
                if (icon != null)
                {
                    // 示例：根据菜单项索引显示不同图标
                    int index = item.transform.GetSiblingIndex();
                    if (index < itemIcons.Length)
                    {
                        icon.sprite = itemIcons[index];
                        icon.enabled = true;
                    }
                }
            }
        );
    }
}
```

---

### 示例五：手动初始化 + 延迟加载

**使用场景**：游戏启动时，UI 尚未完全加载，但需要预先创建下拉菜单组件；或者希望统一管理所有 UI 组件的初始化时机。

```csharp
public class DelayedDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Awake()
    {
        // 在 Inspector 中勾选 ManualInitialize
        // 此时组件不会在 Awake/Start 中初始化，交互被禁用（灰色不可点击）
        // 适合在场景加载完成后统一初始化
    }

    private IEnumerator Start()
    {
        // 模拟等待资源加载、网络数据返回等
        yield return new WaitForSeconds(2f);

        // 手动完成初始化
        dropdown.Initialize(
            onSetDropdownTemplate: template =>
            {
                // 可以在这里对下拉菜单模板做自定义处理
                Debug.Log("下拉菜单模板已设置");
            },
            onSetDropdownItemTemplate: item =>
            {
                // 可以在这里对菜单项模板做自定义处理
                Debug.Log("菜单项模板已设置");
            }
        );

        // 初始化完成后组件自动变为可交互
        dropdown.ClearOptions();
        dropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "延迟加载的选项一",
            "延迟加载的选项二"
        });

        Debug.Log("下拉菜单已准备就绪");
    }
}
```

---

### 示例六：清空对象池（场景切换/资源释放）

**使用场景**：切换场景或卸载 UI 时，需要彻底释放池中所有对象，避免内存残留。

```csharp
public class PoolCleanupDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void OnDestroy()
    {
        // 场景卸载前清空对象池，释放所有池中对象
        dropdown.ClearItemsPool();
        Debug.Log("对象池已清空");
    }

    // 或者手动调用
    public void ForceCleanupPool()
    {
        dropdown.ClearItemsPool();
    }
}
```

---

### 示例七：综合应用（完整流程）

一个完整的使用案例，涵盖开启池化、动态更新、自定义图标、监听事件全部功能：

```csharp
public class CompleteDropdownDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;
    [SerializeField] private Sprite[] icons;

    private List<string> dataSource = new List<string>();

    private void Start()
    {
        // 1. 开启性能优化
        dropdown.PoolingItems = true;
        dropdown.ReuseDropdownList = true;

        // 2. 手动初始化 + 注册所有回调
        dropdown.Initialize(
            onSetDropdownTemplate: template =>
            {
                // 自定义下拉列表背景样式
                var bg = template.GetComponent<Image>();
                if (bg != null) bg.color = new Color(0.1f, 0.1f, 0.2f);
            },
            onSetDropdownItemTemplate: item =>
            {
                // 为每个菜单项添加一个图标占位
                var iconGo = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(item.RectTransform, false);
                // ... 布局设置
            },
            onCreateDropdownItem: item =>
            {
                // 设置图标
                var icon = item.GetComponentInChildren<Image>();
                int index = item.transform.GetSiblingIndex();
                if (icon != null && index < icons.Length)
                {
                    icon.sprite = icons[index];
                }
            },
            onDropdownShown: options =>
            {
                // 显示前刷新数据
                options.Clear();
                dataSource = FetchDataFromServer();
                foreach (var data in dataSource)
                {
                    options.Add(new UnityEngine.UI.Dropdown.OptionData(data));
                }
            },
            beforeDropdownListDestroy: () => Debug.Log("下拉列表即将关闭"),
            onBlockerDestroy: () => Debug.Log("阻挡器已销毁")
        );

        // 3. 监听用户选择
        dropdown.onValueChanged.AddListener(OnItemSelected);
    }

    private List<string> FetchDataFromServer()
    {
        // 模拟网络请求，实际项目中使用异步加载
        return new List<string> { "数据A", "数据B", "数据C", "数据D" };
    }

    private void OnItemSelected(int index)
    {
        Debug.Log($"选中：{dropdown.options[index].text}");
    }

    private void OnDestroy()
    {
        // 清理资源
        dropdown.ClearItemsPool();
    }
}
```

---

### 示例八：回池清理（onReleaseDropdownItem）

**使用场景**：池化复用场景下，动态添加了组件或修改了菜单项状态，需要在回池时进行清理，避免状态污染。

```csharp
public class ReleaseCleanupDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Start()
    {
        dropdown.PoolingItems = true;

        dropdown.Initialize(
            onCreateDropdownItem: item =>
            {
                // 创建时动态添加组件
                if (NeedSpecialHandler(item))
                {
                    var handler = item.AddComponent<SpecialItemHandler>();
                    handler.Initialize(GetDataForItem(item));
                }
            },
            onReleaseDropdownItem: item =>
            {
                // ✅ 回池时清理：移除动态添加的组件，重置状态
                var handler = item.GetComponent<SpecialItemHandler>();
                if (handler != null)
                {
                    handler.Dispose();
                    Destroy(handler);
                }

                // 重置 UI 状态
                item.Text.color = Color.white;
                item.Image.sprite = null;
            }
        );
    }

    private bool NeedSpecialHandler(IExtendDropdownItem item) { /* ... */ }
    private object GetDataForItem(IExtendDropdownItem item) { /* ... */ }
}
```

**为什么需要这个回调？**

池化复用场景下，`onCreateDropdownItem` 和 `onReleaseDropdownItem` 成对出现，确保资源分配与清理对称：

| 阶段 | 回调 | 职责 |
|------|------|------|
| 创建/取出 | `onCreateDropdownItem` | 分配资源、设置数据、附加效果 |
| 回池 | `onReleaseDropdownItem` | 释放资源、移除动态组件、重置状态 |

没有 `onReleaseDropdownItem` 时，动态添加的组件会在多次复用后累积，导致状态污染和内存泄漏。有了它，回池时就能彻底清理，保证下一个使用者拿到的是干净的对象。

---

### 使用建议总结

| 场景 | 推荐配置 |
|------|----------|
| 简单下拉（< 5 项，不常打开） | 默认配置，不开池化 |
| 常规下拉（5-15 项） | 开启 `PoolingItems` |
| 大型下拉（> 15 项） | 开启 `PoolingItems` + `ReuseDropdownList` |
| 动态内容下拉 | 使用 `beforeDropdownShown` 回调更新 |
| 带自定义 UI 的下拉 | 使用 `onCreateDropdownItem` 扩展菜单项 |
| 需要控制初始化时机 | 开启 `ManualInitialize` + 调用 `Initialize()` |
| 菜单项动态添加组件 | 配合 `onReleaseDropdownItem` 成对清理 |

## 其它文档

[API 文档](./source/1.0.0-beta/DOCUMENT.md)

## 📄 许可证

本项目采用 [MIT License](../../LICENSE) 开源许可证。