using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// 兼容性补充测试
    /// </summary>
    public class LayoutCompatibilitySupplementTests
    {
        private GameObject canvasGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(canvasGo);
        }

        private GameObject CreateRectGO(string name, Transform parent, float w = 200f, float h = 200f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(w, h);
            return go;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 一、AutoLayout 三级嵌套
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_TripleNested_OutermostReportsCorrectSize()
        {
            // 外层 VerticalLayoutGroup → 中层 AutoLayout → 内层子物体
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 400f, 600f);
            var vlg = outerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var midGo = CreateRectGO("Mid", outerGo.transform, 300f, 150f);
            var al = midGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 100f);

            var csf = midGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 2; i++)
            {
                var c = CreateRectGO($"Inner_{i}", midGo.transform, 50f, 50f);
                c.AddComponent<Image>();
            }

            yield return null;
            yield return null;

            // AutoLayout 的 preferredWidth 应大于 0，VLG 能正确驱动 mid 宽度
            Assert.Greater(al.preferredWidth, 0f,
                "三级嵌套：AutoLayout 应向父级 VLG 上报正确的 preferredWidth");
            Assert.Greater(midGo.GetComponent<RectTransform>().rect.width, 50f,
                "三级嵌套：ContentSizeFitter 应将中层容器扩展到 preferredWidth");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、ContentSizeFitter = MinSize 模式
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ContentSizeFitter_MinSize_ResizesToMinWidth()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 50f, 50f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 200f);

            var csf = containerGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.MinSize;

            // 子物体挂 LayoutElement 并设 minWidth
            for (int i = 0; i < 2; i++)
            {
                var c = CreateRectGO($"C{i}", containerGo.transform, 30f, 30f);
                c.AddComponent<Image>();
                var le = c.AddComponent<LayoutElement>();
                le.minWidth = 60f;
            }

            yield return null;
            yield return null;

            var rt = containerGo.GetComponent<RectTransform>();
            // minWidth = 累加所有子物体 minWidth + padding = 60+60 = 120
            // 但包围盒 span 也可能更大，取二者较大值
            Assert.GreaterOrEqual(rt.sizeDelta.x, 120f,
                "ContentSizeFitter(MinSize)：容器宽度应至少等于所有子物体 minWidth 之和");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、preferredHeight 精确分配给 VerticalLayoutGroup
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_PreferredHeight_CorrectlyAllocatedByVLG()
        {
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 300f, 500f);
            var vlg = outerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 0f;

            // 第一个子容器：AutoLayout，Y 轴有曲线
            var alGo = CreateRectGO("AL", outerGo.transform, 200f, 100f);
            var al = alGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleY = 1f;
            // 两个关键帧：key0(0), key1(80)
            // child[0](factor=0): edge=0,        posMax=50
            // child[1](factor=80): edge=-80,      posMin=-80, posMax_with_size=-30
            // contentSpan = posMax - posMin = 50 - (-80) = 130 → preferredHeight > 50
            al.AddKeyY(0f, 0f);
            al.AddKeyY(1f, 80f);

            var csf = alGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 2; i++)
            {
                var c = CreateRectGO($"C{i}", alGo.transform, 50f, 50f);
                c.AddComponent<Image>();
            }

            // 第二个子容器：普通 Image，固定高度 60
            var imgGo = CreateRectGO("Img", outerGo.transform, 200f, 60f);
            imgGo.AddComponent<Image>();
            var le = imgGo.AddComponent<LayoutElement>();
            le.preferredHeight = 60f;

            yield return null;
            yield return null;

            // AutoLayout 容器的高度应由 ContentSizeFitter 驱动为 preferredHeight
            float alHeight = alGo.GetComponent<RectTransform>().rect.height;
            Assert.Greater(alHeight, 50f,
                "AutoLayout 容器高度应被 ContentSizeFitter 扩展到 preferredHeight（>初始50px）");

            // Image 容器应固定在 60px
            float imgHeight = imgGo.GetComponent<RectTransform>().rect.height;
            Assert.AreEqual(60f, imgHeight, 2f,
                "普通 Image 容器高度应等于 preferredHeight（60px）");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、AutoLayout 嵌套 AutoLayout
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_NestedInAutoLayout_BothLayout()
        {
            // 外层 AutoLayout 水平排列两个子容器
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 600f, 300f);
            var outerAl = outerGo.AddComponent<AutoLayoutGroup>();
            outerAl.MappingModeX = KeyframeMappingMode.Direct;
            outerAl.MappingModeY = KeyframeMappingMode.Direct;
            outerAl.PositionModeX = PositionMode.ByPixel;
            outerAl.PositionModeY = PositionMode.ByPixel;
            outerAl.ScaleX = 1f;
            outerAl.AddKeyX(0f, 0f);
            outerAl.AddKeyX(1f, 250f);

            // 内层 AutoLayout A
            var innerAGo = CreateRectGO("InnerA", outerGo.transform, 200f, 200f);
            var innerAl = innerAGo.AddComponent<AutoLayoutGroup>();
            innerAl.MappingModeX = KeyframeMappingMode.Direct;
            innerAl.MappingModeY = KeyframeMappingMode.Direct;
            innerAl.PositionModeX = PositionMode.ByPixel;
            innerAl.PositionModeY = PositionMode.ByPixel;
            innerAl.AddKeyX(0f, 0f);
            var c0 = CreateRectGO("C0", innerAGo.transform, 40f, 40f);
            c0.AddComponent<Image>();

            // 内层 AutoLayout B
            var innerBGo = CreateRectGO("InnerB", outerGo.transform, 200f, 200f);
            var innerBl = innerBGo.AddComponent<AutoLayoutGroup>();
            innerBl.MappingModeX = KeyframeMappingMode.Direct;
            innerBl.MappingModeY = KeyframeMappingMode.Direct;
            innerBl.PositionModeX = PositionMode.ByPixel;
            innerBl.PositionModeY = PositionMode.ByPixel;
            innerBl.AddKeyX(0f, 0f);
            var c1 = CreateRectGO("C1", innerBGo.transform, 40f, 40f);
            c1.AddComponent<Image>();

            yield return null;

            // 外层 AutoLayout 应将两个内层容器分开排布
            float xA = innerAGo.GetComponent<RectTransform>().offsetMin.x;
            float xB = innerBGo.GetComponent<RectTransform>().offsetMin.x;
            Assert.Greater(xB, xA,
                "外层 AutoLayout：第二个内层容器应在第一个右侧");

            // 内层 AutoLayout 也应正常工作
            Assert.Greater(innerAl.preferredWidth, 0f,
                "内层 AutoLayout A 应上报正确的 preferredWidth");
            Assert.Greater(innerBl.preferredWidth, 0f,
                "内层 AutoLayout B 应上报正确的 preferredWidth");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、多个 LayoutElement.ignoreLayout 子物体
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator MultipleIgnoredChildren_OnlyNormalChildrenLayout()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 100f);

            // 3 个正常子物体
            for (int i = 0; i < 3; i++)
            {
                var c = CreateRectGO($"Normal_{i}", containerGo.transform, 30f, 30f);
                c.AddComponent<Image>();
            }

            // 2 个被忽略的子物体
            for (int i = 0; i < 2; i++)
            {
                var c = CreateRectGO($"Ignored_{i}", containerGo.transform, 30f, 30f);
                c.AddComponent<Image>();
                var le = c.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }

            yield return null;

            // preferredWidth 应只反映 3 个正常子物体的布局结果
            // 3 个子物体：key0(0), key1(100), key2 超出→Clamp(100) → span=100+30=130
            Assert.Less(al.preferredWidth, 300f,
                "5 个子物体中 2 个 ignoreLayout，preferredWidth 应只反映 3 个正常子物体");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 六、ReverseArrangement + LayoutElement 联合
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ReverseArrangement_WithLayoutElement_CorrectSizeApplied()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 500f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByElementSize;
            al.PositionModeY = PositionMode.ByElementSize;
            al.ScaleX = 1f;
            al.ReverseArrangement = true;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 1f);

            // child[0]：小元素 sizeDelta=30，child[1]：大元素 sizeDelta=80
            var c0 = CreateRectGO("Small", containerGo.transform, 30f, 30f);
            c0.AddComponent<Image>();

            var c1 = CreateRectGO("Large", containerGo.transform, 80f, 30f);
            c1.AddComponent<Image>();
            var le = c1.AddComponent<LayoutElement>();
            le.minWidth = 80f;

            yield return null;

            var rt0 = c0.GetComponent<RectTransform>();
            var rt1 = c1.GetComponent<RectTransform>();

            // ReverseArrangement：child[0]对应key1(factor=1)，child[1]对应key0(factor=0)
            // child[0](factor=1, size=30): pos = 30*1*1 = 30
            // child[1](factor=0, size=80): pos = 0
            // child[1].offsetMin.x < child[0].offsetMin.x
            Assert.Less(rt1.offsetMin.x, rt0.offsetMin.x,
                "ReverseArrangement：大元素(factor=0)应在小元素(factor=1)左侧");

            // 大元素 minWidth=80 应被尊重
            Assert.GreaterOrEqual(rt1.sizeDelta.x, 80f,
                "ReverseArrangement 不影响 LayoutElement.minWidth 的生效");
        }
    }
}
