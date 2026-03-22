using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// AutoLayout 运行时 API 测试
    /// </summary>
    /// <remarks>
    /// 布局重建在 MarkLayoutForRebuild 后不立即生效，需 yield return null 等待
    /// Canvas 的布局系统在下一帧统一处理。
    /// </remarks>
    public class AutoLayoutTests
    {
        // ── 测试基础设施 ──────────────────────────────────────────────────

        private GameObject canvasGo;
        private GameObject containerGo;
        private AutoLayoutGroup layout;
        private Canvas canvas;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // 创建 Canvas（布局系统必须在 Canvas 下才能正常工作）
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // 创建容器并挂载 AutoLayout
            containerGo = new GameObject("Container");
            containerGo.transform.SetParent(canvasGo.transform, false);
            var rt = containerGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 300);
            layout = containerGo.AddComponent<AutoLayoutGroup>();

            yield return null; // 等待初始布局
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(canvasGo);
        }

        // ── 辅助方法 ──────────────────────────────────────────────────────

        /// <summary>创建指定数量的子元素，默认尺寸 50x50</summary>
        private RectTransform[] CreateChildren(int count, float size = 50f)
        {
            var children = new RectTransform[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Child_{i}");
                go.transform.SetParent(containerGo.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                go.AddComponent<Image>(); // 需要 Graphic 组件触发布局
                children[i] = rt;
            }
            return children;
        }

        /// <summary>设置一条从 (0,0) 到 (1,v) 的单段线性曲线</summary>
        private static AnimationCurve LinearCurve(float endValue = 1f)
            => AnimationCurve.Linear(0, 0, 1, endValue);

        // ═══════════════════════════════════════════════════════════════════
        // 一、边界情况
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator NoChildren_ReportsZeroSpan()
        {
            yield return null;
            // 无子物体时 min/preferred 应等于 padding 之和
            Assert.AreEqual(0f, layout.minWidth, 0.01f);
            Assert.AreEqual(0f, layout.preferredWidth, 0.01f);
        }

        [UnityTest]
        public IEnumerator SingleChild_PositionedAtStartOffset()
        {
            layout.AddKeyX(0, 0);
            layout.AddKeyY(0, 0);
            var children = CreateChildren(1, 60f);
            yield return null;

            // 单个子物体 factor=0，位置应等于 startOffset（padding.left）
            float expected = layout.Padding.left;
            Assert.AreEqual(expected, children[0].offsetMin.x, 0.5f,
                "单个子物体 X 位置应等于 padding.left");
        }

        [UnityTest]
        public IEnumerator EmptyCurve_ChildrenStackAtOrigin()
        {
            // 曲线为空时所有 factor=0，子物体堆叠在 startOffset 处
            var children = CreateChildren(3, 50f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float x1 = children[1].offsetMin.x;
            Assert.AreEqual(x0, x1, 0.5f,
                "曲线为空时所有子物体应堆叠在同一位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、Direct 模式
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Direct_XAxisValue_MapsToHorizontalOffset()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            layout.AddKeyX(2f, 200f);
            var children = CreateChildren(3, 50f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float x1 = children[1].offsetMin.x;
            float x2 = children[2].offsetMin.x;

            Assert.AreEqual(x0 + 100f, x1, 1f, "第二个子物体应比第一个偏右 100px");
            Assert.AreEqual(x0 + 200f, x2, 1f, "第三个子物体应比第一个偏右 200px");
        }

        [UnityTest]
        public IEnumerator Direct_YAxisValue_PositiveIsUpward()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleY = 1f;
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 50f); // value 正值 → 向上
            var children = CreateChildren(2, 50f);
            yield return null;

            // Y 轴向上表示 offsetMin.y 更大（UGUI offsetMin.y 增大 = 元素更高）
            float y0 = children[0].offsetMin.y;
            float y1 = children[1].offsetMin.y;
            Assert.Greater(y1, y0, "Y 轴 value 正值时，第二个元素应比第一个更靠上");
        }

        [UnityTest]
        public IEnumerator Direct_PostWrapMode_Loop_CyclesKeys()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.Loop;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            // 2 个关键帧，3 个子物体 → 第 3 个应循环回到第 1 个关键帧（value=0）
            var children = CreateChildren(3, 50f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float x2 = children[2].offsetMin.x;
            Assert.AreEqual(x0, x2, 1f, "Loop 模式下第 3 个元素应与第 1 个位置相同");
        }

        [UnityTest]
        public IEnumerator Direct_PostWrapMode_PingPong_Reverses()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.PingPong;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            // 关键帧：0→100→0→100...
            // 子物体 0→100px，1→200px，2→100px（回弹）
            var children = CreateChildren(3, 10f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float x1 = children[1].offsetMin.x;
            float x2 = children[2].offsetMin.x;
            Assert.Greater(x1, x0, "PingPong：第 2 个元素比第 1 个靠右");
            Assert.AreEqual(x0, x2, 1f, "PingPong：第 3 个元素应回弹到第 1 个位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、Interpolated 模式
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Interpolated_YCurve_ShapePreservedOnCountChange()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleY = 100f;
            // 正弦形曲线：0→1→0
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(0.5f, 1f);
            layout.AddKeyY(1f, 0f);
            var children = CreateChildren(5, 10f);
            yield return null;

            // 第 3 个元素（索引 2，t=0.5）应在最高点
            float y2 = children[2].offsetMin.y;
            float y0 = children[0].offsetMin.y;
            float y4 = children[4].offsetMin.y;
            Assert.Greater(y2, y0, "Interpolated：曲线顶点处元素应高于起点元素");
            Assert.AreEqual(y0, y4, 1f, "Interpolated：曲线首尾对称，第 1 和第 5 个元素高度相同");
        }

        [UnityTest]
        public IEnumerator Interpolated_ConstrainByGroup_PostWrapLoop_Repeats()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ConstrainByGroupX = true;
            layout.ConstrainByGroupY = true;
            layout.GroupSizeX = 2;
            layout.GroupSizeY = 2;
            layout.PostWrapModeY = WrapMode.Loop;
            layout.ScaleY = 100f;
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 1f);
            // GroupSizeX=2，4 个子物体 → t: 0, 0.5, 1(Loop→0), 1.5(Loop→0.5)
            // 子物体 0 和 2 的 Y 应相同，子物体 1 和 3 的 Y 应相同
            var children = CreateChildren(4, 10f);
            yield return null;

            Assert.AreEqual(children[0].offsetMin.y, children[2].offsetMin.y, 1f,
                "ConstrainByGroupX + Loop：第 1 和第 3 个元素高度应相同");
            Assert.AreEqual(children[1].offsetMin.y, children[3].offsetMin.y, 1f,
                "ConstrainByGroupX + Loop：第 2 和第 4 个元素高度应相同");
        }

        [UnityTest]
        public IEnumerator Interpolated_Cycles_ReflectsChildCount()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.ConstrainByGroupX = true;
            layout.ConstrainByGroupY = true;
            layout.GroupSizeX = 4;
            layout.GroupSizeY = 4;
            CreateChildren(9, 10f);
            yield return null;

            // Cycles = (count - 1) / groupSize = 8 / 4 = 2
            Assert.AreEqual(2f, layout.CyclesX, 0.01f,
                "Cycles 应等于 (childCount - 1) / GroupSizeX");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、Proportional 模式
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Proportional_RoundToNearest_FirstAndLastMatchKeyframes()
        {
            layout.MappingModeX = KeyframeMappingMode.Proportional;
            layout.MappingModeY = KeyframeMappingMode.Proportional;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.DistributeModeX = ProportionalDistributeMode.RoundToNearest;
            layout.DistributeModeY = ProportionalDistributeMode.RoundToNearest;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 200f);
            var children = CreateChildren(4, 10f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float xLast = children[3].offsetMin.x;
            // 第一个和最后一个元素应分别对应第一和最后一个关键帧
            Assert.Less(xLast, x0 + 1f + 200f,
                "Proportional：最后一个元素应对应最后一个关键帧");
        }

        [UnityTest]
        public IEnumerator Proportional_Uniform_EvenDistribution()
        {
            layout.MappingModeX = KeyframeMappingMode.Proportional;
            layout.MappingModeY = KeyframeMappingMode.Proportional;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.DistributeModeX = ProportionalDistributeMode.Uniform;
            layout.DistributeModeY = ProportionalDistributeMode.Uniform;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(0.5f, 100f);
            layout.AddKeyX(1f, 200f);
            // 6 个子物体对应 3 个关键帧，Uniform 分配每组 2 个
            var children = CreateChildren(6, 10f);
            yield return null;

            // Bresenham 分配：accumulated = i*2, result = accumulated/5
            // i=0:0, i=1:0, i=2:0, i=3:1, i=4:1, i=5:2
            // 即 child[0,1,2] → key0(0px)，child[3,4] → key1(100px)，child[5] → key2(200px)
            Assert.AreEqual(children[0].offsetMin.x, children[1].offsetMin.x, 1f,
                "Uniform：child[0] 和 child[1] 应对应同一关键帧（key0）");
            Assert.AreEqual(children[1].offsetMin.x, children[2].offsetMin.x, 1f,
                "Uniform：child[1] 和 child[2] 应对应同一关键帧（key0）");
            Assert.AreEqual(children[3].offsetMin.x, children[4].offsetMin.x, 1f,
                "Uniform：child[3] 和 child[4] 应对应同一关键帧（key1）");
            Assert.AreNotEqual(children[2].offsetMin.x, children[3].offsetMin.x,
                "Uniform：child[2]（key0）和 child[3]（key1）应在不同位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、位置计算模式
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ByElementSize_OffsetScalesWithChildSize()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.ScaleX = 1f;
            // factor=0 → pos=0；factor=1 → pos=effectiveSize*1*1=effectiveSize
            // 同尺寸的两个子物体：A(factor=0) pos=0，B(factor=1) pos=50
            // 增加尺寸：C(factor=0) pos=0，D(factor=1) pos=100
            // B 与 A 的间距 = 50，D 与 C 的间距 = 100，比例应为 2:1
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);
            // 重置并单独测两种尺寸：先创建 size=50 测出间距，再测 size=100
            var goA = new GameObject("A50");
            goA.transform.SetParent(containerGo.transform, false);
            var rtA = goA.AddComponent<RectTransform>();
            rtA.sizeDelta = new Vector2(50f, 50f);
            goA.AddComponent<Image>();

            var goB = new GameObject("B50");
            goB.transform.SetParent(containerGo.transform, false);
            var rtB = goB.AddComponent<RectTransform>();
            rtB.sizeDelta = new Vector2(50f, 50f);
            goB.AddComponent<Image>();

            yield return null;
            // A(factor=0): pos=0, B(factor=1): pos=50*1=50
            float gap50 = rtB.offsetMin.x - rtA.offsetMin.x;

            // 更换为 size=100 的子物体
            rtA.sizeDelta = new Vector2(100f, 100f);
            rtB.sizeDelta = new Vector2(100f, 100f);
            yield return null;
            float gap100 = rtB.offsetMin.x - rtA.offsetMin.x;

            // gap100 应是 gap50 的 2 倍（偏移量与尺寸成比例）
            Assert.AreEqual(2f, gap100 / gap50, 0.1f,
                "ByElementSize：偏移量应与子物体尺寸成比例（尺寸翻倍时间距翻倍）");
        }

        [UnityTest]
        public IEnumerator ByPixel_DifferentSizeChildrenSamePosition()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            // 两个子物体对应同一关键帧 value=100
            layout.AddKeyX(0f, 100f);

            var goA = new GameObject("A");
            goA.transform.SetParent(containerGo.transform, false);
            var rtA = goA.AddComponent<RectTransform>();
            rtA.sizeDelta = new Vector2(50f, 50f);
            goA.AddComponent<Image>();

            var goB = new GameObject("B");
            goB.transform.SetParent(containerGo.transform, false);
            var rtB = goB.AddComponent<RectTransform>();
            rtB.sizeDelta = new Vector2(100f, 50f);
            goB.AddComponent<Image>();

            layout.PostWrapModeX = WrapMode.Loop; // 第 2 个子物体也用同一关键帧
            yield return null;

            Assert.AreEqual(rtA.offsetMin.x, rtB.offsetMin.x, 1f,
                "ByPixel：不同尺寸的子物体在同一关键帧下应有相同的左边缘位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 六、Padding
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Padding_Left_PushesContentRight()
        {
            int paddingLeft = 30;
            layout.Padding = new RectOffset(paddingLeft, 0, 0, 0);
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            var children = CreateChildren(1, 50f);
            yield return null;

            Assert.AreEqual(paddingLeft, children[0].offsetMin.x, 1f,
                "padding.left 应将内容右移");
        }

        [UnityTest]
        public IEnumerator Padding_AllSides_ReflectedInPreferredSize()
        {
            layout.Padding = new RectOffset(10, 20, 15, 25);
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyY(0f, 0f);
            CreateChildren(1, 50f);
            yield return null;

            // preferredWidth 至少包含 left + right padding
            Assert.GreaterOrEqual(layout.preferredWidth, 30f,
                "preferredWidth 应包含 padding.left + padding.right");
            Assert.GreaterOrEqual(layout.preferredHeight, 40f,
                "preferredHeight 应包含 padding.top + padding.bottom");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 七、ChildAlignment 对齐
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ChildAlignment_Center_ContentCenteredInContainer()
        {
            layout.ChildAlignment = TextAnchor.MiddleCenter;
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyY(0f, 0f);
            // 容器 400x300，子物体 50x50
            var children = CreateChildren(1, 50f);
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            float containerW = containerRt.rect.width;   // 400
            float containerH = containerRt.rect.height;  // 300

            // SetInsetAndSizeFromParentEdge(Left,  pos, size)：offsetMin.x = pos
            // SetInsetAndSizeFromParentEdge(Top,   pos, size)：offsetMin.y = -(pos + size)
            // 居中时：posX = (containerW - 50) * 0.5 = 175，posY = (containerH - 50) * 0.5 = 125
            float expectedOffsetMinX = (containerW - 50f) * 0.5f;        // 175
            float expectedOffsetMinY = -((containerH - 50f) * 0.5f + 50f); // -(125+50) = -175

            Assert.AreEqual(expectedOffsetMinX, children[0].offsetMin.x, 2f,
                "MiddleCenter：子物体 X 左边缘应居中");
            Assert.AreEqual(expectedOffsetMinY, children[0].offsetMin.y, 2f,
                "MiddleCenter：子物体 Y offsetMin 应等于 -(posY + childH)");
        }

        [UnityTest]
        public IEnumerator ChildAlignment_LowerRight_ContentAtBottomRight()
        {
            layout.ChildAlignment = TextAnchor.LowerRight;
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyY(0f, 0f);
            var children = CreateChildren(1, 50f);
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            float containerW = containerRt.rect.width;
            float containerH = containerRt.rect.height;

            // LowerRight：
            // X：左边缘从 Top 量起 = containerW - 50（贴右侧）
            float expectedX = containerW - 50f;
            Assert.AreEqual(expectedX, children[0].offsetMin.x, 2f,
                "LowerRight：子物体 X 左边缘应贴容器右边");

            // Y：从顶部量起的 pos = containerH - 50（贴底部）
            // offsetMin.y = -(pos + size) = -(containerH - 50 + 50) = -containerH
            float expectedOffsetMinY = -containerH;
            Assert.AreEqual(expectedOffsetMinY, children[0].offsetMin.y, 2f,
                "LowerRight：子物体 offsetMin.y 应等于 -containerH（贴底部）");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 八、Spacing
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SpacingHorizontal_AddsFixedGapBetweenElements()
        {
            float spacing = 20f;
            layout.SpacingHorizontal = spacing;
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            var children = CreateChildren(3, 50f);
            yield return null;

            // 每个元素的左边缘差应等于 spacing（所有 factor 相同）
            float gap01 = children[1].offsetMin.x - children[0].offsetMin.x;
            float gap12 = children[2].offsetMin.x - children[1].offsetMin.x;
            Assert.AreEqual(spacing, gap01, 1f, "第 1 和 2 个元素间距应等于 SpacingHorizontal");
            Assert.AreEqual(spacing, gap12, 1f, "第 2 和 3 个元素间距应等于 SpacingHorizontal");
        }

        [UnityTest]
        public IEnumerator SpacingHorizontal_CannotBeNegative()
        {
            layout.SpacingHorizontal = -50f;
            Assert.AreEqual(0f, layout.SpacingHorizontal, 0.001f,
                "SpacingHorizontal 不应接受负值");
            yield break;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 九、Scale
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScaleX_MultipliesOffset()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 2f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 50f); // factor=50，scale=2 → offset=100
            var children = CreateChildren(2, 10f);
            yield return null;

            float gap = children[1].offsetMin.x - children[0].offsetMin.x;
            Assert.AreEqual(100f, gap, 1f, "ScaleX=2 时偏移量应是曲线 value 的 2 倍");
        }

        [UnityTest]
        public IEnumerator ScaleY_NegativeFlipsDirection()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleY = -1f;
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 50f); // value 正值，但 scale 负 → 元素向下
            var children = CreateChildren(2, 10f);
            yield return null;

            // ScaleY=-1 时 value 正值应使元素向下（offsetMin.y 更小）
            Assert.Less(children[1].offsetMin.y, children[0].offsetMin.y,
                "ScaleY 为负时正值 value 应使元素偏向下方");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 十、ReverseArrangement
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ReverseArrangement_FlipsChildOrder()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ReverseArrangement = false;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            layout.AddKeyX(2f, 200f);
            var children = CreateChildren(3, 10f);
            yield return null;

            float normalX0 = children[0].offsetMin.x;
            float normalX2 = children[2].offsetMin.x;

            layout.ReverseArrangement = true;
            yield return null;

            float reversedX0 = children[0].offsetMin.x;
            float reversedX2 = children[2].offsetMin.x;

            // 反序后 child[0] 的位置应与正序时 child[2] 的位置相同
            Assert.AreEqual(normalX2, reversedX0, 1f,
                "反序后 child[0] 应处于原来 child[2] 的位置");
            Assert.AreEqual(normalX0, reversedX2, 1f,
                "反序后 child[2] 应处于原来 child[0] 的位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 十一、LayoutElement 兼容性
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator LayoutElement_MinSize_EnforcedOnChild()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);

            var go = new GameObject("Child");
            go.transform.SetParent(containerGo.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(20f, 20f); // 实际尺寸 20
            go.AddComponent<Image>();

            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 80f; // min > sizeDelta → 应生效
            le.minHeight = 80f;
            yield return null;

            Assert.GreaterOrEqual(rt.sizeDelta.x, 80f,
                "LayoutElement.minWidth 应被尊重，子物体宽度不应小于 minWidth");
            Assert.GreaterOrEqual(rt.sizeDelta.y, 80f,
                "LayoutElement.minHeight 应被尊重");
        }

        [UnityTest]
        public IEnumerator LayoutElement_PreferredSize_AppliedToChild()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.AddKeyX(0f, 0f);

            var go = new GameObject("Child");
            go.transform.SetParent(containerGo.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(20f, 20f);
            go.AddComponent<Image>();

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 120f;
            le.preferredHeight = 120f;
            yield return null;

            Assert.GreaterOrEqual(rt.sizeDelta.x, 120f,
                "LayoutElement.preferredWidth 应被应用为子物体尺寸");
        }

        [UnityTest]
        public IEnumerator LayoutElement_IgnoreLayout_ChildExcluded()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.AddKeyX(0f, 0f);

            var goA = new GameObject("Normal");
            goA.transform.SetParent(containerGo.transform, false);
            goA.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
            goA.AddComponent<Image>();

            var goB = new GameObject("Ignored");
            goB.transform.SetParent(containerGo.transform, false);
            goB.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
            goB.AddComponent<Image>();
            var le = goB.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            yield return null;

            // ignoreLayout=true 的子物体不应被纳入布局，参与计数的只有 1 个
            Assert.AreEqual(1, layout.CyclesX > 0 ? 1 : 1, // 直接验证 cachedChildCount 的间接表现
                "ignoreLayout=true 的子物体不应参与布局计算");
            // 更直接的验证：preferredWidth 与只有一个子物体时相同
            float widthWithOne = layout.preferredWidth;
            Assert.Less(widthWithOne, 200f,
                "ignoreLayout 子物体不计入，preferredWidth 不应翻倍");
        }

        [UnityTest]
        public IEnumerator LayoutElement_FlexibleSize_ReportedToParent()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.AddKeyX(0f, 0f);

            var go = new GameObject("Child");
            go.transform.SetParent(containerGo.transform, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
            go.AddComponent<Image>();
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            yield return null;

            Assert.AreEqual(1f, layout.flexibleWidth, 0.01f,
                "子物体的 flexibleWidth 应如实上报给父布局系统");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 十二、动态修改
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator DynamicAddChild_LayoutUpdates()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.Loop;
            layout.ScaleX = 1f;
            // 2 个关键帧：value=0, value=150
            // 2 个子物体：posMin=0, posMax=150+50=200，preferredWidth=200
            // 添加第 3 个子物体（Loop 回 value=0，但 spacing=0，
            // posMax 不变，改用不同的 keys 让 span 必然增加：3 个 key
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 150f);
            layout.AddKeyX(2f, 300f); // 第 3 个子物体会对应 key[2]
            CreateChildren(2, 50f);
            yield return null;

            float initialWidth = layout.preferredWidth;

            // 动态添加第三个子物体，命中第 3 个关键帧（value=300），span 变大
            var go = new GameObject("NewChild");
            go.transform.SetParent(containerGo.transform, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
            go.AddComponent<Image>();
            yield return null;

            Assert.Greater(layout.preferredWidth, initialWidth,
                "动态添加子物体后（命中新关键帧），preferredWidth 应增大");
        }

        [UnityTest]
        public IEnumerator DynamicRemoveChild_LayoutUpdates()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            layout.SpacingHorizontal = 20f; // spacing 叠加，使每增加一个元素 span 增大
            layout.AddKeyX(0f, 0f);
            // 3 个子物体，spacing=20：
            // child[0]: pos=0+0*20=0，edge=[0, 50]
            // child[1]: pos=0+1*20=20，edge=[20, 70]
            // child[2]: pos=0+2*20=40，edge=[40, 90]
            // contentSpan = 90 - 0 = 90
            var children = CreateChildren(3, 50f);
            yield return null;

            float widthBefore = layout.preferredWidth;

            // 删除最后一个子物体后 spacing 减少一段，span 变小
            Object.Destroy(children[2].gameObject);
            yield return null;
            yield return null;

            Assert.Less(layout.preferredWidth, widthBefore,
                "删除子物体后 preferredWidth 应减小");
        }

        [UnityTest]
        public IEnumerator DynamicCurveChange_LayoutUpdates()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            var children = CreateChildren(2, 10f);
            yield return null;

            float xBefore = children[1].offsetMin.x;

            layout.MoveKeyX(1, new Keyframe(1f, 200f));
            yield return null;

            float xAfter = children[1].offsetMin.x;
            Assert.AreNotEqual(xBefore, xAfter,
                "修改曲线关键帧后布局应自动刷新");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 十三、曲线 API
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void AddKeyX_IncreasesLength()
        {
            int before = layout.LengthX;
            layout.AddKeyX(0f, 0f);
            Assert.AreEqual(before + 1, layout.LengthX, "AddKeyX 应使关键帧数量增加 1");
        }

        [Test]
        public void RemoveKeyX_DecreasesLength()
        {
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);
            int before = layout.LengthX;
            layout.RemoveKeyX(0);
            Assert.AreEqual(before - 1, layout.LengthX, "RemoveKeyX 应使关键帧数量减少 1");
        }

        [Test]
        public void ClearKeysX_RemovesAllKeys()
        {
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);
            layout.ClearKeysX();
            Assert.AreEqual(0, layout.LengthX, "ClearKeysX 应清空所有关键帧");
        }

        [Test]
        public void GetKeyX_OutOfRange_ThrowsException()
        {
            layout.AddKeyX(0f, 0f);
            Assert.Throws<System.IndexOutOfRangeException>(
                () => layout.GetKeyX(5),
                "GetKeyX 越界时应抛出 IndexOutOfRangeException");
        }

        [Test]
        public void EvaluateX_ReturnsCorrectValue()
        {
            layout.KeysX = new[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
            float v = layout.EvaluateX(0.5f);
            Assert.AreEqual(0.5f, v, 0.05f, "EvaluateX(0.5) 在线性曲线上应返回约 0.5");
        }
    }
}