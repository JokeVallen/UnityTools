namespace EasyMapper.Runtime
{
    /// <summary> 可配置的映射实例 </summary>
    /// <remarks>
    /// <para> 持有字符串流水线、对象流水线和序列化器，对外暴露与 <see cref="IDMap"/> 相同的方法。 </para>
    /// <para> 通过内部类 <see cref="IDMapInstance.Builder"/> 按需构建，支持替换任意组件。 </para>
    /// </remarks>
    public sealed class IDMapInstance
    {
        private IPipeline<string, LongToken> stringPipeline;
        private IPipeline<UnityEngine.Object, LongToken> objectPipeline;
        private IPackage<LongToken> longPackage;

        private IDMapInstance() { }

        /// <summary> 为字符串分配令牌 </summary>
        public long Assign(string name) => stringPipeline.Import(name).Value;
        /// <summary> 查找字符串 </summary>
        public string Locate(long id) => stringPipeline.Export(new LongToken(id));
        /// <summary> 查询字符串令牌是否存在 </summary>
        public bool ContainsString(long id) => stringPipeline.Export(new LongToken(id)) != null;

        /// <summary> 为对象分配令牌 </summary>
        public long Assign(UnityEngine.Object obj) => objectPipeline.Import(obj).Value;
        /// <summary> 查找存活对象 </summary>
        public T Locate<T>(long id) where T : UnityEngine.Object => objectPipeline.Export(new LongToken(id)) as T;
        /// <summary> 查询对象令牌是否存活 </summary>
        public bool ContainsObject(long id) => objectPipeline.Export(new LongToken(id)) != null;

        /// <summary> 序列化令牌 </summary>
        public byte[] Pack(long id) => longPackage.Wrap(new LongToken(id));
        /// <summary> 反序列化令牌 </summary>
        public long Unpack(byte[] bytes) => longPackage.Unwrap(bytes).Value;
        /// <summary> 清理所有映射 </summary>
        public void Cleanup()
        {
            (stringPipeline as IMaintainable)?.Cleanup();
            (objectPipeline as IMaintainable)?.Cleanup();
        }

        /// <summary> 构建器 </summary>
        /// <remarks>
        /// <para> 使用 <c>Builder.Create()</c> 获取实例，通过流式方法配置后调用 <c>Build()</c> 生成 <see cref="IDMapInstance"/>。 </para>
        /// <para> 构建器不可重复使用，构建后任何修改尝试将抛出异常。 </para>
        /// </remarks>
        public class Builder
        {
            private IPipeline<string, LongToken> stringPipeline;
            private IPipeline<UnityEngine.Object, LongToken> objectPipeline;
            private IPackage<LongToken> longPackage;
            private bool built;

            private Builder()
            {
                var stringDistributor = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());
                var objectDistributor = new SmartDistributor(new Char10PackingBlueprint(), new InterningBlueprint());

                stringPipeline = new StandardPipeline<string, LongToken>(stringDistributor, stringDistributor);
                objectPipeline = new UnityWeakPipeline<UnityEngine.Object, LongToken>(new ObjectNamingBlueprint(objectDistributor), objectDistributor);
                longPackage = new BinaryIdentityPackage();
            }

            /// <summary> 创建新的构建器实例 </summary>
            public static Builder Create() => new Builder();

            /// <summary> 设置自定义字符串流水线 </summary>
            public Builder UseStringPipeline(IPipeline<string, LongToken> pipeline)
            {
                ThrowErrorIfReuse();
                stringPipeline = pipeline ?? throw new System.ArgumentNullException(nameof(pipeline));
                return this;
            }

            /// <summary> 设置自定义 Unity 对象流水线 </summary>
            public Builder UseObjectPipeline(IPipeline<UnityEngine.Object, LongToken> pipeline)
            {
                ThrowErrorIfReuse();
                objectPipeline = pipeline ?? throw new System.ArgumentNullException(nameof(pipeline));
                return this;
            }

            /// <summary> 设置自定义 LongToken 序列化器 </summary>
            public Builder UseLongPackage(IPackage<LongToken> package)
            {
                ThrowErrorIfReuse();
                longPackage = package ?? throw new System.ArgumentNullException(nameof(package));
                return this;
            }

            /// <summary> 构建 <see cref="IDMapInstance"/> </summary>
            public IDMapInstance Build()
            {
                ThrowErrorIfReuse();
                built = true;
                return new IDMapInstance()
                {
                    stringPipeline = stringPipeline,
                    objectPipeline = objectPipeline,
                    longPackage = longPackage
                };
            }

            private void ThrowErrorIfReuse()
            {
                if (built) throw new System.InvalidOperationException("You cannot reuse the builder instance.");
            }
        }
    }
}