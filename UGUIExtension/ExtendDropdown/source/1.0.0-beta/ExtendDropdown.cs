using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[AddComponentMenu("UIExtension/UGUI/ExtendDropdown", 35)]
/// <summary>
/// 扩展下拉菜单
/// </summary>
public class ExtendDropdown : Dropdown
{
    protected class ExtendDropdownItem : DropdownItem, IExtendDropdownItem
    {
        public Text Text { get => text; set => text = value; }
        public RectTransform RectTransform { get => rectTransform; set => rectTransform = value; }
        public Image Image { get => image; set => image = value; }
        public Toggle Toggle { get => toggle; set => toggle = value; }

        public new T GetComponent<T>() where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : Component
        {
            return gameObject.AddComponent<T>();
        }
    }

    [Header("Extend")]
    [Tooltip("Initialize drop-down manually."), SerializeField] private bool m_ManualInitialize;
    [Tooltip("Reuse the drop-down list."), SerializeField] private bool m_ReuseDropdownList;
    [Tooltip("Pooling drop-down menu item."), SerializeField] private bool m_PoolingItems;

    private Action<GameObject> m_OnSetDropdownTemplate;
    private Action<IExtendDropdownItem> m_OnSetDropdownItemTemplate;
    private Action<IExtendDropdownItem> m_OnCreateDropdownItem;
    private Action<List<OptionData>> m_BeforeDropdownShown;
    private Action m_OnDropdownListDestroy;
    private Action m_OnBlockerDestroy;
    private Action<IExtendDropdownItem> m_OnReleaseDropdownItem;

    private bool m_IsManualInitializeFinished;
    private bool m_IsModifiedItemTemplate;
    private bool m_AwakeFinished;
    private bool m_StartFinished;

    private GameObject m_DropdownList;
    private GameObject m_ItemsContainer;
    private Stack<ExtendDropdownItem> m_ItemsPool;
    private Vector2 m_ItemTemplateContentSizeDelta_Backup;

    /// <summary>
    /// 是否开启了手动初始化
    /// </summary>
    public bool ManualInitialize
    {
        get
        {
            return m_ManualInitialize;
        }
    }

    /// <summary>
    /// 手动初始化是否完成
    /// </summary>
    public bool ManualInitializeFinished
    {
        get
        {
            return m_IsManualInitializeFinished;
        }
    }

    /// <summary>
    /// 是否复用下拉菜单列表
    /// </summary>
    public bool ReuseDropdownList
    {
        get
        {
            return m_ReuseDropdownList;
        }
        set
        {
            m_ReuseDropdownList = value;
        }
    }

    /// <summary>
    /// 是否池化下拉菜单项
    /// </summary>
    public bool PoolingItems
    {
        get
        {
            return m_PoolingItems;
        }
        set
        {
            m_PoolingItems = value;
        }
    }

    /// <summary>
    /// 初始化下拉菜单
    /// </summary>
    /// <param name="onSetDropdownTemplate">设置下拉菜单模板回调</param>
    /// <param name="onSetDropdownItemTemplate">设置下拉菜单项模板回调</param>
    /// <param name="onCreateDropdownItem">创建下拉菜单项回调</param>
    /// <param name="onDropdownShown">下拉菜单显示回调</param>
    /// <param name="beforeDropdownListDestroy">下拉菜单销毁回调</param>
    /// <param name="onBlockerDestroy">下拉菜单阻挡器销毁回调</param>
    /// <param name="onReleaseDropdownItem">回收下拉菜单项回调</param>
    public void Initialize(Action<GameObject> onSetDropdownTemplate = null,
    Action<IExtendDropdownItem> onSetDropdownItemTemplate = null,
    Action<IExtendDropdownItem> onCreateDropdownItem = null,
    Action<List<OptionData>> onDropdownShown = null,
    Action beforeDropdownListDestroy = null,
    Action onBlockerDestroy = null,
    Action<IExtendDropdownItem> onReleaseDropdownItem = null)
    {
        if (m_ManualInitialize && !m_IsManualInitializeFinished)
        {
            m_OnSetDropdownTemplate = onSetDropdownTemplate;
            m_OnSetDropdownItemTemplate = onSetDropdownItemTemplate;
            m_OnCreateDropdownItem = onCreateDropdownItem;
            m_BeforeDropdownShown = onDropdownShown;
            m_OnDropdownListDestroy = beforeDropdownListDestroy;
            m_OnBlockerDestroy = onBlockerDestroy;
            m_OnReleaseDropdownItem = onReleaseDropdownItem;
            if (!m_AwakeFinished) base.Awake();
            if (!m_StartFinished) base.Start();
            interactable = true;
            m_IsManualInitializeFinished = true;
        }
    }

    /// <summary>
    /// 清空下拉菜单项对象池
    /// </summary>
    public void ClearItemsPool()
    {
        if (m_ItemsPool == null || m_ItemsPool.Count == 0) return;

        ExtendDropdownItem item;
        while (m_ItemsPool.Count > 0)
        {
            item = m_ItemsPool.Pop();
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (ManualInitializeCheck())
        {
            if (m_BeforeDropdownShown != null)
            {
                m_BeforeDropdownShown(options);
            }
            base.OnPointerClick(eventData);
        }
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        if (ManualInitializeCheck())
        {
            if (m_BeforeDropdownShown != null)
            {
                m_BeforeDropdownShown(options);
            }
            base.OnSubmit(eventData);
        }
    }

    protected override void Awake()
    {
        if (ManualInitializeCheck())
        {
            base.Awake();
            m_AwakeFinished = true;
        }
        else
        {
            interactable = false;
        }
    }

    protected override void Start()
    {
        if (ManualInitializeCheck())
        {
            base.Start();
            m_StartFinished = true;
        }
        else
        {
            interactable = false;
        }
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        if (!m_IsModifiedItemTemplate)
        {
            if (m_OnSetDropdownTemplate != null) m_OnSetDropdownTemplate(template);
            var itemTemplate = template.GetComponentInChildren<DropdownItem>();
            ModifiedItemTemplate(itemTemplate);
            m_IsModifiedItemTemplate = true;
        }

        if (m_ReuseDropdownList)
        {
            if (m_DropdownList == null)
            {
                m_DropdownList = Instantiate(template);
            }
        }
        else
        {
            if (m_DropdownList != null)
            {
                DestroyImmediate(m_DropdownList);
            }
        }

        return m_ReuseDropdownList ? m_DropdownList : Instantiate(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        DropdownItem item;
        if (!m_PoolingItems)
        {
            item = Instantiate(itemTemplate);
        }
        else
        {
            item = GetDropdownItemFromPool();
        }

        if (m_OnCreateDropdownItem != null)
        {
            m_OnCreateDropdownItem.Invoke(item as ExtendDropdownItem);
        }

        return item;
    }

    protected override void DestroyBlocker(GameObject blocker)
    {
        base.DestroyBlocker(blocker);
        if (m_OnBlockerDestroy != null)
        {
            m_OnBlockerDestroy.Invoke();
        }
    }

    protected override void DestroyItem(DropdownItem item)
    {
        if (!m_PoolingItems)
        {
            base.DestroyItem(item);
        }
        else
        {
            ReleaseDropdownItemToPool(item as ExtendDropdownItem);
        }
    }

    protected override void DestroyDropdownList(GameObject dropdownList)
    {
        if (m_OnDropdownListDestroy != null) m_OnDropdownListDestroy();

        if (!m_ReuseDropdownList)
        {
            base.DestroyDropdownList(dropdownList);
        }
        else
        {
            bool skipTemplate = false;
            foreach (var item in dropdownList.GetComponentsInChildren<ExtendDropdownItem>(true))
            {
                if (!skipTemplate && !item.gameObject.activeSelf)
                {
                    skipTemplate = true;
                    // Reset 'sizeDelta.y' of 'Content' to original value.
                    var rtf = item.gameObject.transform.parent as RectTransform;
                    if (rtf != null)
                    {
                        Vector2 sizeDelta = rtf.sizeDelta;
                        sizeDelta.y = m_ItemTemplateContentSizeDelta_Backup.y;
                        rtf.sizeDelta = sizeDelta;
                    }
                    // Reset state of itemTemplate to active.
                    item.gameObject.SetActive(true);
                    continue;
                }

                if (m_PoolingItems)
                {
                    ReleaseDropdownItemToPool(item);
                }
                else
                {
                    DestroyImmediate(item.gameObject);
                }
            }
            dropdownList.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        m_OnSetDropdownItemTemplate = null;
        m_OnCreateDropdownItem = null;
        m_BeforeDropdownShown = null;
        m_OnDropdownListDestroy = null;
        m_OnSetDropdownTemplate = null;
        m_OnBlockerDestroy = null;
        m_OnReleaseDropdownItem = null;
        if (m_DropdownList != null) DestroyImmediate(m_DropdownList);
        if (m_ItemsContainer != null) DestroyImmediate(m_ItemsContainer);
        ClearItemsPool();
        m_ItemsPool = null;
        base.OnDestroy();
    }

    private void ModifiedItemTemplate(DropdownItem itemTemplate)
    {
        // Back up the original size delta of 'Content'.
        var rtf = itemTemplate.transform.parent as RectTransform;
        if (rtf != null) m_ItemTemplateContentSizeDelta_Backup = rtf.sizeDelta;

        var go = itemTemplate.gameObject;
        if (!go.TryGetComponent<ExtendDropdownItem>(out var item))
        {
            item = go.AddComponent<ExtendDropdownItem>();
            item.text = itemTemplate.text;
            item.rectTransform = itemTemplate.rectTransform;
            item.image = itemTemplate.image;
            item.toggle = itemTemplate.toggle;
        }

        if (m_OnSetDropdownItemTemplate != null)
        {
            m_OnSetDropdownItemTemplate(item);
        }

        DestroyImmediate(itemTemplate);
    }

    private bool ManualInitializeCheck()
    {
        if (m_ManualInitialize)
        {
            if (!m_IsManualInitializeFinished)
                return false;
        }
        return true;
    }

    private ExtendDropdownItem GetDropdownItemFromPool()
    {
        if (m_ItemsPool == null)
        {
            m_ItemsPool = new Stack<ExtendDropdownItem>();
            m_ItemsContainer = new GameObject("Items Pool", typeof(RectTransform));
            m_ItemsContainer.SetActive(false);
            m_ItemsContainer.transform.SetParent(transform);
        }

        if (m_ItemsPool.Count > 0)
        {
            return m_ItemsPool.Pop();
        }
        else
        {
            var itemTemplate = template.GetComponentInChildren<ExtendDropdownItem>();
            var item = Instantiate(itemTemplate);
            item.transform.SetParent(m_ItemsContainer.transform, false);
            return item;
        }
    }

    private void ReleaseDropdownItemToPool(ExtendDropdownItem item)
    {
        if (item == null || m_ItemsPool == null || m_ItemsContainer == null) return;

        if (m_OnReleaseDropdownItem != null) m_OnReleaseDropdownItem(item);
        item.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        item.gameObject.SetActive(false);
        item.transform.SetParent(m_ItemsContainer.transform, false);
        m_ItemsPool.Push(item);
    }
}