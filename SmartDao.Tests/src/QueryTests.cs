using System;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Meebey.SmartDao.Tests
{
    [TestFixture]
    public class QueryTests
    {
        [Test]
        public void Add()
        {
            log4net.Config.BasicConfigurator.Configure();

            var con = new SqlConnection("Server=mussolini.gsd-software.net;" +
                                    "Database=test;" +
                                    "User ID=test;" +
                                    "Password=test;");
            var provider = new MicrosoftSqlProvider();

            con.Open();
            DatabaseManager dbMan = new DatabaseManager(con, provider);
            if (dbMan.TableExists(typeof(DBTest))) {
                dbMan.DropTable(typeof(DBTest));
            }
            dbMan.InitTable(typeof(DBTest));

            var query = dbMan.CreateQuery<DBTest>();
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
            log4net.Config.BasicConfigurator.Configure();

            var con = new SqlConnection("Server=mussolini.gsd-software.net;" +
                                    "Database=test;" +
                                    "User ID=test;" +
                                    "Password=test;");
            var provider = new MicrosoftSqlProvider();

            con.Open();
            DatabaseManager dbMan = new DatabaseManager(con, provider);
            if (dbMan.TableExists(typeof(DBAutoPKTest))) {
                dbMan.DropTable(typeof(DBAutoPKTest));
            }
            dbMan.InitTable(typeof(DBAutoPKTest));

            var query = dbMan.CreateQuery<DBAutoPKTest>();
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
