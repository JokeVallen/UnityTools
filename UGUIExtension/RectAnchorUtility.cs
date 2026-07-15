using UnityEngine;

namespace UIAssistant.Core
{
    /// <summary>
    /// 锚点模式
    /// </summary>
    public enum AnchorMode : ushort
    {
        Custom = 0,

        /// <summary>
        /// 中心锚点
        /// </summary>
        Center,

        /// <summary>
        /// 左锚点
        /// </summary>
        LeftMiddle,

        /// <summary>
        /// 右锚点
        /// </summary>
        RightMiddle,

        /// <summary>
        /// 上锚点
        /// </summary>
        TopMiddle,

        /// <summary>
        /// 下锚点
        /// </summary>
        BottomMiddle,

        /// <summary>
        /// 左上锚点
        /// </summary>
        LeftTop,

        /// <summary>
        /// 右上锚点
        /// </summary>
        RightTop,

        /// <summary>
        /// 右下锚点
        /// </summary>
        RightBottom,

        /// <summary>
        /// 坐下锚点
        /// </summary>
        LeftBottom,

        /// <summary>
        /// 左锚点且垂直填充
        /// </summary>
        LeftAndVerticalFill,

        /// <summary>
        /// 上锚点且水平填充
        /// </summary>
        TopAndHorizontalFill,

        /// <summary>
        /// 右锚点且垂直填充
        /// </summary>
        RightAndVerticalFill,

        /// <summary>
        /// 下锚点且水平填充
        /// </summary>
        BottomAndHorizontalFill,

        /// <summary>
        /// 水平和垂直填充
        /// </summary>
        HorizontalAndVerticalFill
    }

    /// <summary>
    /// RectTransform 锚点布局工具类
    /// </summary>
    /// <remarks>
    /// <para>功能：提供 RectTransform 的 anchorMin 与 anchorMax 坐标与易读的 AnchorMode 枚举之间的双向转换与快速设置。</para>
    /// <para>设计优势：避开了与 UnityEngine.RectTransformUtility 的命名冲突，采用纯数值等价判断，0 运行时内存分配（Zero-Allocation）。</para>
    /// </remarks>
    public static class RectAnchorUtility
    {
        private static readonly Vector2 HalfVector = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// 获取 RectTransform 当前的锚点布局模式
        /// </summary>
        /// <param name="rectTransform">矩形变换组件</param>
        /// <returns>解析出的锚点布局模式</returns>
        public static AnchorMode GetAnchorMode(RectTransform rectTransform)
        {
            if (rectTransform == null) return AnchorMode.Custom;
            return GetAnchorMode(rectTransform.anchorMin, rectTransform.anchorMax);
        }

        /// <summary>
        /// 通过具体的 anchorMin 和 anchorMax 坐标解析出对应的锚点布局模式
        /// </summary>
        /// <param name="anchorMin">锚点最小值</param>
        /// <param name="anchorMax">锚点最大值</param>
        /// <returns>匹配的锚点布局模式，若无匹配则返回 Custom</returns>
        public static AnchorMode GetAnchorMode(Vector2 anchorMin, Vector2 anchorMax)
        {
            if (anchorMin.x == 0.5f && anchorMin.y == 0.5f && anchorMax.x == 0.5f && anchorMax.y == 0.5f)
            {
                return AnchorMode.Center;
            }

            if (anchorMin.x == 0f && anchorMax.x == 0f)
            {
                if (anchorMin.y == 0.5f && anchorMax.y == 0.5f) return AnchorMode.LeftMiddle;
                if (anchorMin.y == 1f && anchorMax.y == 1f) return AnchorMode.LeftTop;
                if (anchorMin.y == 0f && anchorMax.y == 0f) return AnchorMode.LeftBottom;
                if (anchorMin.y == 0f && anchorMax.y == 1f) return AnchorMode.LeftAndVerticalFill;
            }

            if (anchorMin.x == 1f && anchorMax.x == 1f)
            {
                if (anchorMin.y == 0.5f && anchorMax.y == 0.5f) return AnchorMode.RightMiddle;
                if (anchorMin.y == 1f && anchorMax.y == 1f) return AnchorMode.RightTop;
                if (anchorMin.y == 0f && anchorMax.y == 0f) return AnchorMode.RightBottom;
                if (anchorMin.y == 0f && anchorMax.y == 1f) return AnchorMode.RightAndVerticalFill;
            }

            if (anchorMin.x == 0.5f && anchorMax.x == 0.5f)
            {
                if (anchorMin.y == 1f && anchorMax.y == 1f) return AnchorMode.TopMiddle;
                if (anchorMin.y == 0f && anchorMax.y == 0f) return AnchorMode.BottomMiddle;
            }

            if (anchorMin.y == 1f && anchorMax.y == 1f && anchorMin.x == 0f && anchorMax.x == 1f)
            {
                return AnchorMode.TopAndHorizontalFill;
            }

            if (anchorMin.y == 0f && anchorMax.y == 0f && anchorMin.x == 0f && anchorMax.x == 1f)
            {
                return AnchorMode.BottomAndHorizontalFill;
            }

            if (anchorMin.x == 0f && anchorMin.y == 0f && anchorMax.x == 1f && anchorMax.y == 1f)
            {
                return AnchorMode.HorizontalAndVerticalFill;
            }

            return AnchorMode.Custom;
        }

        /// <summary>
        /// 尝试通过锚点布局模式计算出具体的 anchorMin 和 anchorMax 值
        /// </summary>
        /// <param name="mode">要查询的锚点布局模式</param>
        /// <param name="anchorMin">计算输出的锚点最小值</param>
        /// <param name="anchorMax">计算输出的锚点最大值</param>
        /// <returns>如果模式有效且成功解析返回 true，否则返回 false</returns>
        public static bool TryGetAnchorByMode(AnchorMode mode, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            switch (mode)
            {
                case AnchorMode.Center:
                    anchorMin = HalfVector;
                    anchorMax = HalfVector;
                    return true;

                case AnchorMode.LeftMiddle:
                    anchorMin = new Vector2(0f, 0.5f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.RightMiddle:
                    anchorMin = new Vector2(1f, 0.5f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.TopMiddle:
                    anchorMin = new Vector2(0.5f, 1f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.BottomMiddle:
                    anchorMin = new Vector2(0.5f, 0f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.LeftTop:
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.RightTop:
                    anchorMin = new Vector2(1f, 1f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.RightBottom:
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = anchorMin;
                    return true;

                case AnchorMode.LeftBottom:
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.zero;
                    return true;

                case AnchorMode.LeftAndVerticalFill:
                    anchorMin = Vector2.zero;
                    anchorMax = new Vector2(0f, 1f);
                    return true;

                case AnchorMode.TopAndHorizontalFill:
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = Vector2.one;
                    return true;

                case AnchorMode.RightAndVerticalFill:
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = Vector2.one;
                    return true;

                case AnchorMode.BottomAndHorizontalFill:
                    anchorMin = Vector2.zero;
                    anchorMax = new Vector2(1f, 0f);
                    return true;

                case AnchorMode.HorizontalAndVerticalFill:
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.one;
                    return true;
            }

            anchorMin = Vector2.zero;
            anchorMax = Vector2.zero;
            return false;
        }

        /// <summary>
        /// 为指定的 RectTransform 一键设置锚点布局模式
        /// </summary>
        /// <param name="rectTransform">矩形变换组件</param>
        /// <param name="mode">锚点布局模式</param>
        public static void SetAnchorByMode(RectTransform rectTransform, AnchorMode mode)
        {
            if (rectTransform == null) return;
            if (TryGetAnchorByMode(mode, out var anchorMin, out var anchorMax))
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
            }
        }
    }
}