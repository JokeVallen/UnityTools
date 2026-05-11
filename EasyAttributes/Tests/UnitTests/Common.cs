using EasyAttributes.Core;

namespace EasyAttributes.UnitTests
{
    internal class TestFeature : IFeature
    {
        public string Name { get; set; } = "Test";
    }

    internal class MockContext : IContext, IContextWriter
    {
        public EasyAttribute Attribute { get; set; } = null!;
        public IReadOnlyDictionary<string, object> Items
        {
            get => new Dictionary<string, object>(_items);
            set { _items.Clear(); foreach (var kv in value) _items.Add(kv.Key, kv.Value); }
        }
        public IReadOnlyDictionary<Type, IFeature> Features
        {
            get => new Dictionary<Type, IFeature>(_features);
            set { _features.Clear(); foreach (var kv in value) _features.Add(kv.Key, kv.Value); }
        }
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; }

        private readonly Dictionary<string, object> _items = new();
        private readonly Dictionary<Type, IFeature> _features = new();

        void IContextWriter.SetItem(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            _items[key] = value;
        }

        void IContextWriter.RemoveItem(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _items.Remove(key);
        }

        void IContextWriter.SetFeature(Type featureType, IFeature feature)
        {
            if (featureType == null) throw new ArgumentNullException(nameof(featureType));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (!typeof(IFeature).IsAssignableFrom(featureType))
                throw new ArgumentException($"Type {featureType.FullName} does not implement IFeature.");
            _features[featureType] = feature;
        }

        void IContextWriter.RemoveFeature(Type featureType)
        {
            if (featureType == null) throw new ArgumentNullException(nameof(featureType));
            _features.Remove(featureType);
        }
    }

    internal class FakeRegistry : IProcessorRegistry
    {
        private readonly Dictionary<Type, List<ProcessorDescriptor>> _map = new();

        public FakeRegistry(Type attributeType, params Type[] processorTypes)
        {
            foreach (var pt in processorTypes)
            {
                var descriptor = new ProcessorDescriptor(attributeType, pt);
                if (!_map.ContainsKey(attributeType))
                    _map[attributeType] = new List<ProcessorDescriptor>();
                _map[attributeType].Add(descriptor);
            }
        }

        public IReadOnlyList<ProcessorDescriptor> GetDescriptors(Type attributeType)
            => _map.TryGetValue(attributeType, out var list) ? list : Array.Empty<ProcessorDescriptor>();

        public bool HasProcessors(Type attributeType) => _map.ContainsKey(attributeType);
    }

    internal class FakeProcessorFactory : IProcessorFactory
    {
        private readonly Dictionary<Type, Queue<object>> _queues = new();

        public FakeProcessorFactory(params object[] processors)
        {
            foreach (var p in processors)
            {
                var type = p.GetType();
                if (!_queues.ContainsKey(type))
                    _queues[type] = new Queue<object>();
                _queues[type].Enqueue(p);
            }
        }

        public object Create(Type processorType)
        {
            if (_queues.TryGetValue(processorType, out var queue) && queue.Count > 0)
                return queue.Dequeue();
            throw new InvalidOperationException($"No fake instance available for {processorType.FullName}");
        }
    }

    internal class SpySyncProcessor : IProcessor
    {
        private readonly List<string> _callOrder;

        public SpySyncProcessor(List<string> callOrder) => _callOrder = callOrder;

        public void Before(IContext context) => _callOrder.Add("Sync.Before");
        public IProcessorHandle Process(IContext context)
        {
            _callOrder.Add("Sync.Process");
            return ProcessorHandle.Continue;
        }
        public void After(IContext context) => _callOrder.Add("Sync.After");
    }

    internal class SpyAsyncProcessor : IProcessor, IProcessorAsync
    {
        private readonly List<string> _callOrder;

        public SpyAsyncProcessor(List<string> callOrder) => _callOrder = callOrder;

        public void Before(IContext context) => _callOrder.Add("Async.Before");
        public IProcessorHandle Process(IContext context)
        {
            _callOrder.Add("Async.Process");
            return ProcessorHandle.Continue;
        }
        public void After(IContext context) => _callOrder.Add("Async.After");

        public Task BeforeAsync(IContext context)
        {
            _callOrder.Add("Async.Before");
            return Task.CompletedTask;
        }
        public Task<IProcessorHandle> ProcessAsync(IContext context)
        {
            _callOrder.Add("Async.Process");
            return Task.FromResult<IProcessorHandle>(ProcessorHandle.Continue);
        }
        public Task AfterAsync(IContext context)
        {
            _callOrder.Add("Async.After");
            return Task.CompletedTask;
        }
    }

    internal class CountingProcessor : IProcessor
    {
        public bool BeforeCalled { get; private set; }
        public bool ProcessCalled { get; private set; }
        public bool AfterCalled { get; private set; }

        public void Before(IContext context) => BeforeCalled = true;
        public IProcessorHandle Process(IContext context)
        {
            ProcessCalled = true;
            return ProcessorHandle.Continue;
        }
        public void After(IContext context) => AfterCalled = true;
    }

    internal class AbortProcessor : IProcessor
    {
        private readonly bool _abort;
        private readonly bool _skipAfter;

        public bool BeforeCalled { get; private set; }
        public bool ProcessCalled { get; private set; }
        public bool AfterCalled { get; private set; }

        public AbortProcessor(bool abort = true, bool skipAfter = false)
        {
            _abort = abort;
            _skipAfter = skipAfter;
        }

        public void Before(IContext context) => BeforeCalled = true;
        public IProcessorHandle Process(IContext context)
        {
            ProcessCalled = true;
            if (_abort)
                return _skipAfter ? ProcessorHandle.AbortedAll : ProcessorHandle.Aborted;
            return ProcessorHandle.Continue;
        }
        public void After(IContext context) => AfterCalled = true;
    }

    internal class ThrowingBeforeProcessor : IProcessor
    {
        public bool AfterCalled { get; private set; }
        public void Before(IContext context) => throw new InvalidOperationException("Before error");
        public IProcessorHandle Process(IContext context) => ProcessorHandle.Continue;
        public void After(IContext context) => AfterCalled = true;
    }

    internal class ThrowingAfterProcessor : IProcessor
    {
        public bool BeforeCalled { get; private set; }
        public bool ProcessCalled { get; private set; }
        public bool AfterCalled { get; private set; }

        public void Before(IContext context) => BeforeCalled = true;
        public IProcessorHandle Process(IContext context)
        {
            ProcessCalled = true;
            return ProcessorHandle.Continue;
        }
        public void After(IContext context)
        {
            AfterCalled = true;
            throw new InvalidOperationException("After error");
        }
    }

    internal class MemorizingExceptionHandler : IExceptionHandler
    {
        private readonly bool _shouldHandle;

        public bool WasCalled { get; private set; }
        public EasyAttributeException? Exception { get; private set; }

        public MemorizingExceptionHandler(bool shouldHandle) => _shouldHandle = shouldHandle;

        public bool Handle(EasyAttributeException exception)
        {
            WasCalled = true;
            Exception = exception;
            return _shouldHandle;
        }
    }

    internal class TestAttribute : EasyAttribute { }
    internal class AnotherTestAttribute : EasyAttribute { }
}
