using System;
using BLToolkit.Aspects;
using BLToolkit.DataAccess;
using FluentAssertions;
using NUnit.Framework;

namespace MLC.Wms.Tests.Unit
{
    [TestFixture]
    public class CacheTest
    {
        public class TestClass : DataAccessor
        {
            public int Add { get; set; }

            [Cache]
            public virtual int Get(int i, int j)
            {
                return Add + i + j;
            }
        }

        [Test]
        public void TestWorkFromDifInstances()
        {
            var instance1 = (TestClass)DataAccessor.CreateInstance(typeof (TestClass));
            instance1.Add = 1;
            var instance2 = (TestClass)DataAccessor.CreateInstance(typeof (TestClass));
            instance2.Add = 2;

            instance1.Get(1, 1).Should().Be(3);
            instance2.Get(1, 1).Should().Be(3);
        }
    }
}