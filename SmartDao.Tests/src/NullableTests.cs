using System;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class NullableTests
    {
        [Test]
        public void TestCase()
        {
            Nullable<Int32> foo = new Nullable<Int32>();
            Assert.IsNull(foo, "#1");
            Assert.IsFalse(foo.HasValue, "#2");
            
            foo = 3;
            Assert.IsTrue(foo.HasValue, "#3");
            Assert.AreEqual(3, foo, "#4");
            Assert.IsTrue(foo.Equals(3), "#5");
            Assert.IsTrue(3.Equals(foo), "#6");

            foo = null;
            Assert.IsNull(foo, "#7");
            Assert.IsFalse(foo.HasValue, "#8");
        }
    }
}
