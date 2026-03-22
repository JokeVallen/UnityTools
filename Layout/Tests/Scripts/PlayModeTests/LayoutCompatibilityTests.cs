using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// 与 UGUI 官方布局系统的兼容性测试
    /// </summary>
    public class LayoutCompatibilityTests
    {
        private GameObject canvasGo;
        private Canvas canvas;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
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

        private GameObject CreateRectGO(string name, Transform parent,
            float w = 200f, float h = 200f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(w, h);
            return go;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 一、AutoLayout 嵌套在 VerticalLayoutGroup 中
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_NestedInVerticalGroup_ReportsPreferredSize()
        {
            // 外层：VerticalLayoutGroup
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 400f, 600f);
            var vlg = outerGo.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // 内层：AutoLayout
            var innerGo = CreateRectGO("Inner", outerGo.transform, 300f, 200f);
            var al = innerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 100f);

            // 子物体
            for (int i = 0; i < 3; i++)
            {
                var child = CreateRectGO($"C{i}", innerGo.transform, 50f, 50f);
                child.AddComponent<Image>();
            }

            yield return null;

            // AutoLayout 应向父级 VerticalLayoutGroup 上报合理的 preferredHeight
            Assert.Greater(al.preferredHeight, 0f,
                "AutoLayout 嵌套在 VerticalLayoutGroup 中时应上报正确的 preferredHeight");
            // 外层尺寸应受内层影响
            Assert.Greater(outerGo.GetComponent<RectTransform>().rect.height, 0f);
        }

        [UnityTest]
        public IEnumerator AutoLayout_NestedInHorizontalGroup_PositionedByParent()
        {
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 600f, 200f);
            var hlg = outerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10f;

            // 第一个子元素：普通 Image
            var sibling = CreateRectGO("Sibling", outerGo.transform, 100f, 200f);
            sibling.AddComponent<Image>();

            // 第二个子元素：AutoLayout
            var alGo = CreateRectGO("AutoLayout", outerGo.transform, 200f, 200f);
            var al = alGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.AddKeyX(0f, 0f);

            var child = CreateRectGO("C0", alGo.transform, 50f, 50f);
            child.AddComponent<Image>();

            yield return null;

            // AutoLayout 容器的左边缘应在 Sibling 右边缘之后（HLG 排列）
            float siblingRight = sibling.GetComponent<RectTransform>().offsetMin.x
                               + sibling.GetComponent<RectTransform>().sizeDelta.x;
            float alLeft = alGo.GetComponent<RectTransform>().offsetMin.x;
            Assert.GreaterOrEqual(alLeft, siblingRight - 1f,
                "AutoLayout 容器应被 HorizontalLayoutGroup 正确定位在兄弟元素之后");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、ContentSizeFitter 驱动 AutoLayout 容器尺寸
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ContentSizeFitter_OnAutoLayout_ResizesContainer()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 100f, 100f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            var csf = containerGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 150f);
            al.AddKeyX(2f, 300f);

            for (int i = 0; i < 3; i++)
            {
                var c = CreateRectGO($"C{i}", containerGo.transform, 50f, 50f);
                c.AddComponent<Image>();
            }

            yield return null;
            yield return null; // ContentSizeFitter 可能需要额外一帧

            var rt = containerGo.GetComponent<RectTransform>();
            Assert.Greater(rt.sizeDelta.x, 100f,
                "ContentSizeFitter 应将容器宽度扩展到 preferredWidth（>初始100px）");
        }

        [UnityTest]
        public IEnumerator ContentSizeFitter_OnChildOfAutoLayout_SizeRespected()
        {
            // 子物体挂 ContentSizeFitter，AutoLayout 应尊重其最终 sizeDelta
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByElementSize;
            al.PositionModeY = PositionMode.ByElementSize;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 1f);

            var childGo = CreateRectGO("Child", containerGo.transform, 50f, 50f);
            childGo.AddComponent<Image>();
            var le = childGo.AddComponent<LayoutElement>();
            le.preferredWidth = 120f; // AutoLayout 应将子物体驱动到 120px
            le.preferredHeight = 80f;

            yield return null;
            yield return null;

            var childRt = childGo.GetComponent<RectTransform>();
            Assert.AreEqual(120f, childRt.sizeDelta.x, 1f,
                "AutoLayout 应将子物体宽度驱动到 LayoutElement.preferredWidth");
            Assert.AreEqual(80f, childRt.sizeDelta.y, 1f,
                "AutoLayout 应将子物体高度驱动到 LayoutElement.preferredHeight");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、LayoutElement.ignoreLayout 跳过测试
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator IgnoreLayout_Child_NotAffectingPreferredSize()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 0f);
            al.AddKeyX(1f, 200f);

            // 正常子物体
            var normal = CreateRectGO("Normal", containerGo.transform, 50f, 50f);
            normal.AddComponent<Image>();
            yield return null;

            float widthBefore = al.preferredWidth;

            // 添加一个 ignoreLayout 子物体
            var ignored = CreateRectGO("Ignored", containerGo.transform, 200f, 200f);
            ignored.AddComponent<Image>();
            var le = ignored.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            yield return null;

            float widthAfter = al.preferredWidth;

            Assert.AreEqual(widthBefore, widthAfter, 1f,
                "ignoreLayout=true 的子物体不应影响 AutoLayout 的 preferredWidth");
        }

        [UnityTest]
        public IEnumerator IgnoreLayout_Child_NotPositionedByLayout()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.AddKeyX(0f, 100f);

            var ignoredGo = CreateRectGO("Ignored", containerGo.transform, 50f, 50f);
            ignoredGo.AddComponent<Image>();
            var le = ignoredGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            // 手动设置位置，布局不应改变它
            var ignoredRt = ignoredGo.GetComponent<RectTransform>();
            ignoredRt.anchoredPosition = new Vector2(999f, 999f);
            yield return null;

            Assert.AreEqual(999f, ignoredRt.anchoredPosition.x, 1f,
                "ignoreLayout=true 的子物体位置不应被 AutoLayout 驱动");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、DrivenRectTransformTracker — 布局接管验证
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_DrivesChildPosition_RestoredOnRebuild()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 50f);

            var childGo = CreateRectGO("Child", containerGo.transform, 40f, 40f);
            childGo.AddComponent<Image>();
            yield return null;

            var childRt = childGo.GetComponent<RectTransform>();
            float layoutX = childRt.offsetMin.x; // 布局计算值（50）

            // DrivenRectTransformTracker 在 PlayMode 下不会自动阻止修改，
            // 但布局系统下次执行 SetLayout 时会将接管的属性重新驱动到布局值。
            // 手动修改位置后，主动触发布局刷新来验证驱动行为。
            childRt.anchoredPosition = new Vector2(999f, childRt.anchoredPosition.y);
            // 触发布局刷新
            al.RebuildLayout();
            yield return null;

            // 布局系统执行 SetLayout 后，子物体位置应恢复到布局计算值
            Assert.AreEqual(layoutX, childRt.offsetMin.x, 1f,
                "布局系统重建后，子物体位置应被还原到布局计算值");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、CircleLayout 与 ContentSizeFitter 配合
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator CircleLayout_ContentSizeFitter_ResizesToRadius()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 100f, 100f);
            var cl = containerGo.AddComponent<CircleLayoutGroup>();
            cl.Radius = 150f;
            var csf = containerGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < 4; i++)
            {
                var c = CreateRectGO($"C{i}", containerGo.transform, 20f, 20f);
                c.AddComponent<Image>();
            }

            yield return null;
            yield return null;

            var rt = containerGo.GetComponent<RectTransform>();
            Assert.AreEqual(cl.Radius * 2f, rt.sizeDelta.x, 2f,
                "ContentSizeFitter + CircleLayout：容器宽度应等于 radius * 2");
            Assert.AreEqual(cl.Radius * 2f, rt.sizeDelta.y, 2f,
                "ContentSizeFitter + CircleLayout：容器高度应等于 radius * 2");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 六、AutoLayout 与 CircleLayout 同级嵌套在同一父布局中
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_And_CircleLayout_InSameHorizontalGroup()
        {
            var outerGo = CreateRectGO("Outer", canvasGo.transform, 800f, 300f);
            var hlg = outerGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // AutoLayout 子容器
            var alGo = CreateRectGO("AL", outerGo.transform, 300f, 200f);
            var al = alGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.AddKeyX(0f, 0f);
            for (int i = 0; i < 2; i++)
            {
                var c = CreateRectGO($"AC{i}", alGo.transform, 50f, 50f);
                c.AddComponent<Image>();
            }

            // CircleLayout 子容器
            var clGo = CreateRectGO("CL", outerGo.transform, 200f, 200f);
            var cl = clGo.AddComponent<CircleLayoutGroup>();
            cl.Radius = 80f;
            for (int i = 0; i < 4; i++)
            {
                var c = CreateRectGO($"CC{i}", clGo.transform, 20f, 20f);
                c.AddComponent<Image>();
            }

            yield return null;

            // 两个子容器都应有合理的 preferredWidth
            Assert.Greater(al.preferredWidth, 0f,
                "HLG 下 AutoLayout 的 preferredWidth 应大于 0");
            Assert.Greater(cl.preferredWidth, 0f,
                "HLG 下 CircleLayout 的 preferredWidth 应大于 0");

            // CircleLayout 容器的左边缘应在 AutoLayout 容器右边缘之后
            float alRight = alGo.GetComponent<RectTransform>().offsetMin.x
                          + alGo.GetComponent<RectTransform>().rect.width;
            float clLeft = clGo.GetComponent<RectTransform>().offsetMin.x;
            Assert.GreaterOrEqual(clLeft, alRight - 1f,
                "CircleLayout 容器应排列在 AutoLayout 容器之后");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 七、LayoutElement.flexibleWidth 上报给父布局
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_FlexibleWidth_SummedFromChildren()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.AddKeyX(0f, 0f);

            for (int i = 0; i < 3; i++)
            {
                var c = CreateRectGO($"C{i}", containerGo.transform, 50f, 50f);
                c.AddComponent<Image>();
                var le = c.AddComponent<LayoutElement>();
                le.flexibleWidth = 1f;
            }

            yield return null;

            Assert.AreEqual(3f, al.flexibleWidth, 0.01f,
                "AutoLayout.flexibleWidth 应等于所有子物体 flexibleWidth 之和");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 八、AutoLayout 禁用/启用时布局系统响应
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator AutoLayout_Disabled_ChildrenNotDriven()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 80f);

            var childGo = CreateRectGO("Child", containerGo.transform, 50f, 50f);
            childGo.AddComponent<Image>();
            yield return null;

            var childRt = childGo.GetComponent<RectTransform>();
            float xBefore = childRt.offsetMin.x;

            // 禁用 AutoLayout
            al.enabled = false;
            yield return null;

            // 手动移动子物体（此时不应被接管）
            childRt.anchoredPosition = new Vector2(0f, 0f);
            yield return null;

            // 禁用后子物体的位置应可以自由修改
            Assert.AreNotEqual(xBefore, childRt.offsetMin.x,
                "AutoLayout 禁用后子物体位置应不再被接管");
        }

        [UnityTest]
        public IEnumerator AutoLayout_ReEnabled_ChildrenReposistioned()
        {
            var containerGo = CreateRectGO("Container", canvasGo.transform, 400f, 200f);
            var al = containerGo.AddComponent<AutoLayoutGroup>();
            al.MappingModeX = KeyframeMappingMode.Direct;
            al.MappingModeY = KeyframeMappingMode.Direct;
            al.PositionModeX = PositionMode.ByPixel;
            al.PositionModeY = PositionMode.ByPixel;
            al.ScaleX = 1f;
            al.AddKeyX(0f, 80f);

            var childGo = CreateRectGO("Child", containerGo.transform, 50f, 50f);
            childGo.AddComponent<Image>();
            yield return null;

            var childRt = childGo.GetComponent<RectTransform>();
            float xLayout = childRt.offsetMin.x;

            al.enabled = false;
            yield return null;
            childRt.anchoredPosition = new Vector2(999f, 0f);
            yield return null;

            // 重新启用，子物体应回到布局计算的位置
            al.enabled = true;
            yield return null;

            Assert.AreEqual(xLayout, childRt.offsetMin.x, 2f,
                "AutoLayout 重新启用后子物体应恢复到布局计算的位置");
        }
    }
}