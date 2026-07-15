# API 文档

> 内容由 AI 根据核心代码生成，已通过人工审核。

---

## 公共 API 简介

### 类：`ExtendDropdown`

继承自 `UnityEngine.UI.Dropdown`，提供以下公共成员。

---

### 属性

#### `ManualInitialize`
```csharp
public bool ManualInitialize { get; }
```

**说明**：是否启用手动初始化模式。该值在 Inspector 面板中设置，运行时只读。

**用途**：当需要在 Awake/Start 阶段阻止组件自动初始化时，设置为 `true`，之后通过 `Initialize()` 方法手动触发初始化。

---

#### `ManualInitializeFinished`
```csharp
public bool ManualInitializeFinished { get; }
```

**说明**：手动初始化是否已完成。

**用途**：用于检查手动初始化是否已调用并完成，避免重复初始化。

---

#### `ReuseDropdownList`
```csharp
public bool ReuseDropdownList { get; set; }
```

**说明**：是否复用下拉列表对象。启用后，下拉菜单列表在关闭时不会被销毁，而是隐藏后复用，减少 Instantiate/Destroy 开销。

**注意**：该功能目前处于测试阶段。

---

#### `PoolingItems`
```csharp
public bool PoolingItems { get; set; }
```

**说明**：是否池化菜单项对象。启用后，菜单项在关闭时会回收到对象池而非销毁，下次打开时从池中取出复用，显著降低 GC 压力。

**注意**：该功能目前处于测试阶段。

---

### 方法

#### `Initialize`
```csharp
public void Initialize(
    Action<GameObject> onSetDropdownTemplate = null,
    Action<IExtendDropdownItem> onSetDropdownItemTemplate = null,
    Action<IExtendDropdownItem> onCreateDropdownItem = null,
    Action<List<OptionData>> onDropdownShown = null,
    Action onDropdownListDestroy = null,
    Action onBlockerDestroy = null,
    Action<IExtendDropdownItem> onReleaseDropdownItem = null
)
```

**说明**：手动初始化组件并注册生命周期回调。仅在 `ManualInitialize` 为 `true` 且尚未完成初始化时有效。

**参数**：

| 参数 | 类型 | 说明 |
|------|------|------|
| `onSetDropdownTemplate` | `Action<GameObject>` | 设置下拉菜单模板时调用（仅首次创建列表时触发） |
| `onSetDropdownItemTemplate` | `Action<IExtendDropdownItem>` | 设置菜单项模板时调用（仅首次创建列表时触发） |
| `onCreateDropdownItem` | `Action<IExtendDropdownItem>` | 每次创建/取出菜单项时调用 |
| `onDropdownShown` | `Action<List<OptionData>>` | 每次下拉菜单显示前调用，可在此动态修改选项列表 |
| `onDropdownListDestroy` | `Action` | 下拉菜单列表销毁时调用（列表复用模式下为隐藏时） |
| `onBlockerDestroy` | `Action` | 下拉菜单阻挡器销毁时调用 |
| `onReleaseDropdownItem` | `Action<IExtendDropdownItem>` | 菜单项回池时调用，用于清理状态或移除动态组件 |

**注意**：`onReleaseDropdownItem` 仅在 `PoolingItems` 为 `true` 时生效，用于在回池时执行清理逻辑。

---

#### `ClearItemsPool`
```csharp
public void ClearItemsPool()
```

**说明**：清空菜单项对象池，销毁池中所有缓存的菜单项对象。

**用途**：场景切换或组件销毁时主动释放池中对象，防止内存残留。

---

### 接口：`IExtendDropdownItem`

表示扩展的下拉菜单项，提供对菜单项各组件和通用组件操作的访问。

#### 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Text` | `Text` | 菜单项的 Text 组件 |
| `RectTransform` | `RectTransform` | 菜单项的 RectTransform 组件 |
| `Image` | `Image` | 菜单项的 Image 组件 |
| `Toggle` | `Toggle` | 菜单项的 Toggle 组件 |

#### 方法

##### `GetComponent<T>`
```csharp
T GetComponent<T>() where T : Component
```

**说明**：从菜单项 GameObject 上获取指定类型的组件。

##### `AddComponent<T>`
```csharp
T AddComponent<T>() where T : Component
```

**说明**：为菜单项 GameObject 动态添加指定类型的组件。

**注意**：在池化复用场景下，动态添加的组件需要在 `onReleaseDropdownItem` 回调中主动移除，避免状态污染。

---

## 回调调用顺序

### 首次打开下拉菜单

```
onDropdownShown（显示前）
    ↓
onSetDropdownTemplate（设置列表模板）
    ↓
onSetDropdownItemTemplate（设置菜单项模板）
    ↓
对每个选项依次调用：
    ↓
onCreateDropdownItem（创建菜单项）
    ↓
onCreateDropdownItem（创建菜单项）
    ↓
...
    ↓
（用户选择或点击外部关闭）
    ↓
onBlockerDestroy（阻挡器销毁）
    ↓
onDropdownListDestroy（列表销毁/隐藏）
```

### 第二次及后续打开（池化开启）

```
onDropdownShown（显示前）
    ↓
对每个选项依次调用：
    ↓
onCreateDropdownItem（从池中取出/新创建）
    ↓
...
    ↓
（用户选择或点击外部关闭）
    ↓
onBlockerDestroy（阻挡器销毁）
    ↓
对每个菜单项依次调用：
    ↓
onReleaseDropdownItem（回池清理）  ← 新增
    ↓
onDropdownListDestroy（列表销毁/隐藏）
```

---

## 使用示例

### 完整生命周期回调注册

```csharp
using UnityEngine;
using UnityEngine.UI;
using UIAssistant.Core.Elements;

public class FullLifecycleDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Start()
    {
        // 开启池化
        dropdown.PoolingItems = true;
        dropdown.ReuseDropdownList = true;

        // 手动初始化并注册所有回调
        dropdown.Initialize(
            onSetDropdownTemplate: template =>
            {
                // 自定义下拉列表背景
                template.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f);
                Debug.Log("1. 设置下拉列表模板");
            },
            onSetDropdownItemTemplate: item =>
            {
                // 为菜单项添加额外组件（在模板上只添加一次）
                var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(item.RectTransform, false);
                Debug.Log("2. 设置菜单项模板");
            },
            onCreateDropdownItem: item =>
            {
                // 每个菜单项创建/取出时调用
                var icon = item.GetComponentInChildren<Image>();
                int index = item.transform.GetSiblingIndex();
                if (icon != null)
                {
                    icon.sprite = icons[index % icons.Length];
                }
                Debug.Log($"3. 创建菜单项 #{index}");
            },
            onDropdownShown: options =>
            {
                // 显示前动态更新数据
                options.Clear();
                options.AddRange(GetLatestData());
                Debug.Log("0. 下拉菜单显示前（最先调用）");
            },
            onReleaseDropdownItem: item =>
            {
                // 回池时清理
                var icon = item.GetComponentInChildren<Image>();
                if (icon != null) icon.sprite = null;
                Debug.Log("4. 菜单项回池清理");
            },
            beforeDropdownListDestroy: () =>
            {
                Debug.Log("5. 下拉列表即将关闭");
            },
            onBlockerDestroy: () =>
            {
                Debug.Log("6. 阻挡器已销毁");
            }
        );

        dropdown.AddOptions(new[] { "选项A", "选项B", "选项C" });
    }

    private string[] GetLatestData() => new[] { "数据1", "数据2", "数据3" };
    private Sprite[] icons;
}
```

### 动态添加组件的安全清理

```csharp
public class SafeAddComponentDemo : MonoBehaviour
{
    [SerializeField] private ExtendDropdown dropdown;

    private void Start()
    {
        dropdown.PoolingItems = true;

        dropdown.Initialize(
            onCreateDropdownItem: item =>
            {
                // 为特定菜单项动态添加处理组件
                var index = item.transform.GetSiblingIndex();
                if (index == 0) // 第一个选项特殊处理
                {
                    var handler = item.AddComponent<SpecialHandler>();
                    handler.Setup();
                }
            },
            onReleaseDropdownItem: item =>
            {
                // 回池时移除动态添加的组件
                var handler = item.GetComponent<SpecialHandler>();
                if (handler != null)
                {
                    handler.Cleanup();
                    Destroy(handler);
                }
            }
        );
    }
}
```