using System;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class ConvertTests
    {
        [Test]
        public void ChangeTypeFromDecimalToDouble()
        {
            decimal decimal_value = 1m;
            double  double_value  = 1d;
            Assert.AreEqual(double_value, Convert.ChangeType(decimal_value,
                                                             typeof(double)));
        }
        [Test]
        public void ChangeTypeFromDoubleToDecimal()
        {
            decimal decimal_value = 1m;
            double  double_value  = 1d;
            Assert.AreEqual(decimal_value, Convert.ChangeType(double_value,
                                                             typeof(decimal)));
        }
    }
}
