using System;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class DBNullableTests
    {
        [Test]
        public void IsNullInt32()
        {
            DBNullable<Int32> foo = new DBNullable<Int32>();
            Assert.IsNotNull(foo, "#1");
            Assert.IsTrue(foo.IsNull, "#2");
            
            foo = null;
            Assert.IsNull(foo, "#3");
            
            foo = new DBNullable<Int32>(2);
            Assert.IsFalse(foo.IsNull, "#4");
        }
        
        [Test]
        public void IsNullString()
        {
            DBNullable<String> value = new DBNullable<String>();
            Assert.IsNotNull(value, "#1");
            Assert.IsTrue(value.IsNull, "#2");
            
            value = null;
            Assert.IsNull(value, "#3");
            
            value = new DBNullable<String>("test");
            Assert.IsFalse(value.IsNull, "#4");
        }
        
        [Test]
        public void EqualsString()
        {
            DBNullable<String> value1;
            DBNullable<String> value2;
            
            value1 = new DBNullable<String>();
            value2 = new DBNullable<String>();
            Assert.AreEqual(value1, value2, "#1");
            Assert.IsTrue(value1 == value2, "#2");
            Assert.IsTrue(value2 == value1, "#3");
            Assert.IsTrue(value1.Equals(value2), "#4");
            Assert.IsTrue(value2.Equals(value1), "#5");
            
            value1 = new DBNullable<String>("test");
            value2 = new DBNullable<String>("test");
            Assert.AreEqual(value1, value2, "#6");
            Assert.IsTrue(value1 == value2, "#7");
            Assert.IsTrue(value2 == value1, "#8");
            Assert.IsTrue(value1.Equals(value2), "#9");
            Assert.IsTrue(value2.Equals(value1), "#10");
            
            value1 = "test";
            value2 = "test";
            Assert.AreEqual(value1, value2, "#11");
            Assert.IsTrue(value1 == value2, "#12");
            Assert.IsTrue(value2 == value1, "#13");
            Assert.IsTrue(value1.Equals(value2), "#14");
            Assert.IsTrue(value2.Equals(value1), "#15");
            
            value1 = "test";
            Assert.AreEqual("test", value1.Value, "#16");
            Assert.IsTrue((string) value1 == "test", "#17");
            Assert.IsTrue(value1 == "test", "#18");
            Assert.IsTrue("test" == (string) value1, "#19");
            Assert.IsTrue("test" == value1, "#20");
            Assert.IsTrue(value1.Equals("test"), "#21");
            // TODO: FAIL
            //Assert.IsTrue("test".Equals(value1), "#22");
        }
        
        [Test]
        public void Int32Equals()
        {
            DBNullable<Int32> value1;
            DBNullable<Int32> value2;
            
            value1 = new DBNullable<Int32>();
            value2 = new DBNullable<Int32>();
            Assert.AreEqual(value1, value2, "1");
            Assert.IsTrue(value1 == value2, "2");
            Assert.IsTrue(value2 == value1, "3");
            Assert.IsTrue(value1.Equals(value2), "4");
            Assert.IsTrue(value2.Equals(value1), "5");
            
            value1 = new DBNullable<Int32>(123);
            value2 = new DBNullable<Int32>(123);
            Assert.AreEqual(value1, value2, "6");
            Assert.IsTrue(value1 == value2, "7");
            Assert.IsTrue(value2 == value1, "8");
            Assert.IsTrue(value1.Equals(value2), "9");
            Assert.IsTrue(value2.Equals(value1), "10");
            
            value1 = 123;
            value2 = 123;
            Assert.AreEqual(value1, value2, "11");
            Assert.IsTrue(value1 == value2, "12");
            Assert.IsTrue(value2 == value1, "13");
            Assert.IsTrue(value1.Equals(value2), "14");
            Assert.IsTrue(value2.Equals(value1), "15");
            
            value1 = 123;
            Assert.AreEqual(123, (int) value1, "16");
            Assert.IsTrue((int) value1 == 123, "17");
            Assert.IsTrue(value1 == 123, "18");
            Assert.IsTrue(123 == (int) value1, "19");
            Assert.IsTrue(123 == value1, "20");
            Assert.IsTrue(value1.Equals(123), "21");
            // TODO: FAIL
            //Assert.IsTrue(123.Equals(value1), "22");
        }
    }
}
