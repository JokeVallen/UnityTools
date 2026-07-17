namespace EasyAttributes.Core
{
    public static partial class Extensions
    {
        /// <summary>安全获取结果</summary>
        public static TResult GetResult<TResult>(this IProcessorHandle handle, TResult defaultValue = default)
        {
            if (handle.Result is TResult result)
                return result;
            return defaultValue;
        }

        /// <summary>尝试获取结果</summary>
        public static bool TryGetResult<TResult>( this IProcessorHandle handle, out TResult result)
        {
            if (handle.Result is TResult r)
            {
                result = r;
                return true;
            }
            result = default;
            return false;
        }
    }
}
