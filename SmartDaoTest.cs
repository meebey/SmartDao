using System;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using Npgsql;

namespace Meebey.SmartDao
{
    public class SmartDaoTest
    {
        public static void Main(string[] args)
        {
#if LOG4NET
            log4net.Config.BasicConfigurator.Configure();
#endif
            string connectionString =
                "Server=localhost;" + 
                "Database=test;" +
                "User ID=postgres;";
            
            IDbConnection con = new NpgsqlConnection(connectionString);
            con.Open();
            
            DatabaseManager dbMan = new DatabaseManager(con);
            dbMan.InitTable(typeof(DBTest));
            
            DateTime start, stop;
            double total;
            int count = 10;
            
            // WARMUP
            TestHighLevel(dbMan, 1);
            TestLowLevel(con, 1);

            // SHOWTIME
            TestHighLevel(dbMan, count);
            Thread.Sleep(1000);
            TestLowLevel(con, count);
            
            con.Close();
            con.Dispose();
            
            /*
            for (int i = 0; i < 3; i++) {
                SortedDictionary<Attribute, string> sdict = new SortedDictionary<Attribute,string>();
                sdict.Add(new SerializableAttribute(), "foo");
                sdict.Add(new SerializableAttribute(), "bar");
            }
            */
        }
        
        private static void TestLowLevel(IDbConnection con, int count)
        {
            DateTime start, stop;
            double total = 0d;
            for (int i = 0; i < count; i++) {
                start = DateTime.UtcNow;
                string sql = "INSERT INTO test_table (pk_int32, int32_column_fixed, double_column, string_column, decimal_column, datetime_column, int32_column, single_column) VALUES (0, 0, 0, 'abc', 0, '0001-01-01 00:00:00', 0, 0)";
                IDbCommand com = con.CreateCommand();
                com.CommandText = sql;
                com.ExecuteNonQuery();
                stop = DateTime.UtcNow;
                total += (stop - start).TotalMilliseconds;
            }
            Console.WriteLine("raw SQL avg: " + total / count  + " ms/call");
        }

        private static void TestHighLevel(DatabaseManager dbMan, int count)
        {
            DateTime start, stop;
            double total = 0d;
            Query<DBTest> query = new Query<DBTest>(dbMan);
            for (int i = 0; i < count; i++) {
                start = DateTime.UtcNow;
                DBTest test = new DBTest();
                test.StringColumn = "abc";
                query.Add(test);
                stop = DateTime.UtcNow;
                total += (stop - start).TotalMilliseconds;
            }
            Console.WriteLine("query.Add() avg: " + total / count  + " ms/call");
        }
    }
}
