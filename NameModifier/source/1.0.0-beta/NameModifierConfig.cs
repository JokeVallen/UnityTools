#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools.NameModifier
{
    /// <summary>
    /// 名称修改器配置
    /// </summary>
    /// <remarks>
    /// <para>
    /// 以 ScriptableObject 资产形式存储名称修改器工具的所有运行时配置，
    /// 并以单例模式对外提供访问（<see cref="Instance"/>）。
    /// </para>
    /// <para>
    /// 使用方式：在项目任意位置通过
    /// <c>Assets / Create / EditorTools / NameModifierConfig</c>
    /// 菜单创建配置资产。创建后工具窗口将自动识别并持久化路径；
    /// 若未创建资产，则工具以临时内存实例运行，配置不会被保存。
    /// </para>
    /// <para>
    /// 配置支持导出为 JSON 文件以及从 JSON 文件导入，便于团队共享或版本管理。
    /// </para>
    /// </remarks>
    [CreateAssetMenu(fileName = "NameModifierConfig.asset", menuName = "EditorTools/NameModifierConfig", order = 0)]
    public sealed class NameModifierConfig : ScriptableObject
    {
        /// <summary>
        /// 处理器目录路径
        /// </summary>
        /// <remarks>
        /// 相对于项目 <c>Assets</c> 目录的路径，工具窗口将在该目录下递归搜索
        /// 所有 <see cref="NameModifierHandler"/> 类型的资产并加载为可选处理器。
        /// 可在配置面板中通过"选择处理器目录"按钮设置。
        /// </remarks>
        public string HandlerPath => m_HandlerPath;
        [SerializeField, Tooltip("处理器目录路径")] private string m_HandlerPath;

        internal UndoSystemType UndoSystemType => m_UndoSystemType;
        [SerializeField, Tooltip("撤销系统类型")]
        private UndoSystemType m_UndoSystemType = UndoSystemType.SessionState;

        internal int UndoCapacity => m_UndoCapacity;
        [SerializeField, Tooltip("每对象最大历史条数（0不限）")]
        private int m_UndoCapacity = 10;

        internal int MaxTrackedObjects => m_MaxTrackedObjects;
        [SerializeField, Tooltip("最大追踪对象数（0=默认10000）")]
        private int m_MaxTrackedObjects = 0;

        internal int DefaultGroupCapacity => m_DefaultGroupCapacity;
        [SerializeField, Tooltip("分组最大步数（0不限）")]
        private int m_DefaultGroupCapacity = 20;

        internal string DefaultGroupNameTemplate => m_DefaultGroupNameTemplate;
        [SerializeField, Tooltip("分组名模板，支持 {Date} {Time} {DateTime}")]
        private string m_DefaultGroupNameTemplate = "分组_{DateTime}";

        internal bool AutoReset => m_AutoReset;
        [SerializeField, Tooltip("关闭时重置处理器")]
        private bool m_AutoReset = false;

        internal bool AutoClearInvalidCache => m_AutoClearInvalidCache;
        [SerializeField, Tooltip("切换处理器时清除无效缓存")]
        private bool m_AutoClearInvalidCache = false;

        internal bool AutoClearCache => m_AutoClearCache;
        [SerializeField, Tooltip("关闭时清除缓存")]
        private bool m_AutoClearCache = true;

        internal bool LogEnabled => m_LogEnabled;
        [SerializeField, Tooltip("启用日志")]
        private bool m_LogEnabled = true;

        internal bool IsPersistent => !m_IsTemp && EditorUtility.IsPersistent(this);
        private bool m_IsTemp;

        internal static string AssetPathQualifiedKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(s_AssetPathQualifiedKey))
                {
                    string projectGuid = NameModifierUtility.GUIDUtility.GetOrCreateProjectQualifiedGUID();
                    s_AssetPathQualifiedKey = $"{projectGuid}.{QUALIFIED_NAME}";
                }
                return s_AssetPathQualifiedKey;
            }
        }
        private static string s_AssetPathQualifiedKey;

        internal const string NO_ASSET_TIPS = "你需要在项目中的任一位置创建配置资源，以便为配置数据提供持久存储支持。";
        internal const string NO_HANDLER_TIPS = "处理器不存在。您需要选择一个有效的处理器目录或创建一个处理器资源。";
        private const string QUALIFIED_NAME = "NameModifierConfig_AssetPath";

        /// <summary>
        /// 日志记录器
        /// </summary>
        /// <remarks>
        /// <para>
        /// 工具内部所有日志输出的统一出口。默认使用 <c>NameModifierDefaultLogger</c>，
        /// 将日志转发到 Unity 的 <c>Debug</c> API，并受 <c>LogEnabled</c> 开关控制。
        /// </para>
        /// <para>
        /// 可在运行时替换为自定义实现，以便将日志接入项目自有的日志系统：
        /// <code>
        /// NameModifierConfig.Logger = new MyCustomLogger();
        /// </code>
        /// 替换后立即生效，且对全局单例实例有效。
        /// </para>
        /// </remarks>
        public static INameModifierLogger Logger
        {
            get
            {
                if (s_Logger == null)
                {
                    s_Logger = s_DefaultLogger;
                }
                return s_Logger;
            }
            set => s_Logger = value;
        }
        private static INameModifierLogger s_Logger;
        private static readonly INameModifierLogger s_DefaultLogger = new NameModifierDefaultLogger();

        /// <summary>
        /// 配置单例
        /// </summary>
        /// <remarks>
        /// <para>
        /// 首次访问时自动尝试从 <c>EditorPrefs</c> 中读取上次记录的资产路径并加载；
        /// 若资产不存在或路径无效，则创建一个带有 <c>HideFlags.HideAndDontSave</c>
        /// 的临时内存实例，此时 <see cref="IsPersistent"/> 返回 <c>false</c>。
        /// </para>
        /// <para>
        /// 在项目中创建配置资产并通过 Inspector 打开后，路径会被持久化到
        /// <c>EditorPrefs</c>，后续访问将始终加载该资产实例。
        /// </para>
        /// </remarks>
        public static NameModifierConfig Instance
        {
            get
            {
                if (s_Instance == null) CreateAndLoad();
                return s_Instance;
            }
        }

        private static NameModifierConfig s_Instance;
        private static NameModifierConfig s_TempInstance;

        internal void ExportToJSON(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(path, JsonUtility.ToJson(this, true));
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Logger.LogError($"ExportToJSON Failed:{e.Message}");
            }
        }

        internal void LoadFromJSON(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                string jsonStr = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(jsonStr))
                    JsonUtility.FromJsonOverwrite(jsonStr, this);
            }
            catch (System.Exception e)
            {
                Logger.LogError($"LoadFromJSON Failed:{e.Message}");
            }
        }

        private NameModifierConfig()
        {
            if (s_Instance != null)
            {
                if (s_Instance.m_IsTemp)
                    s_TempInstance = s_Instance;
                else if (!m_IsTemp)
                    Logger.LogError("There should not be two or more assets with configuration in the same project.");

                s_Instance = null;
            }

            if (s_Instance != null)
                Logger.LogError("ScriptableSingleton already exists. Did you query the singleton in a constructor?");
            else
                s_Instance = this;
        }

        private void Awake()
        {
            if (ReferenceEquals(s_Instance, this))
                DestroyImmediate(s_TempInstance);
        }

        private static void CreateAndLoad()
        {
            try
            {
                if (EditorPrefs.HasKey(AssetPathQualifiedKey))
                {
                    string assetPath = EditorPrefs.GetString(AssetPathQualifiedKey);
                    if (File.Exists(assetPath))
                    {
                        int lastIndex = assetPath.LastIndexOf("Assets");
                        if (lastIndex >= 0)
                        {
                            assetPath = assetPath.Substring(lastIndex, assetPath.Length - lastIndex);
                            s_Instance = null;
                            AssetDatabase.LoadAssetAtPath<NameModifierConfig>(assetPath);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"LoadFromAsset Failed:{e.Message}");
            }
            finally
            {
                if (s_Instance == null)
                {
                    NameModifierConfig val = CreateInstance<NameModifierConfig>();
                    val.hideFlags = HideFlags.HideAndDontSave;
                    val.m_IsTemp = true;
                }
            }
        }

        private Editor m_Editor;

        internal void DrawDetailGUI()
        {
            if (m_Editor == null) m_Editor = Editor.CreateEditor(this);
            m_Editor.OnInspectorGUI();
        }

    }
}

#endif