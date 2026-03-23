#if UNITY_EDITOR

using System;
using NUnit.Framework;

namespace EditorTools.NameModifier.Tests
{
    [TestFixture]
    internal sealed class GroupNameFormatterTests
    {
        [Test]
        public void Format_NullTemplate_ReturnsNull()
        {
            Assert.IsNull(GroupNameFormatter.Format(null));
        }

        [Test]
        public void Format_WhitespaceTemplate_ReturnsWhitespace()
        {
            Assert.AreEqual("   ", GroupNameFormatter.Format("   "));
        }

        [Test]
        public void Format_NoTokens_ReturnsOriginal()
        {
            Assert.AreEqual("MyGroup", GroupNameFormatter.Format("MyGroup"));
        }

        [Test]
        public void Format_DateToken_ReplacedWithCurrentDate()
        {
            string result = GroupNameFormatter.Format(GroupNameFormatter.TOKEN_DATE);
            string expected = DateTime.Now.ToString("yyyy-MM-dd");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Format_TimeToken_ReplacedWithCurrentTime()
        {
            // 只校验格式，不校验精确值（测试执行有时间差）
            string result = GroupNameFormatter.Format(GroupNameFormatter.TOKEN_TIME);

            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(result, @"^\d{2}:\d{2}:\d{2}$"),
                $"期望 HH:mm:ss 格式，实际为：{result}");
        }

        [Test]
        public void Format_DateTimeToken_ReplacedWithDateAndTime()
        {
            string result = GroupNameFormatter.Format(GroupNameFormatter.TOKEN_DATETIME);

            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(result, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"),
                $"期望 yyyy-MM-dd HH:mm:ss 格式，实际为：{result}");
        }

        [Test]
        public void Format_DateTimeToken_NotCorruptedByDateToken()
        {
            // {DateTime} 必须先于 {Date} 被替换，否则 {DateTime} 会被拆成 {Date}Time}
            string result = GroupNameFormatter.Format(GroupNameFormatter.TOKEN_DATETIME);

            StringAssert.DoesNotContain("{Date}", result);
            StringAssert.DoesNotContain("{Time}", result);
        }

        [Test]
        public void Format_MixedTemplate_ReplacesAllTokens()
        {
            string template = $"prefix_{GroupNameFormatter.TOKEN_DATE}_suffix";
            string result = GroupNameFormatter.Format(template);
            string expectedDate = DateTime.Now.ToString("yyyy-MM-dd");

            Assert.AreEqual($"prefix_{expectedDate}_suffix", result);
        }

        [Test]
        public void Format_MultipleTokensInTemplate_AllReplaced()
        {
            string template = $"{GroupNameFormatter.TOKEN_DATE}_{GroupNameFormatter.TOKEN_TIME}";
            string result = GroupNameFormatter.Format(template);

            StringAssert.DoesNotContain("{Date}", result);
            StringAssert.DoesNotContain("{Time}", result);
        }

        [Test]
        public void ContainsTokens_NullTemplate_ReturnsFalse()
        {
            Assert.IsFalse(GroupNameFormatter.ContainsTokens(null));
        }

        [Test]
        public void ContainsTokens_NoTokens_ReturnsFalse()
        {
            Assert.IsFalse(GroupNameFormatter.ContainsTokens("MyGroup"));
        }

        [Test]
        public void ContainsTokens_WithDateToken_ReturnsTrue()
        {
            Assert.IsTrue(GroupNameFormatter.ContainsTokens($"grp_{GroupNameFormatter.TOKEN_DATE}"));
        }

        [Test]
        public void ContainsTokens_WithTimeToken_ReturnsTrue()
        {
            Assert.IsTrue(GroupNameFormatter.ContainsTokens($"grp_{GroupNameFormatter.TOKEN_TIME}"));
        }

        [Test]
        public void ContainsTokens_WithDateTimeToken_ReturnsTrue()
        {
            Assert.IsTrue(GroupNameFormatter.ContainsTokens($"grp_{GroupNameFormatter.TOKEN_DATETIME}"));
        }
    }
}

#endif
