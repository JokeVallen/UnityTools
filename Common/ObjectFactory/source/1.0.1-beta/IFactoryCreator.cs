using System;

/// <summary>
/// 工厂创建器接口
/// </summary>
public interface IFactoryCreator
{
    /// <summary>
    /// 工厂类型
    /// </summary>
    Type FactoryType { get; }

    /// <summary>
    /// 创建工厂实例
    /// </summary>
    /// <returns>工厂实例</returns>
    IObjectFactory Create();
}

/// <summary>
/// 工厂创建器接口
/// </summary>
/// <typeparam name="T">工厂类型</typeparam>
public interface IFactoryCreator<T> where T : IObjectFactory
{
    /// <summary>
    /// 创建工厂实例
    /// </summary>
    /// <returns>工厂实例</returns>
    T Create();
}