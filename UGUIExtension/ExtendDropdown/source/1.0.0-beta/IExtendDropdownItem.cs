using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 扩展菜单项接口
/// </summary>
public interface IExtendDropdownItem
{
    /// <summary>
    /// 菜单项 Text 组件
    /// </summary>
    public Text Text { get; set; }

    /// <summary>
    /// 菜单项 RectTransform 组件
    /// </summary>
    public RectTransform RectTransform { get; set; }

    /// <summary>
    /// 菜单项 Image 组件
    /// </summary>
    public Image Image { get; set; }

    /// <summary>
    /// 菜单项 Toggle 组件
    /// </summary>
    public Toggle Toggle { get; set; }

    /// <summary>
    /// 获取组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public T GetComponent<T>() where T : Component;

    /// <summary>
    /// 添加组件
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    public T AddComponent<T>() where T : Component;
}