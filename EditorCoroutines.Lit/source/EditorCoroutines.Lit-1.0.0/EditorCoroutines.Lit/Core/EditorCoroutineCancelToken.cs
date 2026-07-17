#if UNITY_EDITOR

/// <summary>
/// 编辑器协程取消Token
/// </summary>
public class EditorCoroutineCancelToken
{
    /// <summary>
    /// 是否已取消
    /// </summary>
    public bool IsCancelled => isCancelled;
    private bool isCancelled;

    /// <summary>
    /// 取消
    /// </summary>
    public void Cancel()
    {
        isCancelled = true;
    }
}

#endif