public class TestObject
{
    public int Value { get; set; }
    public bool IsReset { get; set; }

    public TestObject() { Value = 0; IsReset = false; }

    public void Reset()
    {
        Value = 0;
        IsReset = true;
    }
}