using System;
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class QueryTests
    {
        DatabaseManager Manager { get; set; }
        IDbConnection Connection { get; set; }

        [SetUp]
        protected void SetUp()
        {
            log4net.Config.BasicConfigurator.Configure();

            Connection = MainClass.Connection;
            var provider = MainClass.Provider;

            Manager = new DatabaseManager(Connection, provider);
            Connection.Open();
        }

        [TearDown]
        protected void TearDown()
        {
            Connection.Close();
        }

        [Test]
        public void Add()
        {
            if (Manager.TableExists(typeof(DBTest))) {
                Manager.DropTable(typeof(DBTest));
            }
            Manager.InitTable(typeof(DBTest));

            var query = Manager.CreateQuery<DBTest>();
            var entry = new DBTest();
            entry.StringColumn = "foobar123";
            entry.DateTimeColumnNotNullable = DateTime.UtcNow;

            // create the 1nd row
            entry.PKInt32 = 0;
            var pkEntry = query.Add(entry);
            Assert.IsNotNull(pkEntry.PKInt32);
            Assert.AreEqual(0, pkEntry.PKInt32);

            // create the 2nd row
            entry.PKInt32 = 1;
            pkEntry = query.Add(entry);
            Assert.IsNotNull(pkEntry.PKInt32);
            Assert.AreEqual(1, pkEntry.PKInt32);
        }

        [Test]
        public void AddWithPrimaryKeySequence()
        {
            if (Manager.TableExists(typeof(DBAutoPKTest))) {
                Manager.DropTable(typeof(DBAutoPKTest));
            }
            Manager.InitTable(typeof(DBAutoPKTest));

            var query = Manager.CreateQuery<DBAutoPKTest>();
            var entry = new DBAutoPKTest();
            entry.StringColumn = "foobar123";

            // create the 1nd row
            var pkEntry = query.Add(entry);
            var firstPkValue = pkEntry.PKInt32;
            Assert.IsNotNull(firstPkValue);

            // create the 2nd row
            pkEntry = query.Add(entry);
            Assert.IsNotNull(pkEntry.PKInt32);
            var secondPkValue = pkEntry.PKInt32;

            // check if the sequence worked
            Assert.AreEqual(firstPkValue + 1, secondPkValue);
        }
    }
}
