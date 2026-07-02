// 文件: Tests/TestStaticCollectionPools.cs
using NUnit.Framework;
using PoolKit.Tests;
using System;
using System.Collections.Generic;

namespace PoolKit.Collections.Tests
{
    /// <summary>
    /// 静态集合池单元测试 - 运行于 EditMode
    /// </summary>
    public class TestStaticCollectionPools : TestBase
    {
        #region ListPool

        [Test]
        public void ListPool_Rent_ShouldReturnList()
        {
            var list = ListPool.Rent<int>();
            Assert.NotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void ListPool_RentWithScope_ShouldAutoReturn()
        {
            List<int> capturedList = null;

            using (var scope = ListPool.RentWithScope<int>())
            {
                capturedList = scope.List;
                capturedList.Add(42);
                Assert.AreEqual(1, capturedList.Count);
            }

            // 归还后列表被清空
            Assert.AreEqual(0, capturedList.Count);
        }

        [Test]
        public void ListPool_Return_ShouldClearList()
        {
            var list = ListPool.Rent<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            ListPool.Return(list);

            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void ListPool_Return_Null_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() => ListPool.Return<int>(null));
        }

        [Test]
        public void ListPool_Return_ShouldReuseInstance()
        {
            var list1 = ListPool.Rent<int>();
            var id1 = list1.GetHashCode();
            ListPool.Return(list1);

            var list2 = ListPool.Rent<int>();
            var id2 = list2.GetHashCode();

            Assert.AreEqual(id1, id2);
        }

        #endregion

        #region DictionaryPool

        [Test]
        public void DictionaryPool_Rent_ShouldReturnDictionary()
        {
            var dict = DictionaryPool.Rent<string, int>();
            Assert.NotNull(dict);
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void DictionaryPool_RentWithScope_ShouldAutoReturn()
        {
            Dictionary<string, int> capturedDict = null;

            using (var scope = DictionaryPool.RentWithScope<string, int>())
            {
                capturedDict = scope.Dictionary;
                capturedDict["test"] = 42;
                Assert.AreEqual(1, capturedDict.Count);
            }

            Assert.AreEqual(0, capturedDict.Count);
        }

        #endregion

        #region QueuePool

        [Test]
        public void QueuePool_Rent_ShouldReturnQueue()
        {
            var queue = QueuePool.Rent<int>();
            Assert.NotNull(queue);
            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void QueuePool_RentWithScope_ShouldAutoReturn()
        {
            Queue<int> capturedQueue = null;

            using (var scope = QueuePool.RentWithScope<int>())
            {
                capturedQueue = scope.Queue;
                capturedQueue.Enqueue(42);
                Assert.AreEqual(1, capturedQueue.Count);
            }

            Assert.AreEqual(0, capturedQueue.Count);
        }

        #endregion

        #region StackPool

        [Test]
        public void StackPool_Rent_ShouldReturnStack()
        {
            var stack = StackPool.Rent<int>();
            Assert.NotNull(stack);
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void StackPool_RentWithScope_ShouldAutoReturn()
        {
            Stack<int> capturedStack = null;

            using (var scope = StackPool.RentWithScope<int>())
            {
                capturedStack = scope.Stack;
                capturedStack.Push(42);
                Assert.AreEqual(1, capturedStack.Count);
            }

            Assert.AreEqual(0, capturedStack.Count);
        }

        #endregion

        #region HashSetPool

        [Test]
        public void HashSetPool_Rent_ShouldReturnHashSet()
        {
            var set = HashSetPool.Rent<int>();
            Assert.NotNull(set);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void HashSetPool_RentWithScope_ShouldAutoReturn()
        {
            HashSet<int> capturedSet = null;

            using (var scope = HashSetPool.RentWithScope<int>())
            {
                capturedSet = scope.HashSet;
                capturedSet.Add(42);
                Assert.AreEqual(1, capturedSet.Count);
            }

            Assert.AreEqual(0, capturedSet.Count);
        }

        #endregion

        #region ArrayPool

        [Test]
        public void ArrayPool_Rent_ShouldReturnArray()
        {
            var array = ArrayPool.Rent<int>(10);
            Assert.NotNull(array);
            Assert.GreaterOrEqual(array.Length, 10);
        }

        [Test]
        public void ArrayPool_Rent_ZeroLength_ShouldReturnEmpty()
        {
            var array = ArrayPool.Rent<int>(0);
            Assert.AreEqual(0, array.Length);
        }

        [Test]
        public void ArrayPool_Rent_NegativeLength_ShouldThrow()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool.Rent<int>(-1));
        }

        [Test]
        public void ArrayPool_Return_ShouldReuseArray()
        {
            var array1 = ArrayPool.Rent<int>(64);
            ArrayPool.Return(array1);
            var array2 = ArrayPool.Rent<int>(64);
            Assert.True(ReferenceEquals(array1, array2), "Array should be reused");
        }

        [Test]
        public void ArrayPool_Return_ClearArray_ShouldClearElements()
        {
            var array = ArrayPool.Rent<int>(10);
            for (int i = 0; i < array.Length; i++)
                array[i] = i;

            ArrayPool.Return(array, clearArray: true);

            for (int i = 0; i < array.Length; i++)
                Assert.AreEqual(0, array[i]);
        }

        #endregion
    }
}