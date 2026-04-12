// 辅助 MonoBehaviour
using EventHub.Unity;
using UnityEngine;

public class TestMonoBehaviour : MonoBehaviour
{
    public int CallCount = 0;
    public void TestSubscribe()
    {
        this.Subscribe<TestEvent>(__ => CallCount++);
    }
}