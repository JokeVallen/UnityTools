using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EasyAttributes.Core
{
    internal abstract class Context<TAttr> : IContext, IContextWriter where TAttr : EasyAttribute
    {
        public TAttr Attribute { get; }
        EasyAttribute IContext.Attribute => Attribute;

        public IReadOnlyDictionary<string, object> Items
        {
            get
            {
                if (readOnlyItems == null)
                    readOnlyItems = new ReadOnlyDictionary<string, object>(items);
                return readOnlyItems;
            }
        }
        private readonly Dictionary<string, object> items = new Dictionary<string, object>();
        private ReadOnlyDictionary<string, object> readOnlyItems;

        public IReadOnlyDictionary<Type, IFeature> Features
        {
            get
            {
                if (readOnlyFeatures == null)
                    readOnlyFeatures = new ReadOnlyDictionary<Type, IFeature>(features);
                return readOnlyFeatures;
            }
        }
        private readonly Dictionary<Type, IFeature> features = new Dictionary<Type, IFeature>();
        private ReadOnlyDictionary<Type, IFeature> readOnlyFeatures;

        public bool IsEnabled { get; }
        public int Priority { get; }

        protected Context(TAttr attribute)
        {
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            IsEnabled = attribute.Enabled;
            Priority = attribute.Priority;
        }

        void IContextWriter.SetItem(string key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            items[key] = value;
        }

        void IContextWriter.RemoveItem(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            items.Remove(key);
        }

        void IContextWriter.SetFeature(Type featureType, IFeature feature)
        {
            if (featureType == null) throw new ArgumentNullException(nameof(featureType));
            if (feature == null) throw new ArgumentNullException(nameof(feature));
            if (!typeof(IFeature).IsAssignableFrom(featureType))
                throw new ArgumentException($"The type '{featureType.FullName}' does not implement {nameof(IFeature)}.", nameof(featureType));
            features[featureType] = feature;
        }

        void IContextWriter.RemoveFeature(Type featureType)
        {
            if (featureType == null) throw new ArgumentNullException(nameof(featureType));
            if (features == null) return;
            features.Remove(featureType);
        }
    }
}