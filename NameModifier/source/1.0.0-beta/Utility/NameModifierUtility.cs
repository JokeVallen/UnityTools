#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace EditorTools.NameModifier
{
    internal static class NameModifierUtility
    {
        /// <summary>
        /// 工具类：GUID
        /// </summary>
        public static class GUIDUtility
        {
            private const string FILE_NAME = "ProjectQualified.guid";
            private static readonly string s_FileFullPath;

            static GUIDUtility()
            {
                string path = Application.dataPath;
                int lastIndex = path.LastIndexOf("Assets");
                if (lastIndex < 0)
                {
                    s_FileFullPath = string.Empty;
                    return;
                }

                path = path.Substring(0, lastIndex);
                s_FileFullPath = Path.Combine(path, "ProjectSettings", FILE_NAME);
            }

            /// <summary>
            /// 获取或创建项目唯一GUID
            /// </summary>
            public static string GetOrCreateProjectQualifiedGUID()
            {
                if (File.Exists(s_FileFullPath))
                {
                    return File.ReadAllText(s_FileFullPath);
                }
                else
                {
                    string key = Guid.NewGuid().ToString();
                    File.WriteAllText(s_FileFullPath, key);
                    return key;
                }
            }
        }

        /// <summary>
        /// 从只读列表中查找值的索引
        /// </summary>
        /// <typeparam name="T">只读列表中值的类型</typeparam>
        /// <param name="source">只读列表</param>
        /// <param name="match">匹配表达式</param>
        /// <returns>若找到则返回对应的索引，否则返回-1</returns>
        public static int FindIndex<T>(this IReadOnlyList<T> source, Func<T, bool> match)
        {
            if (source == null || match == null) return -1;

            int count = source.Count;
            for (int i = 0; i < count; i++)
            {
                T value = source[i];
                if (match(value)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 获取基于Assets目录的相对路径
        /// </summary>
        public static string GetAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            int lastIndex = path.LastIndexOf("Assets");
            if (lastIndex < 0) return string.Empty;

            return path.Substring(lastIndex, path.Length - lastIndex);
        }
    }
}

#endif