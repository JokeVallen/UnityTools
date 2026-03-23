#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace EditorTools.NameModifier
{
    /// <summary>
    /// 名称修改处理器基类
    /// </summary>
    /// <remarks>
    /// <para>
    /// 所有自定义重命名策略均需继承此类并实现 <see cref="Modify"/> 与
    /// <see cref="OptionName"/>。将派生类保存为 ScriptableObject 资产并放置于
    /// <see cref="NameModifierConfig.HandlerPath"/> 所指定的目录下，
    /// 工具窗口将自动扫描并加载。
    /// </para>
    /// <para>
    /// 最简实现示例：
    /// <code>
    /// [CreateAssetMenu(menuName = "EditorTools/Handlers/MyHandler")]
    /// public class MyHandler : NameModifierHandler
    /// {
    ///     public override string OptionName => "我的处理器";
    ///
    ///     public override void Modify(Object obj, int index, int count)
    ///     {
    ///         ApplyRename(obj, $"Item_{index:D3}");
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public abstract class NameModifierHandler : ScriptableObject
    {
        /// <summary>
        /// 处理器选项名称
        /// </summary>
        /// <remarks>
        /// 显示在工具窗口的处理器下拉列表中，用于区分不同的重命名策略。
        /// 建议使用简短、直观的中文名称，例如"序号后缀"、"前缀替换"等。
        /// </remarks>
        public abstract string OptionName { get; }

        /// <summary>
        /// 处理器提示文本
        /// </summary>
        /// <remarks>
        /// 显示在工具窗口底部的 HelpBox 中，用于向用户说明当前处理器的使用方法
        /// 或注意事项。默认返回空字符串，子类可按需重写。
        /// </remarks>
        public virtual string Tip => string.Empty;

        private IUndoSystem m_UndoSystem;
        private UnityAction m_RepaintDriver;

        /// <summary>
        /// 对单个对象执行重命名
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="index">批次索引</param>
        /// <param name="count">批次总数</param>
        /// <remarks>
        /// <para>
        /// 工具窗口点击"修改"后，对每一个已选中的对象依次调用此方法。
        /// 子类应在此方法内计算目标名称，并通过 <see cref="ApplyRename"/> 应用。
        /// </para>
        /// <para>
        /// <paramref name="index"/> 从 0 开始，<paramref name="count"/> 为本次批量操作
        /// 的对象总数，可利用这两个参数生成带序号的名称，例如：
        /// <code>
        /// ApplyRename(obj, $"{prefix}_{index:D3}");
        /// </code>
        /// </para>
        /// </remarks>
        public abstract void Modify(Object obj, int index, int count);

        /// <summary>
        /// 绘制处理器自定义 GUI
        /// </summary>
        /// <remarks>
        /// 在工具窗口中处理器选择下拉框下方调用，用于绘制该处理器专属的参数输入界面。
        /// 使用标准的 <c>EditorGUILayout</c> API 即可；默认实现为空，子类按需重写。
        /// </remarks>
        public virtual void DrawGUI() { }

        /// <summary>
        /// 重置处理器状态
        /// </summary>
        /// <remarks>
        /// 将处理器内部的所有用户输入参数或运行时状态恢复为初始值。
        /// 在 <see cref="NameModifierConfig"/> 的 <c>AutoReset</c> 开关开启时，
        /// 工具窗口关闭时会自动调用；也可由用户在 GUI 中手动触发。
        /// 默认实现为空，子类按需重写。
        /// </remarks>
        public virtual void Reset() { }

        /// <summary>
        /// 处理器被选中时的回调
        /// </summary>
        /// <remarks>
        /// 当用户在下拉列表中切换到本处理器时由工具窗口调用，可用于延迟初始化或刷新
        /// 缓存数据。默认实现为空，子类按需重写。
        /// </remarks>
        public virtual void OnSelected() { }

        internal void Initialize(UnityAction repaintDriver, IUndoSystem undoSystem)
        {
            m_RepaintDriver = repaintDriver;
            m_UndoSystem = undoSystem;
            OnInitialize();
        }

        internal void ClearInvalidCache()
        {
            m_UndoSystem?.ClearInvalid();
        }

        internal void ClearCache()
        {
            m_UndoSystem?.ClearAll();
        }

        /// <summary>
        /// 处理器初始化回调
        /// </summary>
        /// <remarks>
        /// 在工具窗口完成内部依赖注入（撤销系统、重绘回调）之后调用，
        /// 子类可在此处执行需要访问这些依赖的初始化逻辑。
        /// 默认实现为空，按需重写。
        /// </remarks>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 请求重绘工具窗口
        /// </summary>
        /// <remarks>
        /// 当处理器内部状态发生变化（例如异步数据加载完成）需要立即刷新 GUI 时调用。
        /// 内部转发至工具窗口的 <c>Repaint</c> 委托。
        /// </remarks>
        protected void Repaint()
        {
            m_RepaintDriver?.Invoke();
        }

        /// <summary>
        /// 应用重命名并记录历史
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="newName">新名称</param>
        /// <remarks>
        /// <para>
        /// 子类在 <see cref="Modify"/> 中应调用此方法而非直接修改 <c>obj.name</c>，
        /// 以确保改名操作被撤销系统正确记录。
        /// </para>
        /// <para>
        /// 以下情况下此方法为空操作，不会产生任何副作用：
        /// <list type="bullet">
        ///   <item><description><paramref name="obj"/> 为 <c>null</c></description></item>
        ///   <item><description><paramref name="newName"/> 为空或纯空白字符</description></item>
        ///   <item><description><paramref name="newName"/> 与对象当前名称相同</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        protected void ApplyRename(Object obj, string newName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(newName)) return;
            if (obj.name == newName) return;

            string oldName = obj.name;
            ApplyNameOnly(obj, newName);
            m_UndoSystem?.Record(obj, oldName, newName);
        }

        internal static void ApplyNameOnly(Object obj, string name)
        {
            if (EditorUtility.IsPersistent(obj))
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                AssetDatabase.RenameAsset(assetPath, name);
            }
            else
            {
                obj.name = name;
                EditorUtility.SetDirty(obj);
            }
        }
    }
}

#endif