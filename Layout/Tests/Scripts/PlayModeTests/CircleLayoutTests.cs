using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UGUI.Layout.Extension;

namespace UGUI.Layout.Extension.Tests
{
    /// <summary>
    /// CircleLayout 运行时 API 测试
    /// </summary>
    public class CircleLayoutTests
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
            var rt = containerGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 400);
            layout = containerGo.AddComponent<CircleLayoutGroup>();

            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(canvasGo);
        }

        private RectTransform[] CreateChildren(int count, float size = 40f)
        {
            var children = new RectTransform[count];
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Child_{i}");
                go.transform.SetParent(containerGo.transform, false);
                var rt = go.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(size, size);
                go.AddComponent<Image>();
                children[i] = rt;
            }
            return children;
        }

        // ── 辅助：用世界坐标计算元素中心，避免 UGUI offsetMin 坐标系混乱 ──
        private static Vector2 GetChildCenterWorld(RectTransform child)
        {
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);
            // corners: [0]=leftBottom, [1]=leftTop, [2]=rightTop, [3]=rightBottom
            return new Vector2(
                (corners[0].x + corners[2].x) * 0.5f,
                (corners[0].y + corners[2].y) * 0.5f
            );
        }

        private static Vector2 GetContainerCenterWorld(RectTransform container)
        {
            Vector3[] corners = new Vector3[4];
            container.GetWorldCorners(corners);
            return new Vector2(
                (corners[0].x + corners[2].x) * 0.5f,
                (corners[0].y + corners[2].y) * 0.5f
            );
        }

        // ═══════════════════════════════════════════════════════════════════
        // 一、圆周均匀分布
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator FourChildren_EqualAngularSpacing()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(4, 20f);
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);

            // 4 个子物体中心应等角度分布（相邻夹角 90°）
            float[] angles = new float[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 c = GetChildCenterWorld(children[i]);
                Vector2 dir = c - center;
                angles[i] = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            // 相邻夹角应接近 90°
            for (int i = 0; i < 3; i++)
            {
                float delta = Mathf.Abs(Mathf.DeltaAngle(angles[i], angles[i + 1]));
                Assert.AreEqual(90f, delta, 2f,
                    $"子物体 {i} 和 {i + 1} 之间的角度差应为 90°");
            }
        }

        [UnityTest]
        public IEnumerator AllChildrenEquidistantFromCenter()
        {
            layout.Radius = 120f;
            layout.Rotation = 0;
            var children = CreateChildren(6, 20f);
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);

            float[] distances = new float[6];
            for (int i = 0; i < 6; i++)
            {
                Vector2 c = GetChildCenterWorld(children[i]);
                distances[i] = Vector2.Distance(c, center);
            }

            for (int i = 1; i < 6; i++)
                Assert.AreEqual(distances[0], distances[i], 2f,
                    $"所有子物体到圆心的距离应相等，child[{i}] 偏差过大");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 二、Radius
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Radius_ScalesDistanceFromCenter()
        {
            layout.Rotation = 0;
            var children = CreateChildren(1, 20f);

            layout.Radius = 80f;
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);
            float dist80 = Vector2.Distance(GetChildCenterWorld(children[0]), center);

            layout.Radius = 160f;
            yield return null;
            float dist160 = Vector2.Distance(GetChildCenterWorld(children[0]), center);

            Assert.AreEqual(2f, dist160 / dist80, 0.1f,
                "Radius 翻倍时子物体到圆心的距离应翻倍");
        }

        [UnityTest]
        public IEnumerator Radius_Zero_AllChildrenAtCenter()
        {
            layout.Radius = 0f;
            layout.Rotation = 0;
            var children = CreateChildren(4, 20f);
            yield return null;

            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);

            foreach (var child in children)
            {
                float dist = Vector2.Distance(GetChildCenterWorld(child), center);
                Assert.AreEqual(0f, dist, 1f, "Radius=0 时所有子物体应堆叠在圆心");
            }
        }

        [UnityTest]
        public IEnumerator Radius_NegativeValue_Rejected()
        {
            layout.Radius = 100f;
            layout.Radius = -50f; // 应被拒绝
            Assert.AreEqual(100f, layout.Radius, 0.001f,
                "负值 Radius 应被拒绝，Radius 保持不变");
            yield break;
        }

        // ═══════════════════════════════════════════════════════════════════
        // 三、Rotation
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Rotation_90Degrees_RotatesAllChildren()
        {
            layout.Radius = 100f;
            var children = CreateChildren(1, 20f);

            layout.Rotation = 0;
            yield return null;
            Vector2 pos0 = GetChildCenterWorld(children[0]);

            layout.Rotation = 90;
            yield return null;
            Vector2 pos90 = GetChildCenterWorld(children[0]);

            // rotation=0 时子物体在圆右侧（Cos(0)=1，Sin(0)=0）
            // rotation=90 时子物体在圆上方（Cos(90)=0，Sin(90)=1）
            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);
            Vector2 dir0 = pos0 - center;
            Vector2 dir90 = pos90 - center;

            Assert.AreEqual(0f, dir0.y, 2f, "Rotation=0：子物体应在圆的右侧（Y 偏移≈0）");
            // CircleLayout 用 sin(angleDeg)，UGUI Y 轴向下（pos 增大 = 屏幕向下）
            // SetInsetAndSizeFromParentEdge(Top, pos) 中 pos 越大元素越靠下
            // 世界坐标 Y 轴向上，所以 sin(90°)=1 → 元素靠下 → dir90.y < 0
            Assert.Less(dir90.y, 0f, "Rotation=90：sin(90°)=1，UGUI Y 轴向下，元素应在圆心下方");
        }

        [UnityTest]
        public IEnumerator Rotation_Normalization_ArbitraryValues()
        {
            layout.Radius = 100f;
            var children = CreateChildren(1, 20f);

            layout.Rotation = 0;
            yield return null;
            Vector2 pos0 = GetChildCenterWorld(children[0]);

            layout.Rotation = 360;
            yield return null;
            Vector2 pos360 = GetChildCenterWorld(children[0]);

            layout.Rotation = 720;
            yield return null;
            Vector2 pos720 = GetChildCenterWorld(children[0]);

            Assert.AreEqual(pos0.x, pos360.x, 1f, "Rotation=360 应与 Rotation=0 位置相同");
            Assert.AreEqual(pos0.x, pos720.x, 1f, "Rotation=720 应与 Rotation=0 位置相同");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 四、ClockWise
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ClockWise_True_SecondChildBelowFirst()
        {
            layout.Radius = 100f;
            // 使用 3 个子物体，angleDelta=120°
            // rotation=0：child[0] 在正右（angle=0），child[1] 在右下 120°（顺时针）或右上 -120°（逆时针）
            // sin(120°)=0.866>0 → UGUI 向下 → 世界坐标 Y 更小（低于圆心）
            // sin(-120°)=-0.866<0 → UGUI 向上 → 世界坐标 Y 更大（高于圆心）
            layout.Rotation = 0;
            var children = CreateChildren(3, 20f);

            layout.ClockWise = true;
            yield return null;
            float yClockwise = GetChildCenterWorld(children[1]).y;

            layout.ClockWise = false;
            yield return null;
            float yCounterCW = GetChildCenterWorld(children[1]).y;

            var containerRt = containerGo.GetComponent<RectTransform>();
            float centerY = GetContainerCenterWorld(containerRt).y;

            // ClockWise=true，child[1] 在顺时针 120° 处，sin(120°)>0 → UGUI 向下 → 世界 Y < centerY
            Assert.Less(yClockwise, centerY, "ClockWise=true：第 2 个元素应在圆心下方");
            // ClockWise=false，child[1] 在逆时针 -120° 处，sin(-120°)<0 → UGUI 向上 → 世界 Y > centerY
            Assert.Greater(yCounterCW, centerY, "ClockWise=false：第 2 个元素应在圆心上方");
        }

        [UnityTest]
        public IEnumerator ClockWise_Toggle_MirrorsLayout()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(4, 20f);
            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);

            layout.ClockWise = true;
            yield return null;
            float[] cwAngles = new float[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 dir = GetChildCenterWorld(children[i]) - center;
                cwAngles[i] = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            layout.ClockWise = false;
            yield return null;
            float[] ccwAngles = new float[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 dir = GetChildCenterWorld(children[i]) - center;
                ccwAngles[i] = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            // 顺/逆时针互换时角度符号相反
            Assert.AreEqual(Mathf.Abs(cwAngles[1] - cwAngles[0]),
                            Mathf.Abs(ccwAngles[1] - ccwAngles[0]), 2f,
                "顺/逆时针切换时角度间距大小应相同");
            Assert.AreNotEqual(
                Mathf.Sign(cwAngles[1] - cwAngles[0]),
                Mathf.Sign(ccwAngles[1] - ccwAngles[0]),
                "顺/逆时针切换时排列方向应相反");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 五、preferredSize 上报
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator PreferredSize_EqualsRadiusTwoPlusPadding()
        {
            layout.Radius = 100f;
            layout.padding = new RectOffset(10, 20, 15, 25);
            CreateChildren(4, 20f);
            yield return null;

            float expectedW = layout.padding.horizontal + layout.Radius * 2; // 30 + 200
            float expectedH = layout.padding.vertical + layout.Radius * 2; // 40 + 200

            Assert.AreEqual(expectedW, layout.preferredWidth, 1f,
                "preferredWidth 应等于 radius*2 + padding.horizontal");
            Assert.AreEqual(expectedH, layout.preferredHeight, 1f,
                "preferredHeight 应等于 radius*2 + padding.vertical");
        }

        // ═══════════════════════════════════════════════════════════════════
        // 六、动态修改
        // ═══════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator DynamicAddChild_AnglesRecomputed()
        {
            layout.Radius = 100f;
            layout.Rotation = 0;
            var children = CreateChildren(3, 20f);
            var containerRt = containerGo.GetComponent<RectTransform>();
            Vector2 center = GetContainerCenterWorld(containerRt);
            yield return null;

            Vector2 dir0Before = GetChildCenterWorld(children[0]) - center;
            float angle0Before = Mathf.Atan2(dir0Before.y, dir0Before.x) * Mathf.Rad2Deg;

            // 添加第 4 个子物体后角度步进应变为 90°
            var go = new GameObject("NewChild");
            go.transform.SetParent(containerGo.transform, false);
            go.AddComponent<RectTransform>().sizeDelta = new Vector2(20f, 20f);
            go.AddComponent<Image>();
            yield return null;

            Vector2 dir0After = GetChildCenterWorld(children[0]) - center;
            float angle0After = Mathf.Atan2(dir0After.y, dir0After.x) * Mathf.Rad2Deg;

            // 第一个子物体角度不变（总是从 startAngle=rotation 开始）
            Assert.AreEqual(angle0Before, angle0After, 2f,
                "新增子物体后第一个子物体的角度应保持不变");
        }
    }
}