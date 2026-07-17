namespace EasyMapper.Runtime
{
    /// <summary> Unity 对象名称适配蓝图 </summary>
    /// <remarks>
    /// <para> 提取 <see cref="UnityEngine.Object.name"/> 并委托给内部的字符串蓝图生成令牌，实现从 UnityEngine.Object 到令牌的适配。 </para>
    /// <para> 不可溯源，需要配合 <see cref="UnityWeakPipeline{TSource, TToken}"/> 等流水线存储反向引用。 </para>
    /// </remarks>
    public sealed class ObjectNamingBlueprint : IBlueprint<UnityEngine.Object, LongToken>, IFeature
    {
        public bool IsTraceable => false;
        private readonly IBlueprint<string, LongToken> nameDistributor;

        /// <param name="nameDistributor"> 字符串令牌生成蓝图 </param>
        public ObjectNamingBlueprint(IBlueprint<string, LongToken> nameDistributor)
        {
            this.nameDistributor = nameDistributor;
        }

        public LongToken Refine(UnityEngine.Object source) => nameDistributor.Refine(source.name);
        public UnityEngine.Object Restore(LongToken token) => throw new System.NotSupportedException();
    }
}