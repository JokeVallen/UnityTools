using EasyProgress.Core;

namespace EasyProgress.UnitTests
{
    internal class SpyLeafManager : ILeafManager<double>
    {
        private readonly List<IProgressLeaf<double>> _released = new List<IProgressLeaf<double>>();
        public IReadOnlyList<IProgressLeaf<double>> ReleasedLeaves => _released;
        public int ReleasedCount => _released.Count;

        public IProgressLeaf<double> AcquireLeaf() => new DefaultLeaf();

        public void ReleaseLeaf(IProgressLeaf<double> leaf)
        {
            _released.Add(leaf);
            if (leaf is IResettable resettable) resettable.Reset();
        }

        public void Clear() => _released.Clear();
    }
}
