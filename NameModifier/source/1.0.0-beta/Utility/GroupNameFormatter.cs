#if UNITY_EDITOR

using System;
using System.Text;

namespace EditorTools.NameModifier
{
    internal static class GroupNameFormatter
    {
        internal const string TOKEN_DATE = "{Date}";
        internal const string TOKEN_TIME = "{Time}";
        internal const string TOKEN_DATETIME = "{DateTime}";

        internal static string Format(string template)
        {
            if (string.IsNullOrWhiteSpace(template)) return template;

            DateTime now = DateTime.Now;
            var sb = new StringBuilder(template);
            sb.Replace(TOKEN_DATETIME, now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Replace(TOKEN_DATE, now.ToString("yyyy-MM-dd"));
            sb.Replace(TOKEN_TIME, now.ToString("HH:mm:ss"));
            return sb.ToString();
        }

        internal static bool ContainsTokens(string template)
        {
            if (string.IsNullOrWhiteSpace(template)) return false;
            return template.Contains(TOKEN_DATE)
                || template.Contains(TOKEN_TIME)
                || template.Contains(TOKEN_DATETIME);
        }
    }
}

#endif
