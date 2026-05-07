using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

[InitializeOnLoad]
public static class TemplateInitializer
{
    private const string preloadFolder = ".Assets";
    private const string BlacklistFileName = "package_blacklist.json";

    static TemplateInitializer()
    {
        Debug.Log("开启模板初始化程序...");
        EditorApplication.delayCall += StartUPMResolve;
    }

    private static void StartUPMResolve()
    {
        Debug.Log("开始处理UPM...");
        UnityEditor.PackageManager.Client.Resolve();
        EditorApplication.delayCall += ExecuteSanitize;
    }

    private static void ExecuteSanitize()
    {
        string scriptPath = GetScriptPath();
        if (string.IsNullOrEmpty(scriptPath)) return;

        string scriptDir = Path.GetDirectoryName(scriptPath);
        string blacklistPath = Path.Combine(scriptDir, BlacklistFileName);
        string manifestPath = Path.GetFullPath("Packages/manifest.json");
        string preloadPath = Path.Combine(Application.dataPath, preloadFolder);

        if (!File.Exists(manifestPath))
        {
            Debug.Log($"未找到 manifest.json 文件: 跳过清理。");
            SelfDestruct(scriptPath, blacklistPath);
            return;
        }

        if (!File.Exists(blacklistPath))
        {
            Debug.Log($"未找到 {BlacklistFileName} 黑名单文件: 跳过清理。");
            SelfDestruct(scriptPath, blacklistPath);
            return;
        }

        Debug.Log($"开始解析 {BlacklistFileName} 黑名单文件...");
        var blacklist = ParseJsonKeys(File.ReadAllText(blacklistPath)).Keys.ToList();
        Debug.Log($"{BlacklistFileName} 黑名单文件解析完成。");
        Debug.Log("开始读取 manifest.json 文件内容...");
        string originalContent = File.ReadAllText(manifestPath);
        Debug.Log("manifest.json 文件内容读取完成。");

        Debug.Log("开始解析 manifest.json 文件内容...");
        string pattern = @"(""dependencies""\s*:\s*\{)(.*?)(\s*\})";
        var match = Regex.Match(originalContent, pattern, RegexOptions.Singleline);
        if (match.Success)
        {
            string prefix = match.Groups[1].Value;
            string body = match.Groups[2].Value;
            string suffix = match.Groups[3].Value;

            var deps = ParseJsonKeys(body);
            Debug.Log("manifest.json 文件内容解析完成。");
            Debug.Log("开始根据黑名单剔除 manifest.json 中的资源包...");
            var filteredDeps = deps.Where(kvp => !blacklist.Contains(kvp.Key)).ToList();
            Debug.Log("manifest.json 文件内容已剔除黑名单资源包。");

            Debug.Log("开始重构 manifest.json 文件内容...");
            StringBuilder newBody = new StringBuilder();
            newBody.AppendLine("");
            for (int i = 0; i < filteredDeps.Count; i++)
            {
                string comma = (i == filteredDeps.Count - 1) ? "" : ",";
                newBody.AppendLine($"\t\t\"{filteredDeps[i].Key}\": \"{filteredDeps[i].Value}\"{comma}");
            }
            newBody.Append("\t");

            string newContent = originalContent.Replace(match.Value, prefix + newBody.ToString() + suffix);
            Debug.Log("manifest.json 文件内容重构完成。");
            Debug.Log("开始为 manifest.json 文件写入重构内容...");
            File.WriteAllText(manifestPath, newContent);
            Debug.Log("manifest.json 文件写入重构内容完成。");
        }
        else
        {
            Debug.Log("manifest.json 文件内容未解析到 'dependencies' 相关配置项。");
        }

        Debug.Log("开始处理预加载目录...");
        if (Directory.Exists(preloadPath))
        {
            CopyDirectoryContents(preloadPath, Application.dataPath, true);
            Debug.Log("预加载目录已处理完成。");
            Debug.Log("正在删除预加载目录...");
            FileUtil.DeleteFileOrDirectory(preloadPath);
            Debug.Log("预加载目录已删除。");
        }
        else
        {
            Debug.Log("预加载目录不存在。");
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        SelfDestruct(scriptPath, blacklistPath);
    }

    private static Dictionary<string, string> ParseJsonKeys(string json)
    {
        var dict = new Dictionary<string, string>();
        var matches = Regex.Matches(json, @"\""([^\""]+)\""\s*:\s*\""([^\""]+)\""");
        foreach (Match m in matches)
        {
            if (!dict.ContainsKey(m.Groups[1].Value))
                dict.Add(m.Groups[1].Value, m.Groups[2].Value);
        }
        return dict;
    }

    private static string GetScriptPath()
    {
        string[] guids = AssetDatabase.FindAssets($"{nameof(TemplateInitializer)} t:Script");
        return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
    }

    private static void SelfDestruct(string scriptPath, string blacklistPath)
    {
        Debug.Log("脚本自毁程序已启动...");
        EditorApplication.delayCall += () =>
        {
            if (File.Exists(blacklistPath)) AssetDatabase.DeleteAsset(blacklistPath);
            if (File.Exists(scriptPath)) AssetDatabase.DeleteAsset(scriptPath);
            Debug.Log("自毁工作已完成...");
            AssetDatabase.SaveAssets();
            Debug.Log("初始化环境已清理完毕。");
        };
    }

    private static void CopyDirectoryContents(string sourceDir, string destDir, bool overwrite = true)
    {
        if (!Directory.Exists(sourceDir))
            return;

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(filePath);
            string destFilePath = Path.Combine(destDir, fileName);
            File.Copy(filePath, destFilePath, overwrite);
        }

        foreach (string subDirPath in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDirPath);
            string destSubDirPath = Path.Combine(destDir, dirName);
            CopyDirectoryContents(subDirPath, destSubDirPath, overwrite);
        }
    }
}