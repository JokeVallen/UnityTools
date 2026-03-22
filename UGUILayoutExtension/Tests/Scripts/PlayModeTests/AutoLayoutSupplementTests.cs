using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// AutoLayout 补充测试 — 覆盖原测试集未触及的功能点
    /// </summary>
    public class AutoLayoutSupplementTests
    {
        private GameObject canvasGo;
        private GameObject containerGo;
        private AutoLayoutGroup layout;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            containerGo = new GameObject("Container");
            containerGo.transform.SetParent(canvasGo.transform, false);
            containerGo.AddComponent<RectTransform>().sizeDelta = new Vector2(600, 400);
            layout = containerGo.AddComponent<AutoLayoutGroup>();

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(canvasGo);
        }

        private RectTransform[] CreateChildren(int count, float size = 50f)
        {
            var result = new RectTransform[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Child_{i}");
                go.transform.SetParent(containerGo.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                go.AddComponent<Image>();
                result[i] = rt;
            }
            return result;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 一、Direct — Clamp WrapMode
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Direct_PostWrapMode_Clamp_StaysAtLastKey()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.ClampForever;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            // 3 个子物体，2 个关键帧，Clamp → 第 3 个钉在最后一个关键帧（value=100）
            var children = CreateChildren(3, 10f);
            yield return null;

            Assert.AreEqual(children[1].offsetMin.x, children[2].offsetMin.x, 1f,
                "Clamp：超出关键帧数量时应钉在最后一个关键帧位置");
        }

        [UnityTest]
        public IEnumerator Direct_PostWrapMode_Default_ClampsBehavior()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.Default;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 80f);
            var children = CreateChildren(4, 10f);
            yield return null;

            // Default 行为与 Clamp 相同，超出后钉在最后关键帧
            Assert.AreEqual(children[1].offsetMin.x, children[2].offsetMin.x, 1f,
                "Default WrapMode：超出后行为应与 Clamp 相同");
            Assert.AreEqual(children[1].offsetMin.x, children[3].offsetMin.x, 1f,
                "Default WrapMode：多个超出的元素都应钉在最后关键帧");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、Proportional — FloorBias / CeilBias
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Proportional_FloorBias_FrontHeavier()
        {
            layout.MappingModeX = KeyframeMappingMode.Proportional;
            layout.MappingModeY = KeyframeMappingMode.Proportional;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.DistributeModeX = ProportionalDistributeMode.FloorBias;
            layout.DistributeModeY = ProportionalDistributeMode.FloorBias;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(0.5f, 100f);
            layout.AddKeyX(1f, 200f);
            // FloorBias：floor(i/(count-1) * (keyCount-1))
            // 4 子物体，3 关键帧：i=0→0, i=1→floor(0.33*2)=0, i=2→floor(0.66*2)=1, i=3→2
            // child[0,1]→key0(0), child[2]→key1(100), child[3]→key2(200)
            var children = CreateChildren(4, 10f);
            yield return null;

            Assert.AreEqual(children[0].offsetMin.x, children[1].offsetMin.x, 1f,
                "FloorBias：child[0] 和 child[1] 应对应同一关键帧（前密）");
            Assert.AreNotEqual(children[1].offsetMin.x, children[2].offsetMin.x,
                "FloorBias：child[1] 和 child[2] 应对应不同关键帧");
            Assert.AreNotEqual(children[2].offsetMin.x, children[3].offsetMin.x,
                "FloorBias：child[2] 和 child[3] 应对应不同关键帧");
        }

        [UnityTest]
        public IEnumerator Proportional_CeilBias_BackHeavier()
        {
            layout.MappingModeX = KeyframeMappingMode.Proportional;
            layout.MappingModeY = KeyframeMappingMode.Proportional;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.DistributeModeX = ProportionalDistributeMode.CeilBias;
            layout.DistributeModeY = ProportionalDistributeMode.CeilBias;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(0.5f, 100f);
            layout.AddKeyX(1f, 200f);
            // CeilBias：ceil(i/(count-1) * (keyCount-1))
            // 4 子物体，3 关键帧：i=0→0, i=1→ceil(0.33*2)=1, i=2→ceil(0.66*2)=2, i=3→2
            // child[0]→key0(0), child[1]→key1(100), child[2,3]→key2(200)
            var children = CreateChildren(4, 10f);
            yield return null;

            Assert.AreNotEqual(children[0].offsetMin.x, children[1].offsetMin.x,
                "CeilBias：child[0] 和 child[1] 应对应不同关键帧");
            Assert.AreNotEqual(children[1].offsetMin.x, children[2].offsetMin.x,
                "CeilBias：child[1] 和 child[2] 应对应不同关键帧");
            Assert.AreEqual(children[2].offsetMin.x, children[3].offsetMin.x, 1f,
                "CeilBias：child[2] 和 child[3] 应对应同一关键帧（后密）");
        }

        [UnityTest]
        public IEnumerator Proportional_FloorBias_vs_CeilBias_Asymmetric()
        {
            // FloorBias 和 CeilBias 对相同输入应产生不同的分配结果
            layout.MappingModeX = KeyframeMappingMode.Proportional;
            layout.MappingModeY = KeyframeMappingMode.Proportional;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(0.5f, 100f);
            layout.AddKeyX(1f, 200f);
            var children = CreateChildren(4, 10f);

            layout.DistributeModeX = ProportionalDistributeMode.FloorBias;
            layout.DistributeModeY = ProportionalDistributeMode.FloorBias;
            yield return null;
            float floorX1 = children[1].offsetMin.x;

            layout.DistributeModeX = ProportionalDistributeMode.CeilBias;
            layout.DistributeModeY = ProportionalDistributeMode.CeilBias;
            yield return null;
            float ceilX1 = children[1].offsetMin.x;

            Assert.AreNotEqual(floorX1, ceilX1,
                "FloorBias 和 CeilBias 对 child[1] 应产生不同的关键帧映射");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、SpacingVertical
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SpacingVertical_AddsFixedGapBetweenElements()
        {
            float spacing = 25f;
            layout.SpacingVertical = spacing;
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyY(0f, 0f);
            var children = CreateChildren(3, 50f);
            yield return null;

            // spacing 向下叠加（pos 增大），offsetMin.y = -(pos + size)，pos 越大 offsetMin.y 越小
            // child[0]: pos=0,  offsetMin.y = -(0+50)  = -50
            // child[1]: pos=25, offsetMin.y = -(25+50) = -75  → gap = 25
            // child[2]: pos=50, offsetMin.y = -(50+50) = -100 → gap = 25
            float gap01 = children[0].offsetMin.y - children[1].offsetMin.y;
            float gap12 = children[1].offsetMin.y - children[2].offsetMin.y;
            Assert.AreEqual(spacing, gap01, 1f, "SpacingVertical：第 1 和 2 个元素间距应等于 SpacingVertical");
            Assert.AreEqual(spacing, gap12, 1f, "SpacingVertical：第 2 和 3 个元素间距应等于 SpacingVertical");
        }

        [UnityTest]
        public IEnumerator SpacingVertical_CannotBeNegative()
        {
            layout.SpacingVertical = -30f;
            Assert.AreEqual(0f, layout.SpacingVertical, 0.001f,
                "SpacingVertical 不应接受负值");
            yield break;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、ScaleX = 0 极端值
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ScaleX_Zero_AllChildrenSameXPosition()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 0f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 999f); // value 很大但 scale=0，偏移应为 0
            var children = CreateChildren(3, 30f);
            yield return null;

            Assert.AreEqual(children[0].offsetMin.x, children[1].offsetMin.x, 1f,
                "ScaleX=0：所有元素 X 位置应相同（偏移为零）");
            Assert.AreEqual(children[1].offsetMin.x, children[2].offsetMin.x, 1f,
                "ScaleX=0：所有元素 X 位置应相同（偏移为零）");
        }

        [UnityTest]
        public IEnumerator ScaleY_Zero_AllChildrenSameYPosition()
        {
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleY = 0f;
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 999f);
            var children = CreateChildren(3, 30f);
            yield return null;

            Assert.AreEqual(children[0].offsetMin.y, children[1].offsetMin.y, 1f,
                "ScaleY=0：所有元素 Y 位置应相同");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、Y 轴曲线 API
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void AddKeyY_IncreasesLength()
        {
            int before = layout.LengthY;
            layout.AddKeyY(0f, 0f);
            Assert.AreEqual(before + 1, layout.LengthY, "AddKeyY 应使关键帧数量增加 1");
        }

        [Test]
        public void RemoveKeyY_DecreasesLength()
        {
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 1f);
            int before = layout.LengthY;
            layout.RemoveKeyY(0);
            Assert.AreEqual(before - 1, layout.LengthY, "RemoveKeyY 应使关键帧数量减少 1");
        }

        [Test]
        public void ClearKeysY_RemovesAllKeys()
        {
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 1f);
            layout.ClearKeysY();
            Assert.AreEqual(0, layout.LengthY, "ClearKeysY 应清空所有关键帧");
        }

        [Test]
        public void GetKeyY_OutOfRange_ThrowsException()
        {
            layout.AddKeyY(0f, 0f);
            Assert.Throws<System.IndexOutOfRangeException>(
                () => layout.GetKeyY(5),
                "GetKeyY 越界时应抛出 IndexOutOfRangeException");
        }

        [Test]
        public void EvaluateY_ReturnsCorrectValue()
        {
            layout.KeysY = new[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) };
            float v = layout.EvaluateY(0.5f);
            Assert.AreEqual(0.5f, v, 0.05f, "EvaluateY(0.5) 在线性曲线上应返回约 0.5");
        }

        [Test]
        public void KeysY_Setter_UpdatesCurve()
        {
            var keys = new[] { new Keyframe(0f, 0f), new Keyframe(1f, 2f) };
            layout.KeysY = keys;
            Assert.AreEqual(2, layout.LengthY, "KeysY setter 应更新曲线关键帧数量");
            Assert.AreEqual(2f, layout.EvaluateY(1f), 0.05f, "KeysY setter 后 EvaluateY(1) 应返回新值");
        }

        [Test]
        public void KeysX_Setter_UpdatesCurve()
        {
            var keys = new[] { new Keyframe(0f, 0f), new Keyframe(1f, 3f) };
            layout.KeysX = keys;
            Assert.AreEqual(2, layout.LengthX, "KeysX setter 应更新曲线关键帧数量");
            Assert.AreEqual(3f, layout.EvaluateX(1f), 0.05f, "KeysX setter 后 EvaluateX(1) 应返回新值");
        }

        [Test]
        public void MoveKeyY_ChangesKeyframePosition()
        {
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(1f, 1f);
            layout.MoveKeyY(1, new Keyframe(1f, 5f));
            Assert.AreEqual(5f, layout.EvaluateY(1f), 0.1f,
                "MoveKeyY 后曲线末端 value 应更新");
        }

        [Test]
        public void SmoothTangentsX_DoesNotThrow()
        {
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(0.5f, 1f);
            layout.AddKeyX(1f, 0f);
            Assert.DoesNotThrow(() => layout.SmoothTangentsX(1, 0f),
                "SmoothTangentsX 不应抛出异常");
        }

        [Test]
        public void SmoothTangentsY_DoesNotThrow()
        {
            layout.AddKeyY(0f, 0f);
            layout.AddKeyY(0.5f, 1f);
            layout.AddKeyY(1f, 0f);
            Assert.DoesNotThrow(() => layout.SmoothTangentsY(1, 0f),
                "SmoothTangentsY 不应抛出异常");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 六、ChildAlignment = UpperLeft（默认值）
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ChildAlignment_UpperLeft_ContentAtTopLeft()
        {
            layout.ChildAlignment = TextAnchor.UpperLeft;
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyY(0f, 0f);
            var children = CreateChildren(1, 50f);
            yield return null;

            // UpperLeft：posX = padding.left = 0，posY = padding.top = 0
            // offsetMin.x = 0，offsetMin.y = -(0 + 50) = -50
            Assert.AreEqual(0f, children[0].offsetMin.x, 1f,
                "UpperLeft：子物体 X 应贴容器左边缘");
            Assert.AreEqual(-50f, children[0].offsetMin.y, 1f,
                "UpperLeft：子物体 Y 应贴容器顶部（offsetMin.y = -size）");
        }

        [UnityTest]
        public IEnumerator ChildAlignment_UpperLeft_IsDefault()
        {
            // 验证默认对齐方式确实是 UpperLeft
            Assert.AreEqual(TextAnchor.UpperLeft, layout.ChildAlignment,
                "ChildAlignment 默认值应为 UpperLeft");
            yield return null;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 七、GroupSize 边界值
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator GroupSize_One_EachChildIsOneCycle()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.ConstrainByGroupX = true;
            layout.ConstrainByGroupY = true;
            layout.GroupSizeX = 1;
            layout.GroupSizeY = 1;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.PostWrapModeX = WrapMode.Loop;
            layout.ScaleX = 100f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);
            // GroupSizeX=1：t = i/1 = 0, 1, 2, 3...，Loop 后 t%1 = 0, 0, 0, 0
            var children = CreateChildren(4, 10f);
            yield return null;

            // 所有元素 t=0（Loop）→ Evaluate(0) = 0 → pos=0，X 位置相同
            Assert.AreEqual(children[0].offsetMin.x, children[1].offsetMin.x, 1f,
                "GroupSizeX=1：所有元素 t 经 Loop 后均为 0，X 位置应相同");
            Assert.AreEqual(children[1].offsetMin.x, children[2].offsetMin.x, 1f,
                "GroupSizeX=1：所有元素 X 位置应相同");
        }

        [Test]
        public void GroupSize_Zero_ClampedToOne()
        {
            layout.GroupSizeX = 0;
            layout.GroupSizeY = 0;
            Assert.AreEqual(1, layout.GroupSizeX,
                "GroupSizeX 设为 0 时应被 clamp 到 1");
        }

        [Test]
        public void GroupSize_Negative_ClampedToOne()
        {
            layout.GroupSizeX = -5;
            layout.GroupSizeY = -5;
            Assert.AreEqual(1, layout.GroupSizeX,
                "GroupSizeX 设为负值时应被 clamp 到 1");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 八、Interpolated — X 轴分布验证
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Interpolated_XAxis_LinearSpread()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ScaleX = 1f;
            // X 轴曲线：线性 0→100，5 个子物体 t = 0, 0.25, 0.5, 0.75, 1
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 100f);
            var children = CreateChildren(5, 10f);
            yield return null;

            float x0 = children[0].offsetMin.x;
            float x4 = children[4].offsetMin.x;
            float x2 = children[2].offsetMin.x;

            // x2 应在 x0 和 x4 的中间
            Assert.AreEqual((x0 + x4) * 0.5f, x2, 2f,
                "Interpolated X 轴线性曲线：中间元素应在首尾中点");
        }

        [UnityTest]
        public IEnumerator Interpolated_XAxis_PingPong_WithConstrainByGroup()
        {
            layout.MappingModeX = KeyframeMappingMode.Interpolated;
            layout.MappingModeY = KeyframeMappingMode.Interpolated;
            layout.PositionModeX = PositionMode.ByPixel;
            layout.PositionModeY = PositionMode.ByPixel;
            layout.ConstrainByGroupX = true;
            layout.ConstrainByGroupY = true;
            layout.GroupSizeX = 2;
            layout.GroupSizeY = 2;
            layout.PostWrapModeX = WrapMode.PingPong;
            layout.ScaleX = 100f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);
            // t: 0, 0.5, 1(→PingPong反弹→1), 1.5(→PingPong→0.5)
            // child[0]: Evaluate(0)=0, child[1]: Evaluate(0.5)=0.5
            // child[2]: Evaluate(1)=1, child[3]: Evaluate(0.5)=0.5（PingPong回弹）
            var children = CreateChildren(4, 10f);
            yield return null;

            Assert.AreEqual(children[1].offsetMin.x, children[3].offsetMin.x, 2f,
                "Interpolated X PingPong：child[1] 和 child[3] t 相同，位置应相同");
            Assert.Greater(children[2].offsetMin.x, children[1].offsetMin.x,
                "Interpolated X PingPong：child[2] 的 t 最大，X 位置应最大");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 九、effectiveSize 三者交互（sizeDelta / min / preferred）
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator EffectiveSize_MinWinsOverSizeDelta()
        {
            // sizeDelta=20, min=80, preferred=0 → effectiveSize=80
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f); // factor=1 → pos = effectiveSize

            // 参照元素先创建 → 对应 key0（factor=0），pos=0
            var go0 = new GameObject("Ref");
            go0.transform.SetParent(containerGo.transform, false);
            var rt0 = go0.AddComponent<RectTransform>();
            rt0.sizeDelta = new Vector2(20f, 20f);
            go0.AddComponent<Image>();
            var le0 = go0.AddComponent<LayoutElement>();
            le0.minWidth = 80f;

            // 测试元素后创建 → 对应 key1（factor=1），pos = effectiveSize * 1 * 1
            var go = new GameObject("Child");
            go.transform.SetParent(containerGo.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(20f, 20f);
            go.AddComponent<Image>();
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 80f;

            yield return null;

            // child[1](factor=1) 的 offsetMin.x = startOffset + effectiveSize*1*1
            // child[0](factor=0) 的 offsetMin.x = startOffset
            // 差值应等于 effectiveSize = max(20, 80, 0) = 80
            float diff = rt.offsetMin.x - rt0.offsetMin.x;
            Assert.AreEqual(80f, diff, 1f,
                "min > sizeDelta 时，effectiveSize 应取 min（80），位置差应为 80");
        }

        [UnityTest]
        public IEnumerator EffectiveSize_PreferredWinsOverMin()
        {
            // sizeDelta=20, min=50, preferred=120 → effectiveSize=120
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);

            var go0 = new GameObject("Ref");
            go0.transform.SetParent(containerGo.transform, false);
            go0.AddComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
            go0.AddComponent<Image>();
            var le0 = go0.AddComponent<LayoutElement>();
            le0.minWidth = 50f; le0.preferredWidth = 120f;

            var go1 = new GameObject("Child");
            go1.transform.SetParent(containerGo.transform, false);
            go1.AddComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
            go1.AddComponent<Image>();
            var le1 = go1.AddComponent<LayoutElement>();
            le1.minWidth = 50f; le1.preferredWidth = 120f;

            yield return null;

            float diff = go1.GetComponent<RectTransform>().offsetMin.x
                       - go0.GetComponent<RectTransform>().offsetMin.x;
            Assert.AreEqual(120f, diff, 1f,
                "preferred > min 时，effectiveSize 应取 preferred（120），位置差应为 120");
        }

        [UnityTest]
        public IEnumerator EffectiveSize_SizeDeltaWinsWhenLargest()
        {
            // sizeDelta=200, min=50, preferred=80 → effectiveSize=200
            layout.MappingModeX = KeyframeMappingMode.Direct;
            layout.MappingModeY = KeyframeMappingMode.Direct;
            layout.PositionModeX = PositionMode.ByElementSize;
            layout.PositionModeY = PositionMode.ByElementSize;
            layout.ScaleX = 1f;
            layout.AddKeyX(0f, 0f);
            layout.AddKeyX(1f, 1f);

            var go0 = new GameObject("Ref");
            go0.transform.SetParent(containerGo.transform, false);
            go0.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
            go0.AddComponent<Image>();
            var le0 = go0.AddComponent<LayoutElement>();
            le0.minWidth = 50f; le0.preferredWidth = 80f;

            var go1 = new GameObject("Child");
            go1.transform.SetParent(containerGo.transform, false);
            go1.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 50f);
            go1.AddComponent<Image>();
            var le1 = go1.AddComponent<LayoutElement>();
            le1.minWidth = 50f; le1.preferredWidth = 80f;

            yield return null;

            float diff = go1.GetComponent<RectTransform>().offsetMin.x
                       - go0.GetComponent<RectTransform>().offsetMin.x;
            Assert.AreEqual(200f, diff, 1f,
                "sizeDelta > preferred > min 时，effectiveSize 应取 sizeDelta（200）");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 十、WrapMode 属性设置同步到曲线
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void PreWrapModeX_SetterSyncsToCurve()
        {
            layout.PreWrapModeX = WrapMode.Loop;
            Assert.AreEqual(WrapMode.Loop, layout.PreWrapModeX,
                "PreWrapModeX setter 应同步到曲线");
        }

        [Test]
        public void PostWrapModeX_SetterSyncsToCurve()
        {
            layout.PostWrapModeX = WrapMode.PingPong;
            Assert.AreEqual(WrapMode.PingPong, layout.PostWrapModeX,
                "PostWrapModeX setter 应同步到曲线");
        }

        [Test]
        public void PreWrapModeY_SetterSyncsToCurve()
        {
            layout.PreWrapModeY = WrapMode.Loop;
            Assert.AreEqual(WrapMode.Loop, layout.PreWrapModeY,
                "PreWrapModeY setter 应同步到曲线");
        }

        [Test]
        public void PostWrapModeY_SetterSyncsToCurve()
        {
            layout.PostWrapModeY = WrapMode.PingPong;
            Assert.AreEqual(WrapMode.PingPong, layout.PostWrapModeY,
                "PostWrapModeY setter 应同步到曲线");
        }
    }
}