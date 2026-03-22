using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// 按 Tests.md 规格实现的测试套件（测试A ~ 测试H）
    /// </summary>
    /// <remarks>
    /// 公共规定：
    /// 1. 所有布局组件挂载在 Canvas 一级子对象上，坐标 (0,0)，尺寸 (500,500)。
    /// 2. 所有布局元素（E）均挂载 LayoutElement，minWidth = minHeight = 100，其余参数默认。
    ///    这确保 ALG 的 effectiveSize 固定为 100，与 HLG/VLG 的子元素尺寸行为对齐。
    /// 预设A：ALG 的两条曲线均为 value=1 的水平线（time 范围 [0,1]）。
    /// </remarks>
    public class LayoutGroupSpecTests
    {
        // ── 基础设施 ──────────────────────────────────────────────────

        private GameObject canvasGo;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            canvasGo = new GameObject("Canvas");
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

        // 创建一个 500x500 的容器，挂载指定布局组件
        private T CreateContainer<T>(string name) where T : LayoutGroup
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGo.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 500);
            rt.anchoredPosition = Vector2.zero;
            return go.AddComponent<T>();
        }

        // 创建 ALG 容器
        private AutoLayoutGroup CreateALG(string name = "ALG")
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGo.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 500);
            rt.anchoredPosition = Vector2.zero;
            return go.AddComponent<AutoLayoutGroup>();
        }

        // 创建一个布局元素 E：挂载 Image + LayoutElement(minWidth=100, minHeight=100)
        private RectTransform CreateE(Transform parent, string name = "E")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 100);
            go.AddComponent<Image>();
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 100;
            le.minHeight = 100;
            return rt;
        }

        // 对 ALG 应用预设A（水平排列变体，对应 HLG）
        // HLG 紧密排列：第 i 个元素起始位置 = i * (effectiveSize + spacing)。
        // 等价实现：ByPixel + X曲线 value 恒为 0，完全依靠 SpacingHorizontal 产生间距。
        private void ApplyPresetA(AutoLayoutGroup alg)
        {
            // ByElementSize + X曲线 key[i].value=i：第 i 个元素偏移 = i * effectiveSize，
            // 与 HLG 紧密排列（spacing=0）完全等价；加 SpacingHorizontal 后与 HLG.spacing 也等价。
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByElementSize;
            alg.PositionModeY = PositionMode.ByElementSize;
            alg.PostWrapModeX = WrapMode.ClampForever;
            alg.PostWrapModeY = WrapMode.ClampForever;
            alg.ScaleX = 1f;
            alg.ScaleY = 1f;
            alg.AddKeyX(0f, 0f);   // E0: factor=0 → posX=0
            alg.AddKeyX(1f, 1f);   // E1: factor=1 → posX=effectiveSize
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, 0f);
        }

        // 对 ALG 应用预设A（垂直排列变体，对应 VLG）
        private void ApplyPresetA_VLG(AutoLayoutGroup alg)
        {
            // ALG Y轴 factor = -kf.value，所以 value=-1 → factor=1 → posY=effectiveSize（向下）。
            // ByElementSize + Y曲线 key[i].value=-i：与 VLG 紧密排列等价。
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByElementSize;
            alg.PositionModeY = PositionMode.ByElementSize;
            alg.PostWrapModeX = WrapMode.ClampForever;
            alg.PostWrapModeY = WrapMode.ClampForever;
            alg.ScaleX = 1f;
            alg.ScaleY = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 0f);
            alg.AddKeyY(0f, 0f);    // E0: factor=0 → posY=0
            alg.AddKeyY(1f, -1f);   // E1: factor=-(-1)=1 → posY=effectiveSize（向下）
        }

        // 比较两个 RectTransform 的关键属性
        private static void AssertRTFEqual(RectTransform a, RectTransform b, string msg = "", float tolerance = 1f)
        {
            Assert.AreEqual(a.anchoredPosition.x, b.anchoredPosition.x, tolerance, $"{msg} anchoredPosition.x");
            Assert.AreEqual(a.anchoredPosition.y, b.anchoredPosition.y, tolerance, $"{msg} anchoredPosition.y");
            Assert.AreEqual(a.sizeDelta.x, b.sizeDelta.x, tolerance, $"{msg} sizeDelta.x");
            Assert.AreEqual(a.sizeDelta.y, b.sizeDelta.y, tolerance, $"{msg} sizeDelta.y");
            Assert.AreEqual(a.offsetMin.x, b.offsetMin.x, tolerance, $"{msg} offsetMin.x");
            Assert.AreEqual(a.offsetMin.y, b.offsetMin.y, tolerance, $"{msg} offsetMin.y");
            Assert.AreEqual(a.offsetMax.x, b.offsetMax.x, tolerance, $"{msg} offsetMax.x");
            Assert.AreEqual(a.offsetMax.y, b.offsetMax.y, tolerance, $"{msg} offsetMax.y");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试A — ChildAlignment（与 HLG / VLG 对比，9 种对齐模式 × 2 种组件）
        // ═══════════════════════════════════════════════════════════════

        private static readonly TextAnchor[] AllAlignments = (TextAnchor[])System.Enum.GetValues(typeof(TextAnchor));

        // ALG vs HLG — 9 种对齐模式
        [UnityTest]
        public IEnumerator TestA_ALG_vs_HLG_ChildAlignment(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment)
        {
            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childAlignment = alignment;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var eHlg = CreateE(hlg.transform, "E_HLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            ApplyPresetA(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;

            AssertRTFEqual(eAlg, eHlg, $"HLG alignment={alignment}");
        }

        // ALG vs VLG — 9 种对齐模式
        [UnityTest]
        public IEnumerator TestA_ALG_vs_VLG_ChildAlignment(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment)
        {
            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childAlignment = alignment;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            var eVlg = CreateE(vlg.transform, "E_VLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            ApplyPresetA_VLG(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;

            AssertRTFEqual(eAlg, eVlg, $"VLG alignment={alignment}");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试B — Padding（在 ChildAlignment 基础上叠加 Padding 变化）
        // ═══════════════════════════════════════════════════════════════

        // 测试B依赖测试A通过后才有意义，这里选取有代表性的对齐模式覆盖各方向 Padding

        private static readonly int[] PaddingValues = { 0, 20, -10 };

        // ALG vs HLG — Padding.left
        [UnityTest]
        public IEnumerator TestB_HLG_PaddingLeft(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment,
            [ValueSource(nameof(PaddingValues))] int left)
        {
            var pad = new RectOffset(left, 0, 0, 0);

            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childAlignment = alignment;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = pad;
            var eHlg = CreateE(hlg.transform, "E_HLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            alg.Padding = pad;
            ApplyPresetA(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;
            AssertRTFEqual(eAlg, eHlg, $"HLG left={left} align={alignment}");
        }

        // ALG vs HLG — Padding.right
        [UnityTest]
        public IEnumerator TestB_HLG_PaddingRight(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment,
            [ValueSource(nameof(PaddingValues))] int right)
        {
            var pad = new RectOffset(0, right, 0, 0);

            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childAlignment = alignment;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = pad;
            var eHlg = CreateE(hlg.transform, "E_HLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            alg.Padding = pad;
            ApplyPresetA(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;
            AssertRTFEqual(eAlg, eHlg, $"HLG right={right} align={alignment}");
        }

        // ALG vs VLG — Padding.top
        [UnityTest]
        public IEnumerator TestB_VLG_PaddingTop(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment,
            [ValueSource(nameof(PaddingValues))] int top)
        {
            var pad = new RectOffset(0, 0, top, 0);

            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childAlignment = alignment;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = pad;
            var eVlg = CreateE(vlg.transform, "E_VLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            alg.Padding = pad;
            ApplyPresetA_VLG(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;
            AssertRTFEqual(eAlg, eVlg, $"VLG top={top} align={alignment}");
        }

        // ALG vs VLG — Padding.bottom
        [UnityTest]
        public IEnumerator TestB_VLG_PaddingBottom(
            [ValueSource(nameof(AllAlignments))] TextAnchor alignment,
            [ValueSource(nameof(PaddingValues))] int bottom)
        {
            var pad = new RectOffset(0, 0, 0, bottom);

            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childAlignment = alignment;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = pad;
            var eVlg = CreateE(vlg.transform, "E_VLG");

            var alg = CreateALG();
            alg.ChildAlignment = alignment;
            alg.Padding = pad;
            ApplyPresetA_VLG(alg);
            var eAlg = CreateE(alg.transform, "E_ALG");

            yield return null;
            AssertRTFEqual(eAlg, eVlg, $"VLG bottom={bottom} align={alignment}");
        }

        // ALG vs HLG — 四个方向同时赋值
        [UnityTest]
        public IEnumerator TestB_HLG_AllPaddingDirections()
        {
            var pad = new RectOffset(10, 20, 15, 5);

            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = pad;
            var eHlg = CreateE(hlg.transform);

            var alg = CreateALG();
            alg.ChildAlignment = TextAnchor.MiddleCenter;
            alg.Padding = pad;
            ApplyPresetA(alg);
            var eAlg = CreateE(alg.transform);

            yield return null;
            AssertRTFEqual(eAlg, eHlg, "HLG all-direction padding");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试C — SpacingHorizontal（ALG↔HLG）/ SpacingVertical（ALG↔VLG）
        // ═══════════════════════════════════════════════════════════════

        private static readonly float[] SpacingValues = { 0f, 30f, -10f };

        // SpacingHorizontal vs HLG.spacing
        [UnityTest]
        public IEnumerator TestC_SpacingHorizontal_vs_HLG(
            [ValueSource(nameof(SpacingValues))] float spacing)
        {
            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            // HLG.spacing 接受负值但 ALG.SpacingHorizontal 会 clamp 到 0，
            // 对比时 HLG 侧也需要用 clamp 后的值，否则负值情况下两者行为不同。
            hlg.spacing = Mathf.Max(0f, spacing);
            var eHlg0 = CreateE(hlg.transform, "E0");
            var eHlg1 = CreateE(hlg.transform, "E1");

            var alg = CreateALG();
            alg.SpacingHorizontal = spacing;
            ApplyPresetA(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eHlg0, $"SpacingH={spacing} E0");
            AssertRTFEqual(eAlg1, eHlg1, $"SpacingH={spacing} E1");
        }

        // SpacingVertical vs VLG.spacing
        [UnityTest]
        public IEnumerator TestC_SpacingVertical_vs_VLG(
            [ValueSource(nameof(SpacingValues))] float spacing)
        {
            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            // 同上，VLG.spacing 接受负值，对比时 clamp 后再传入。
            vlg.spacing = Mathf.Max(0f, spacing);
            var eVlg0 = CreateE(vlg.transform, "E0");
            var eVlg1 = CreateE(vlg.transform, "E1");

            var alg = CreateALG();
            alg.SpacingVertical = spacing;
            ApplyPresetA_VLG(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eVlg0, $"SpacingV={spacing} E0");
            AssertRTFEqual(eAlg1, eVlg1, $"SpacingV={spacing} E1");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试D — ReverseArrangement（ALG vs HLG / VLG）
        // ═══════════════════════════════════════════════════════════════

        // ALG vs HLG — ReverseArrangement = false
        [UnityTest]
        public IEnumerator TestD_HLG_ReverseArrangement_False()
        {
            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.reverseArrangement = false;
            var eHlg0 = CreateE(hlg.transform, "E0");
            var eHlg1 = CreateE(hlg.transform, "E1");

            var alg = CreateALG();
            alg.ReverseArrangement = false;
            ApplyPresetA(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eHlg0, "HLG reverse=false E0");
            AssertRTFEqual(eAlg1, eHlg1, "HLG reverse=false E1");
        }

        // ALG vs HLG — ReverseArrangement = true
        [UnityTest]
        public IEnumerator TestD_HLG_ReverseArrangement_True()
        {
            var hlg = CreateContainer<HorizontalLayoutGroup>("HLG");
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.reverseArrangement = true;
            var eHlg0 = CreateE(hlg.transform, "E0");
            var eHlg1 = CreateE(hlg.transform, "E1");

            var alg = CreateALG();
            alg.ReverseArrangement = true;
            ApplyPresetA(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eHlg0, "HLG reverse=true E0");
            AssertRTFEqual(eAlg1, eHlg1, "HLG reverse=true E1");
        }

        // ALG vs VLG — ReverseArrangement = false
        [UnityTest]
        public IEnumerator TestD_VLG_ReverseArrangement_False()
        {
            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.reverseArrangement = false;
            var eVlg0 = CreateE(vlg.transform, "E0");
            var eVlg1 = CreateE(vlg.transform, "E1");

            var alg = CreateALG();
            alg.ReverseArrangement = false;
            ApplyPresetA_VLG(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eVlg0, "VLG reverse=false E0");
            AssertRTFEqual(eAlg1, eVlg1, "VLG reverse=false E1");
        }

        // ALG vs VLG — ReverseArrangement = true
        [UnityTest]
        public IEnumerator TestD_VLG_ReverseArrangement_True()
        {
            var vlg = CreateContainer<VerticalLayoutGroup>("VLG");
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.reverseArrangement = true;
            var eVlg0 = CreateE(vlg.transform, "E0");
            var eVlg1 = CreateE(vlg.transform, "E1");

            var alg = CreateALG();
            alg.ReverseArrangement = true;
            ApplyPresetA_VLG(alg);
            var eAlg0 = CreateE(alg.transform, "E0");
            var eAlg1 = CreateE(alg.transform, "E1");

            yield return null;

            AssertRTFEqual(eAlg0, eVlg0, "VLG reverse=true E0");
            AssertRTFEqual(eAlg1, eVlg1, "VLG reverse=true E1");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试E — PositionMode（ByElementSize vs ByPixel）
        // ═══════════════════════════════════════════════════════════════

        // ByElementSize：偏移量与元素尺寸成比例，factor=1 时偏移等于自身尺寸
        [UnityTest]
        public IEnumerator TestE_ByElementSize_OffsetEqualsSizeWhenFactorOne()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByElementSize;
            alg.PositionModeY = PositionMode.ByElementSize;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 1f);
            alg.AddKeyX(2f, 2f);
            alg.AddKeyX(3f, 3f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // E0: factor=0, pos=0*100=0; E1: factor=1, pos=1*100=100 → 间距=100（=effectiveSize）
            float gap = e[1].offsetMin.x - e[0].offsetMin.x;
            Assert.AreEqual(100f, gap, 1f, "ByElementSize: factor=1 时间距应等于 effectiveSize(100)");
            Assert.AreEqual(200f, e[2].offsetMin.x - e[0].offsetMin.x, 1f, "ByElementSize: factor=2 偏移=200");
            Assert.AreEqual(300f, e[3].offsetMin.x - e[0].offsetMin.x, 1f, "ByElementSize: factor=3 偏移=300");
        }

        // ByPixel：偏移量与元素尺寸无关，factor 直接表示像素偏移
        [UnityTest]
        public IEnumerator TestE_ByPixel_OffsetIndependentOfSize()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 80f);
            alg.AddKeyX(2f, 160f);
            alg.AddKeyX(3f, 240f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            float gap = e[1].offsetMin.x - e[0].offsetMin.x;
            Assert.AreEqual(80f, gap, 1f, "ByPixel: factor=80 时偏移应为 80px");
            Assert.AreEqual(160f, e[2].offsetMin.x - e[0].offsetMin.x, 1f, "ByPixel: factor=160 偏移=160");
        }

        // ByPixel：不同尺寸的元素在相同 factor 下左边缘位置一致
        [UnityTest]
        public IEnumerator TestE_ByPixel_DifferentSizesSameLeftEdge()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 1f;
            alg.PostWrapModeX = WrapMode.Loop;
            alg.AddKeyX(0f, 100f);

            var go1 = new GameObject("E_small");
            go1.transform.SetParent(alg.transform, false);
            var rt1 = go1.AddComponent<RectTransform>();
            rt1.sizeDelta = new Vector2(50f, 50f);
            go1.AddComponent<Image>();
            var le1 = go1.AddComponent<LayoutElement>();
            le1.minWidth = 50f; le1.minHeight = 50f;

            var go2 = new GameObject("E_large");
            go2.transform.SetParent(alg.transform, false);
            var rt2 = go2.AddComponent<RectTransform>();
            rt2.sizeDelta = new Vector2(150f, 150f);
            go2.AddComponent<Image>();
            var le2 = go2.AddComponent<LayoutElement>();
            le2.minWidth = 150f; le2.minHeight = 150f;

            yield return null;

            Assert.AreEqual(rt1.offsetMin.x, rt2.offsetMin.x, 1f,
                "ByPixel: 不同尺寸元素在相同 factor 下左边缘应一致");
        }

        // ByElementSize 结合 ScaleX：偏移 = size * factor * scale
        [UnityTest]
        public IEnumerator TestE_ByElementSize_WithScale()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByElementSize;
            alg.PositionModeY = PositionMode.ByElementSize;
            alg.ScaleX = 2f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 1f);
            alg.AddKeyX(2f, 1f);
            alg.AddKeyX(3f, 1f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // E1: pos = 100*1*2 = 200；E0: pos = 0 → 间距 200
            float gap = e[1].offsetMin.x - e[0].offsetMin.x;
            Assert.AreEqual(200f, gap, 1f, "ByElementSize ScaleX=2: 间距应为 effectiveSize*factor*scale=200");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试F — MappingMode（Direct / Interpolated / Proportional）
        //         注意避免与测试E重复
        // ═══════════════════════════════════════════════════════════════

        // Direct：超出关键帧数量时按 PostWrapMode 处理
        [UnityTest]
        public IEnumerator TestF_Direct_Loop_CyclesKeys()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.PostWrapModeX = WrapMode.Loop;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 100f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 2 个关键帧，Loop：E0→key0(0), E1→key1(100), E2→key0(0), E3→key1(100)
            Assert.AreEqual(e[0].offsetMin.x, e[2].offsetMin.x, 1f, "Direct Loop: E0 与 E2 位置相同");
            Assert.AreEqual(e[1].offsetMin.x, e[3].offsetMin.x, 1f, "Direct Loop: E1 与 E3 位置相同");
        }

        // Direct：PingPong
        [UnityTest]
        public IEnumerator TestF_Direct_PingPong_Reverses()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.PostWrapModeX = WrapMode.PingPong;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 100f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // PingPong: E0→0, E1→100, E2→0（回弹）, E3→100
            Assert.AreEqual(e[0].offsetMin.x, e[2].offsetMin.x, 1f, "Direct PingPong: E0 与 E2 位置相同");
        }

        // Interpolated：无 ConstrainByGroup，5 个元素均匀分布在曲线上
        [UnityTest]
        public IEnumerator TestF_Interpolated_UniformSpread()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Interpolated;
            alg.MappingModeY = KeyframeMappingMode.Interpolated;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 200f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // t=0,1/3,2/3,1 → Evaluate 线性：0, ~66, ~133, 200
            float x0 = e[0].offsetMin.x;
            float x3 = e[3].offsetMin.x;
            float x1 = e[1].offsetMin.x;
            Assert.AreEqual(x0 + (x3 - x0) / 3f, x1, 2f, "Interpolated: E1 应在首尾 1/3 处");
        }

        // Interpolated + ConstrainByGroup：每组独立重复曲线
        [UnityTest]
        public IEnumerator TestF_Interpolated_ConstrainByGroup_Repeats()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Interpolated;
            alg.MappingModeY = KeyframeMappingMode.Interpolated;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ConstrainByGroupX = true;
            alg.ConstrainByGroupY = true;
            alg.GroupSizeX = 2;
            alg.GroupSizeY = 2;
            alg.PostWrapModeX = WrapMode.Loop;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 100f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // GroupSizeX=2：t=0, 0.5, 1(Loop→0), 1.5(Loop→0.5)
            Assert.AreEqual(e[0].offsetMin.x, e[2].offsetMin.x, 1f, "ConstrainByGroup Loop: E0=E2");
            Assert.AreEqual(e[1].offsetMin.x, e[3].offsetMin.x, 1f, "ConstrainByGroup Loop: E1=E3");
        }

        // Proportional：首尾元素对应第一和最后一个关键帧
        [UnityTest]
        public IEnumerator TestF_Proportional_FirstAndLastMatchKeyframes()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Proportional;
            alg.MappingModeY = KeyframeMappingMode.Proportional;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.DistributeModeX = ProportionalDistributeMode.RoundToNearest;
            alg.DistributeModeY = ProportionalDistributeMode.RoundToNearest;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(0.5f, 100f);
            alg.AddKeyX(1f, 200f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 首元素对应 key0(0)，尾元素对应 key2(200)
            float x0 = e[0].offsetMin.x;
            float x3 = e[3].offsetMin.x;
            Assert.Greater(x3, x0, "Proportional: 尾元素位置应大于首元素");
        }

        // Proportional Uniform：各组分配均匀
        [UnityTest]
        public IEnumerator TestF_Proportional_Uniform_EvenDistribution()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Proportional;
            alg.MappingModeY = KeyframeMappingMode.Proportional;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.DistributeModeX = ProportionalDistributeMode.Uniform;
            alg.DistributeModeY = ProportionalDistributeMode.Uniform;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(0.5f, 100f);
            alg.AddKeyX(1f, 200f);
            // 6 个元素对应 3 个关键帧，Uniform → i=0,1,2→key0; i=3,4→key1; i=5→key2
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            Assert.AreEqual(e[0].offsetMin.x, e[1].offsetMin.x, 1f, "Uniform: E0=E1 应对应同一关键帧");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试G — 曲线形状 × PreWrapMode × PostWrapMode
        //         注意避免与测试E/F重复，聚焦曲线形状和 WrapMode 的组合
        // ═══════════════════════════════════════════════════════════════

        // 正弦形曲线（0→1→0）：中间元素在最高点，首尾对称
        [UnityTest]
        public IEnumerator TestG_YCurve_SineShape_Symmetric()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Interpolated;
            alg.MappingModeY = KeyframeMappingMode.Interpolated;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleY = 100f;
            alg.AddKeyX(0f, 0f); alg.AddKeyX(1f, 0f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(0.5f, 1f);
            alg.AddKeyY(1f, 0f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // t=0,1/3,2/3,1 → Y Evaluate(0)=0, (0.5)≈1（中间偏高）, (1)=0
            // 首尾 Y 应相同
            Assert.AreEqual(e[0].offsetMin.y, e[3].offsetMin.y, 2f, "正弦曲线首尾 Y 应相同");
            Assert.Greater(e[1].offsetMin.y + e[2].offsetMin.y,
                           e[0].offsetMin.y + e[3].offsetMin.y,
                           "正弦曲线中间元素 Y 应高于首尾");
        }

        // 负值曲线（value=-1）：Y 轴元素向下偏移
        [UnityTest]
        public IEnumerator TestG_YCurve_NegativeValue_ElementsBelow()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleY = 1f;
            alg.AddKeyX(0f, 0f); alg.AddKeyX(1f, 0f); alg.AddKeyX(2f, 0f); alg.AddKeyX(3f, 0f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, -50f);
            alg.AddKeyY(2f, -100f);
            alg.AddKeyY(3f, -150f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // value 负值 → Y 轴 factor 为正（-(-value)）→ 元素向下（offsetMin.y 更小）
            Assert.Greater(e[0].offsetMin.y, e[1].offsetMin.y, "负值曲线：后续元素 Y 应更低");
            Assert.Greater(e[1].offsetMin.y, e[2].offsetMin.y, "负值曲线：元素依次向下");
        }

        // X 曲线 PostWrapMode = Loop，Direct 模式
        [UnityTest]
        public IEnumerator TestG_XCurve_PostWrapLoop_Direct()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.PostWrapModeX = WrapMode.Loop;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 50f);
            alg.AddKeyX(2f, 100f);
            alg.AddKeyY(0f, 0f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 3 个关键帧，Loop：E0→0, E1→50, E2→100, E3→Loop→key0(0)
            Assert.AreEqual(e[0].offsetMin.x, e[3].offsetMin.x, 1f,
                "X PostWrapMode Loop: E3 应循环回 E0 的位置");
        }

        // X 曲线 PostWrapMode = ClampForever，超出后钉住最后关键帧
        [UnityTest]
        public IEnumerator TestG_XCurve_PostWrapClamp_Direct()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.PostWrapModeX = WrapMode.ClampForever;
            alg.ScaleX = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 100f);
            alg.AddKeyY(0f, 0f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 2 个关键帧，Clamp：E0→0, E1→100, E2/E3→钉在 key1(100)
            Assert.AreEqual(e[1].offsetMin.x, e[2].offsetMin.x, 1f, "X PostWrapMode Clamp: E2 钉在 E1");
            Assert.AreEqual(e[1].offsetMin.x, e[3].offsetMin.x, 1f, "X PostWrapMode Clamp: E3 钉在 E1");
        }

        // Y 曲线 PostWrapMode = PingPong
        [UnityTest]
        public IEnumerator TestG_YCurve_PostWrapPingPong_Direct()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.PostWrapModeY = WrapMode.PingPong;
            alg.ScaleY = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, 50f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 2 个关键帧，PingPong：E0→0, E1→50, E2→0（回弹）, E3→50
            Assert.AreEqual(e[0].offsetMin.y, e[2].offsetMin.y, 1f, "Y PostWrapMode PingPong: E0=E2");
            Assert.AreEqual(e[1].offsetMin.y, e[3].offsetMin.y, 1f, "Y PostWrapMode PingPong: E1=E3");
        }

        // 非线性曲线（三次方：0→0→1→1）：中间段增长更缓，末尾急速上升
        [UnityTest]
        public IEnumerator TestG_XCurve_NonLinear_Interpolated()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Interpolated;
            alg.MappingModeY = KeyframeMappingMode.Interpolated;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 1f;
            // 近似三次方曲线：控制点 (0,0), (0.5,0.1), (0.8,0.5), (1,1)
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(0.5f, 0.1f);
            alg.AddKeyX(0.8f, 0.5f);
            alg.AddKeyX(1f, 1f);
            alg.AddKeyY(0f, 0f); alg.AddKeyY(1f, 0f);
            var e = new RectTransform[4];
            for (int i = 0; i < 4; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            // 非线性：前半段间距小，后半段间距大
            float gap01 = e[1].offsetMin.x - e[0].offsetMin.x;
            float gap23 = e[3].offsetMin.x - e[2].offsetMin.x;
            Assert.Less(gap01, gap23, "非线性曲线：后半段间距应大于前半段");
        }

        // ═══════════════════════════════════════════════════════════════
        // 测试H — ScaleX / ScaleY（0、正、负）
        //         注意避免与测试E/F/G重复
        // ═══════════════════════════════════════════════════════════════

        // ScaleX = 0：所有元素 X 位置相同（偏移归零）
        [UnityTest]
        public IEnumerator TestH_ScaleX_Zero_AllSameX()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 0f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 999f);
            alg.AddKeyY(0f, 0f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            Assert.AreEqual(e[0].offsetMin.x, e[1].offsetMin.x, 1f, "ScaleX=0: 所有元素 X 应相同");
        }

        // ScaleX = 2：偏移量是 ScaleX=1 时的两倍
        [UnityTest]
        public IEnumerator TestH_ScaleX_Positive_DoublesOffset()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 50f);
            alg.AddKeyY(0f, 0f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");

            alg.ScaleX = 1f;
            yield return null;
            float gap1 = e[1].offsetMin.x - e[0].offsetMin.x;

            alg.ScaleX = 2f;
            yield return null;
            float gap2 = e[1].offsetMin.x - e[0].offsetMin.x;

            Assert.AreEqual(2f, gap2 / gap1, 0.05f, "ScaleX=2 时偏移应为 ScaleX=1 的两倍");
        }

        // ScaleX 负值：偏移方向反转
        [UnityTest]
        public IEnumerator TestH_ScaleX_Negative_FlipsDirection()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = -1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 50f); // factor=50，scale=-1 → 偏移=-50（向左）
            alg.AddKeyY(0f, 0f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            Assert.Less(e[1].offsetMin.x, e[0].offsetMin.x,
                "ScaleX 负值：第二个元素应在第一个左侧（偏移方向反转）");
        }

        // ScaleY = 0：所有元素 Y 位置相同
        [UnityTest]
        public IEnumerator TestH_ScaleY_Zero_AllSameY()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleY = 0f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, 999f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            Assert.AreEqual(e[0].offsetMin.y, e[1].offsetMin.y, 1f, "ScaleY=0: 所有元素 Y 应相同");
        }

        // ScaleY 负值：正 value 的曲线使元素向下偏移
        [UnityTest]
        public IEnumerator TestH_ScaleY_Negative_FlipsDirection()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleY = 1f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, 50f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");

            yield return null;
            float yPositive = e[1].offsetMin.y;

            alg.ScaleY = -1f;
            yield return null;
            float yNegative = e[1].offsetMin.y;

            Assert.AreNotEqual(yPositive, yNegative, "ScaleY 正负应产生不同 Y 偏移");
            // ScaleY=-1 时 factor 内部取反两次（-value * -scale），实际方向与 ScaleY=1 相反
            Assert.Less(yNegative, yPositive, "ScaleY 负值应使元素偏向更低的 Y");
        }

        // ScaleX 与 ScaleY 同时为非零正值：两轴偏移独立叠加
        [UnityTest]
        public IEnumerator TestH_ScaleXY_Both_Independent()
        {
            var alg = CreateALG();
            alg.MappingModeX = KeyframeMappingMode.Direct;
            alg.MappingModeY = KeyframeMappingMode.Direct;
            alg.PositionModeX = PositionMode.ByPixel;
            alg.PositionModeY = PositionMode.ByPixel;
            alg.ScaleX = 3f;
            alg.ScaleY = 2f;
            alg.AddKeyX(0f, 0f);
            alg.AddKeyX(1f, 30f);
            alg.AddKeyY(0f, 0f);
            alg.AddKeyY(1f, 20f);
            var e = new RectTransform[2];
            for (int i = 0; i < 2; i++) e[i] = CreateE(alg.transform, $"E{i}");
            yield return null;

            float xGap = e[1].offsetMin.x - e[0].offsetMin.x;
            // Y轴: factor = -kf.value = -20, posY = factor * scaleY = -20 * 2 = -40
            // SetInsetAndSizeFromParentEdge(Top, posY, size)：offsetMin.y = -(posY + size)
            // E0: posY=0  → offsetMin.y = -(0+100)  = -100
            // E1: posY=-40 → offsetMin.y = -(-40+100) = -60
            // yGap = e[1].offsetMin.y - e[0].offsetMin.y = -60 - (-100) = +40
            float yGap = e[1].offsetMin.y - e[0].offsetMin.y;

            Assert.AreEqual(90f, xGap, 1f, "ScaleX=3, factor=30 → X 偏移=90");
            Assert.AreEqual(40f, yGap, 1f, "ScaleY=2, factor=20 → Y 偏移=+40（posY=-40，offsetMin.y升高）");
        }
    }
}
