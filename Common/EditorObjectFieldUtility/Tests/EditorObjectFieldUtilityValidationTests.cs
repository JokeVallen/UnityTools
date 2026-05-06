using NUnit.Framework;
using System;
using UnityEngine;

public class EditorObjectFieldUtilityValidationTests
{
    #region 参数校验

    [Test]
    public void NoPickerObjectField_WithNullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            EditorObjectFieldUtility.NoPickerObjectField(new Rect(), null, null, (Type)null));
    }

    [Test]
    public void NoPickerObjectField_WithNonUnityObjectType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            EditorObjectFieldUtility.NoPickerObjectField(new Rect(), null, null, typeof(string)));
    }

    [Test]
    public void ObjectField_WithNullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            EditorObjectFieldUtility.ObjectField(new Rect(), null, null, (Type)null));
    }

    [Test]
    public void ObjectField_WithNonUnityObjectType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            EditorObjectFieldUtility.ObjectField(new Rect(), null, null, typeof(int)));
    }

    #endregion

    #region 类型过滤：传入不匹配的值应被清空

    [Test]
    public void NoPickerObjectField_MismatchedType_ReturnsNull()
    {
        var value = new GameObject(); // 实际类型 GameObject
        var result = EditorObjectFieldUtility.NoPickerObjectField(
            new Rect(0, 0, 200, 20),
            (GUIContent)null,
            value,
            typeof(Texture2D),
            allowSceneObject: false);
        Assert.IsNull(result);
    }

    [Test]
    public void ObjectField_MismatchedType_ReturnsNull()
    {
        var value = ScriptableObject.CreateInstance<TestScriptable>();
        var result = EditorObjectFieldUtility.ObjectField(
            new Rect(0, 0, 200, 20),
            (GUIContent)null,
            value,
            typeof(Texture2D),
            allowSceneObject: false);
        Assert.IsNull(result);
    }

    // 泛型版本也会走入相同的内部逻辑，只需确认非空类型不会崩溃即可
    [Test]
    public void NoPickerObjectField_Generic_WithCorrectType_DoesNotThrow()
    {
        var go = new GameObject();
        var result = EditorObjectFieldUtility.NoPickerObjectField<GameObject>(
            new Rect(0, 0, 200, 20),
            (GUIContent)null,
            go);
        Assert.AreEqual(go, result);
    }

    [Test]
    public void ObjectField_Generic_WithCorrectType_DoesNotThrow()
    {
        var tex = Texture2D.whiteTexture;
        var result = EditorObjectFieldUtility.ObjectField<Texture2D>(
            new Rect(0, 0, 200, 20),
            (GUIContent)null,
            tex);
        Assert.AreEqual(tex, result);
    }

    #endregion

    // 用于测试的简单 ScriptableObject 子类
    private class TestScriptable : ScriptableObject { }
}