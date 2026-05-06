> 内容由 AI 根据核心代码生成，已通过人工审核。

# EditorObjectFieldUtility API 文档

## 公共 API 简介

`EditorObjectFieldUtility` 是一组用于绘制增强型 `ObjectField` 的静态方法。所有方法仅可在 `#if UNITY_EDITOR` 环境中使用。主要提供以下三种能力：
- 只读对象字段（不可拖拽赋值，仅展示与定位）
- 无选择器按钮的对象字段（隐藏右侧小圆点选择器）
- 自定义选择器按钮行为的对象字段（可注入回调）

每个能力均提供 **Rect** 版本（配合 `EditorGUI`）与 **Layout** 版本（配合 `EditorGUILayout`），并同时支持泛型和非泛型重载。

---

### 只读对象字段

#### `ReadOnlyObjectField(Rect, GUIContent, T)`
```csharp
public static void ReadOnlyObjectField<T>(Rect rect, GUIContent label, T value) where T : UnityEngine.Object
```
- **作用**：在指定矩形区域绘制只读对象字段，不接受拖拽赋值，仅用于展示。单击字段会 Ping 对应对象。
- **泛型参数** `T`：对象类型。

#### `ReadOnlyObjectField(Rect, GUIContent, Object, Type)`
```csharp
public static void ReadOnlyObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType)
```
- **作用**：上述方法的非泛型版本，需显式指定 `objectType`。

#### `ReadOnlyObjectFieldLayout(GUIContent, T, GUILayoutOption[])`
```csharp
public static void ReadOnlyObjectFieldLayout<T>(GUIContent label, T value, params GUILayoutOption[] options) where T : UnityEngine.Object
```
- **作用**：自动布局版本的只读对象字段。

#### `ReadOnlyObjectFieldLayout(GUIContent, Object, Type, GUILayoutOption[])`
```csharp
public static void ReadOnlyObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, params GUILayoutOption[] options)
```
- **作用**：自动布局版本的非泛型只读对象字段。

---

### 无选择器按钮的对象字段

#### `NoPickerObjectField<T>(Rect, GUIContent, T, bool)`
```csharp
public static T NoPickerObjectField<T>(Rect rect, GUIContent label, T value, bool allowSceneObject = true) where T : UnityEngine.Object
```
- **作用**：绘制可拖拽赋值的对象字段，但右侧的圆形选择器按钮被移除。支持拖拽验证和类型过滤。
- **参数**：
  - `allowSceneObject`：是否允许接受场景中的对象（默认 `true`）。
- **返回值**：拖拽赋值后的对象（类型不匹配时返回 `null`）。

#### `NoPickerObjectField(Rect, GUIContent, Object, Type, bool)`
```csharp
public static UnityEngine.Object NoPickerObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true)
```
- **作用**：非泛型版本的无选择器对象字段。

#### `NoPickerObjectFieldLayout<T>(GUIContent, T, bool, GUILayoutOption[])`
```csharp
public static T NoPickerObjectFieldLayout<T>(GUIContent label, T value, bool allowSceneObject = true, params GUILayoutOption[] options) where T : UnityEngine.Object
```
- **作用**：自动布局版本的无选择器对象字段。

#### `NoPickerObjectFieldLayout(GUIContent, Object, Type, bool, GUILayoutOption[])`
```csharp
public static UnityEngine.Object NoPickerObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, params GUILayoutOption[] options)
```
- **作用**：自动布局版本的非泛型无选择器对象字段。

---

### 自定义选择器按钮的对象字段

#### `ObjectField<T>(Rect, GUIContent, T, bool, Action<T>)`
```csharp
public static T ObjectField<T>(Rect rect, GUIContent label, T value, bool allowSceneObject = true, Action<T> onPickerClick = null) where T : UnityEngine.Object
```
- **作用**：绘制标准的对象字段，但右侧选择器按钮的点击行为由 `onPickerClick` 回调接管，不会打开原生选择器窗口。
- **参数**：
  - `onPickerClick`：可选的回调，提供当前字段的值。
- **返回值**：赋值后的对象。

#### `ObjectField(Rect, GUIContent, Object, Type, bool, Action<Object>)`
```csharp
public static UnityEngine.Object ObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, Action<UnityEngine.Object> onPickerClick = null)
```
- **作用**：非泛型版本的自定义选择器对象字段。

#### `ObjectFieldLayout<T>(GUIContent, T, bool, Action<T>, GUILayoutOption[])`
```csharp
public static T ObjectFieldLayout<T>(GUIContent label, T value, bool allowSceneObject = true, Action<T> onPickerClick = null, params GUILayoutOption[] options) where T : UnityEngine.Object
```
- **作用**：自动布局版本的自定义选择器对象字段。

#### `ObjectFieldLayout(GUIContent, Object, Type, bool, Action<Object>, GUILayoutOption[])`
```csharp
public static UnityEngine.Object ObjectFieldLayout(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObject = true, Action<UnityEngine.Object> onPickerClick = null, params GUILayoutOption[] options)
```
- **作用**：自动布局版本的非泛型自定义选择器对象字段。

---

## 使用示例

以下示例节选自 `ObjectFieldTestWindow.cs`：

```csharp
private Texture2D myTexture;

private void OnGUI()
{
    // 1. 只读对象字段（Layout）
    EditorObjectFieldUtility.ReadOnlyObjectFieldLayout(
        new GUIContent("ReadOnly Texture"), myTexture);

    // 2. 无选择器按钮的对象字段（Rect），且拒绝场景对象
    Rect r1 = GUILayoutUtility.GetRect(200, EditorGUIUtility.singleLineHeight);
    myTexture = EditorObjectFieldUtility.NoPickerObjectField(
        r1, new GUIContent("No Picker"), myTexture, allowSceneObject: false) as Texture2D;

    // 3. 带自定义选择器行为的对象字段（Layout）
    myTexture = EditorObjectFieldUtility.ObjectFieldLayout<Texture2D>(
        new GUIContent("Custom Picker"),
        myTexture,
        allowSceneObject: false,
        onPickerClick: tex => Debug.Log($"Clicked picker! Current value: {tex?.name}")
    );
}
```

所有字段均支持：
- **拖拽赋值**：从 Project 或 Hierarchy 拖拽对象到字段。
- **单击定位**：单击已赋值的字段可在 Project/Hierarchy 中高亮对象。
- **类型过滤**：类型不匹配的对象会被自动拒绝，并返回 `null`。