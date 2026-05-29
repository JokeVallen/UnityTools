using EasyProgress.Core;

namespace EasyProgress.UnitTests
{
    internal class SpyCompositeManager : ICompositeManager<double>
    {
        private readonly List<IProgressComposite<double>> _released = new List<IProgressComposite<double>>();
        public IReadOnlyList<IProgressComposite<double>> ReleasedComposites => _released;

        public IProgressComposite<double> AcquireComposite(ICompositionRule<double> rule) => new RealtimeComposite(rule);

        public void ReleaseComposite(IProgressComposite<double> composite)
        {
            _released.Add(composite);
            if (composite is IResettable resettable) resettable.Reset();
        }
    }
}
