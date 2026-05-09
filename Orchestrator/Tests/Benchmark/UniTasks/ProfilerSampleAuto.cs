using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// 使用方式：
/// <code>
/// using var sample = ProfilerSampleAuto.Start("MyMethod");
/// // ... 你的业务逻辑 ...
/// </code>
/// 所有测试结束后调用 <see cref="SaveToCsv"/> 将数据写入文件。
/// </summary>
public class ProfilerSampleAuto : IDisposable
{
    private static readonly List<SampleEntry> sampleBuffer = new List<SampleEntry>();
    private static readonly ProfilerMarker dummyMarker = new ProfilerMarker("ProfilerSampleAuto.Dummy");
    private static readonly bool useGCAllocRecorder;

    // 全局 GC 分配计数器（如果可用）
    private static ProfilerRecorder gcRecorder;
    private static bool gcRecorderInitialized;

    private readonly string sampleName;
    private readonly ProfilerMarker marker;
    private readonly double startTimeMs;
    private readonly long gcAllocBefore;

    // 用于回退方案的临时字段
    private readonly bool fallbackMode;
    private readonly long allocBeforeFallback;

    static ProfilerSampleAuto()
    {
        // 尝试初始化 GC Alloc Recorder
        try
        {
            gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated Memory");
            useGCAllocRecorder = gcRecorder.Valid;
            gcRecorderInitialized = true;
        }
        catch
        {
            useGCAllocRecorder = false;
        }
    }

    private ProfilerSampleAuto(string name)
    {
        sampleName = name;
        marker = new ProfilerMarker(name);
        marker.Begin();

        startTimeMs = Time.realtimeSinceStartup * 1000.0;

        // 采用 ProfilerRecorder 或回退方案
        if (useGCAllocRecorder)
        {
            gcAllocBefore = gcRecorder.CurrentValue;
        }
        else
        {
            // 强制 GC 以获得稳定的基线（仅回退模式下）
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            allocBeforeFallback = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        }
    }

    /// <summary>
    /// 开始一个采样块。必须配合 using 语句或手动调用 Dispose。
    /// </summary>
    public static ProfilerSampleAuto Start(string name) => new ProfilerSampleAuto(name);

    public void Dispose()
    {
        marker.End();

        double elapsedMs = Time.realtimeSinceStartup * 1000.0 - startTimeMs;

        long gcAllocBytes;
        if (useGCAllocRecorder)
        {
            long gcAllocAfter = gcRecorder.CurrentValue;
            gcAllocBytes = gcAllocAfter - gcAllocBefore;
        }
        else
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long allocAfter = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            gcAllocBytes = allocAfter - allocBeforeFallback;
        }

        lock (sampleBuffer)
        {
            sampleBuffer.Add(new SampleEntry(sampleName, elapsedMs, gcAllocBytes));
        }
    }

    /// <summary>
    /// 将收集到的所有样本写入 CSV 文件。
    /// </summary>
    /// <param name="filePath">文件路径，例如 Application.persistentDataPath + "/profiling.csv"</param>
    public static void SaveToCsv(string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("SampleName, TimeMs, GCAllocBytes");
        lock (sampleBuffer)
        {
            foreach (var entry in sampleBuffer)
            {
                sb.AppendLine($"{entry.Name},{entry.TimeMs:F4},{entry.AllocBytes}");
            }
        }
        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"Profiling data saved to {filePath}");

        // 清空缓冲区，便于下次使用
        lock (sampleBuffer)
        {
            sampleBuffer.Clear();
        }
    }

    /// <summary>
    /// 清空已收集的样本（不写入文件）。
    /// </summary>
    public static void ClearBuffer()
    {
        lock (sampleBuffer)
        {
            sampleBuffer.Clear();
        }
    }

    private struct SampleEntry
    {
        public string Name;
        public double TimeMs;
        public long AllocBytes;

        public SampleEntry(string name, double timeMs, long allocBytes)
        {
            Name = name;
            TimeMs = timeMs;
            AllocBytes = allocBytes;
        }
    }
}