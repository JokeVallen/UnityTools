using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// CircleLayout 补充测试
    /// </summary>
    public class CircleLayoutSupplementTests
    {
        private GameObject canvasGo;
        private GameObject containerGo;
        private CircleLayoutGroup layout;

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
            containerGo.AddComponent<RectTransform>().sizeDelta = new Vector2(400, 400);
            layout = containerGo.AddComponent<CircleLayoutGroup>();

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(canvasGo);
        }

        private RectTransform[] CreateChildren(int count, float size = 30f)
        {
            var result = new RectTransform[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"C_{i}");
                go.transform.SetParent(containerGo.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                go.AddComponent<Image>();
                result[i] = rt;
            }
            return result;
        }

        private static Vector2 GetChildCenterWorld(RectTransform child)
        {
            Vector3[] c = new Vector3[4];
            child.GetWorldCorners(c);
            return new Vector2((c[0].x + c[2].x) * 0.5f, (c[0].y + c[2].y) * 0.5f);
        }

        private static Vector2 GetContainerCenterWorld(RectTransform container)
        {
            Vector3[] c = new Vector3[4];
            container.GetWorldCorners(c);
            return new Vector2((c[0].x + c[2].x) * 0.5f, (c[0].y + c[2].y) * 0.5f);
        }

        // ═══════════════════════════════════════════════════════════════════
        // 一、单个子物体
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SingleChild_PlacedAtRotationAngle()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(1, 20f);
            var containerRt = containerGo.GetComponent<RectTransform>();
            yield return null;

            Vector2 center = GetContainerCenterWorld(containerRt);
            Vector2 pos = GetChildCenterWorld(children[0]);
            Vector2 dir = pos - center;

            // rotation=0，单个子物体：cos(0)=1，sin(0)=0 → X 方向偏移，Y≈0
            Assert.AreEqual(0f, dir.y, 2f,
                "单个子物体 Rotation=0：应在圆的正右方（Y 偏移≈0）");
            Assert.Greater(dir.x, 0f,
                "单个子物体 Rotation=0：应在圆心右侧");
        }

        [UnityTest]
        public IEnumerator SingleChild_Radius_DistanceCorrect()
        {
            layout.Radius = 120f;
            layout.Rotation = 0;
            var children = CreateChildren(1, 20f);
            var containerRt = containerGo.GetComponent<RectTransform>();
            yield return null;

            Vector2 center = GetContainerCenterWorld(containerRt);
            float dist = Vector2.Distance(GetChildCenterWorld(children[0]), center);
            Assert.AreEqual(120f, dist, 2f,
                "单个子物体到圆心的距离应等于 Radius");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、动态删除子物体
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator DynamicRemoveChild_AnglesRecomputed()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(4, 20f);
            var containerRt = containerGo.GetComponent<RectTransform>();
            yield return null;

            Vector2 center = GetContainerCenterWorld(containerRt);

            // 4 个子物体时 angleDelta=90°
            Vector2 dir0_before = GetChildCenterWorld(children[0]) - center;
            Vector2 dir1_before = GetChildCenterWorld(children[1]) - center;
            float angle_before = Mathf.Abs(Mathf.DeltaAngle(
                Mathf.Atan2(dir0_before.y, dir0_before.x) * Mathf.Rad2Deg,
                Mathf.Atan2(dir1_before.y, dir1_before.x) * Mathf.Rad2Deg));

            // 删除一个子物体，变为 3 个，angleDelta 应变为 120°
            Object.Destroy(children[3].gameObject);
            yield return null;
            yield return null;

            Vector2 dir0_after = GetChildCenterWorld(children[0]) - center;
            Vector2 dir1_after = GetChildCenterWorld(children[1]) - center;
            float angle_after = Mathf.Abs(Mathf.DeltaAngle(
                Mathf.Atan2(dir0_after.y, dir0_after.x) * Mathf.Rad2Deg,
                Mathf.Atan2(dir1_after.y, dir1_after.x) * Mathf.Rad2Deg));

            Assert.AreEqual(90f, angle_before, 2f, "4 个子物体时角度步进应为 90°");
            Assert.AreEqual(120f, angle_after, 2f, "删除一个后角度步进应变为 120°");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、禁用 / 启用
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Disabled_ChildrenNotReposistioned()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(2, 20f);
            yield return null;

            var childRt = children[0];
            float xBefore = childRt.offsetMin.x;

            layout.enabled = false;
            yield return null;

            childRt.anchoredPosition = new Vector2(0f, 0f);
            yield return null;

            Assert.AreNotEqual(xBefore, childRt.offsetMin.x,
                "CircleLayout 禁用后子物体位置应可以自由修改");
        }

        [UnityTest]
        public IEnumerator ReEnabled_ChildrenRepositioned()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(2, 20f);
            yield return null;

            var childRt = children[0];
            float xLayout = childRt.offsetMin.x;

            layout.enabled = false;
            yield return null;
            childRt.anchoredPosition = new Vector2(999f, 0f);
            yield return null;

            layout.enabled = true;
            yield return null;

            Assert.AreEqual(xLayout, childRt.offsetMin.x, 2f,
                "CircleLayout 重新启用后子物体应恢复到布局计算位置");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、Padding 对 preferredSize 的影响
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Padding_IncludedinPreferredSize()
        {
            layout.Radius = 80f;
            layout.padding = new RectOffset(10, 20, 15, 25);
            CreateChildren(4, 20f);
            yield return null;

            float expectedW = layout.padding.horizontal + layout.Radius * 2; // 30 + 160 = 190
            float expectedH = layout.padding.vertical + layout.Radius * 2; // 40 + 160 = 200
            Assert.AreEqual(expectedW, layout.preferredWidth, 1f,
                "preferredWidth 应等于 radius*2 + padding.horizontal");
            Assert.AreEqual(expectedH, layout.preferredHeight, 1f,
                "preferredHeight 应等于 radius*2 + padding.vertical");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、属性验证
        // ═══════════════════════════════════════════════════════════════════

        [Test]
        public void Radius_Zero_Accepted()
        {
            layout.Radius = 0f;
            Assert.AreEqual(0f, layout.Radius, "Radius=0 应被接受");
        }

        [Test]
        public void Rotation_NegativeValue_Normalized()
        {
            layout.Rotation = -90;
            // ((-90 % 360) + 360) % 360 = 270
            Assert.AreEqual(270, layout.Rotation,
                "Rotation=-90 应归一化为 270");
        }

        [Test]
        public void Rotation_LargeValue_Normalized()
        {
            layout.Rotation = 450;
            // 450 % 360 = 90
            Assert.AreEqual(90, layout.Rotation,
                "Rotation=450 应归一化为 90");
        }

        [Test]
        public void ClockWise_DefaultIsFalse()
        {
            Assert.IsFalse(layout.ClockWise,
                "ClockWise 默认值应为 false（逆时针）");
        }
    }
}
