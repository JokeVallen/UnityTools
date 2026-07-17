using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewPipeline.Unity.Core
{
    internal static class Validation
    {
        private static readonly Dictionary<Guid, HashSet<IMiddlewareValidator>> middlewareValidators = new Dictionary<Guid, HashSet<IMiddlewareValidator>>();

        /// <summary>
        /// 注册验证器
        /// </summary>
        public static void RegisterValidator(Guid key, IMiddlewareValidator validator)
        {
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (!middlewareValidators.TryGetValue(key, out var hashset)) 
            { 
                hashset = new HashSet<IMiddlewareValidator>();
                middlewareValidators.Add(key, hashset);
            }

            hashset.Add(validator);
        }

        /// <summary>
        /// 执行所有已注册的验证器
        /// </summary>
        public static void ValidateAll(Guid key, IReadOnlyList<IViewMiddleware> middlewares)
        {
            if (!middlewareValidators.TryGetValue(key, out var hashset)) return;

            var errors = new List<ValidationError>();
            foreach (var validator in hashset)
            {
                validator.Validate(middlewares, errors);
            }

            if (errors.Any(e => e.Severity == ValidationSeverity.Error))
            {
                var messages = string.Join("\n", errors.Select(e => e.Message));
                throw new InvalidOperationException($"[ViewPipeline] Middleware validation failed：\n{messages}");
            }

            foreach (var warning in errors.Where(e => e.Severity == ValidationSeverity.Warning))
            {
                Log.Logger.Warning(warning.Message);
            }
        }

        /// <summary>
        /// 异步释放资源
        /// </summary>
        /// <param name="key">标识符</param>
        /// <returns>异步任务实例</returns>
        public static async UniTask DisposeAsync(Guid key) 
        {
            if (!middlewareValidators.TryGetValue(key, out var hashset)) return;
            foreach (var validator in hashset)
            {
                if(validator is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                (validator as IDisposable)?.Dispose();
            }
            middlewareValidators.Remove(key);
        }

        /// <summary>
        /// 同步释放资源
        /// </summary>
        /// <param name="key">标识符</param>
        public static void Dispose(Guid key) 
        {
            if (!middlewareValidators.TryGetValue(key, out var hashset)) return;
            foreach (var validator in hashset)
                (validator as IDisposable)?.Dispose();
            middlewareValidators.Remove(key);
        }
    }
}
