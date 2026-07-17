using System;
using System.Collections.Generic;

namespace ViewPipeline.Unity.Core
{
    internal static class ViewPipelineUtility
    {
        public static IViewMiddleware[] FilterAndToArray(IEnumerable<IViewMiddleware> middlewares)
        {
            if (middlewares == null) return new IViewMiddleware[0];
            var list = new List<IViewMiddleware>();
            foreach (var m in middlewares)
            {
                if (m != null) 
                    list.Add(m);
            }
            return list.ToArray();
        }

        public static bool Validate(object obj)
        {
            if (!(obj is IValidatable)) return true;
            var validatable = obj as IValidatable;
            var result = validatable.GetValidator().Validate();

            if (result.IsValid)
            {
                if (result.Severity == ValidationSeverity.Warning)
                    Log.Logger.Warning($"[ViewPipeline] The component '{obj.GetType()}' passed the precondition validation: ({result.Severity}) {result.Message}");
                return true;
            }

            Log.Logger.Error($"[ViewPipeline] The component '{obj.GetType()}' failed the precondition validation: ({result.Severity}) {result.Message}");
            return false;
        }

        public static bool ShouldSkipView(IViewMiddleware middleware, IView view) 
        {
            if (!(middleware is ISkippableMiddleware)) return false;
            var viewSkippable = middleware as ISkippableMiddleware;
            return viewSkippable.ShouldSkip(view);
        }

        public static bool ShouldSkipView(Guid key, IViewMiddleware middleware, IView view)
        {
            var skipped = ShouldSkipView(middleware, view);
            if (!skipped) skipped = ExecutionPolicy.ShouldSkipView(key, middleware, view);
            return skipped;
        }

        public static bool ShouldSkipMiddleware(IView view, IViewMiddleware middleware)
        {
            if (!(view is ISkippableView)) return false;
            var viewSkippable = view as ISkippableView;
            return viewSkippable.ShouldSkip(middleware);
        }

        public static bool ShouldSkipMiddleware(Guid key, IView view, IViewMiddleware middleware)
        {
            var skipped = ShouldSkipMiddleware(view, middleware);
            if (!skipped) skipped = ExecutionPolicy.ShouldSkipMiddleware(key, view, middleware);
            return skipped;
        }

        public static bool ShouldTerminate(IView view) 
        {
            if (!(view is ITerminable)) return false;
            var terminable = view as ITerminable;
            return terminable.ShouldTerminate();
        }

        public static bool ShouldTerminate(Guid key, IView view)
        {
            var terminated = ShouldTerminate(view);
            if (!terminated) terminated = ExecutionPolicy.ShouldTerminate(key, view);
            return terminated;
        }

        public static bool ShouldTerminate(IViewMiddleware middleware)
        {
            if (!(middleware is ITerminable)) return false;
            var terminable = middleware as ITerminable;
            return terminable.ShouldTerminate();
        }

        public static bool ShouldTerminate(Guid key, IViewMiddleware middleware)
        {
            var terminated = ShouldTerminate(middleware);
            if (!terminated) terminated = ExecutionPolicy.ShouldTerminate(key, middleware);
            return terminated;
        }
    }
}
