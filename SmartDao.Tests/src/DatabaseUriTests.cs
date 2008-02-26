/*
 * $Id$
 * $URL$
 * $Revision$
 * $Author$
 * $Date$
 */


using System;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class DatabaseUriTests
    {
        [Test]
        public void Constructor()
        {
            DatabaseUri dbUri = new DatabaseUri("mysql://root:secret@localhost:3306/mydb");
            Assert.AreEqual("mysql",     dbUri.DatabaseType);
            Assert.AreEqual("root",      dbUri.Username);
            Assert.AreEqual("secret",    dbUri.Password);
            Assert.AreEqual("localhost", dbUri.Host);
            Assert.AreEqual(3306,        dbUri.Port);
            Assert.AreEqual("mydb",      dbUri.DatabaseName);
        }
    }
}
