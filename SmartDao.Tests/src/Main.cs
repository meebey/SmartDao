using System;
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Npgsql;
using MySql.Data.MySqlClient;

namespace Meebey.SmartDao.Tests
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
#if LOG4NET
            log4net.Config.BasicConfigurator.Configure();
#endif
            IDbConnection con;
            ISqlProvider provider;
            
            /*
            con = new NpgsqlConnection("Server=localhost;" + 
                                       "Database=test;" +
                                       "User ID=test;" +
                                       "Password=test");
            provider = new PostgreSqlProvider();
            */
            
            con = new NpgsqlConnection("Server=localhost;" +
                                       "Port=5433;" +
                                       "Database=test;" +
                                       "User ID=test;" +
                                       "Password=test");
            provider = new PostgreSqlProvider();
            
            /*
            con = new MySqlConnection("Server=localhost;" + 
                                      "Database=test;" +
                                      "User ID=root;");
            provider = new MySqlProvider();
            */
            
            /*
            con = new SqliteConnection("Data Source=test.db;");
            provider = new SqliteProvider();
            */
            
            /*
            con = new NpgsqlConnection("Server=mussolini.gsd-software.net;" + 
                                       "Database=test;" +
                                       "User ID=test;" +
                                       "Password=test;");
            provider = new PostgreSqlProvider();
            */
            
            /*
            con = new SqlConnection("Server=mussolini.gsd-software.net;" + 
            //con = new SqlConnection("Server=62.80.20.132;" + 
                                    "Database=test;" +
                                    "User ID=test;" +
                                    "Password=test;");
            provider = new MicrosoftSqlProvider();
            */
            
            /*
            con = new NpgsqlConnection("Server=lincoln.gsd-software.net;" + 
                                       "Database=test;" +
                                       "User ID=test;" +
                                       "Password=test;");
            provider = new PostgreSqlProvider();
            */
            
            /*
            con = new NpgsqlConnection("Server=merkel.lan.gsd-software.net;" + 
            //con = new NpgsqlConnection("Server=192.168.8.111;" + 
                                       "Database=test;" +
                                       "User ID=test;" +
                                       "Password=test;");
            provider = new PostgreSqlProvider();
            */
            
            Console.WriteLine("--- " + con + " ---");
            Console.WriteLine("--- " + provider + " ---");
            int count = 1000;
            DateTime start, stop;

            // WARMUP
            con.Open();
            con.Close();
            
            Console.WriteLine("--- CONNECT ---");
            start = DateTime.UtcNow;
            con.Open();
            stop = DateTime.UtcNow;
            Console.WriteLine("IDbConnect.Open(): took: " + (stop - start).TotalMilliseconds + " ms");
            
            Console.WriteLine("--- DISCONNECT ---");
            start = DateTime.UtcNow;
            con.Close();
            stop = DateTime.UtcNow;
            Console.WriteLine("IDbConnect.Close(): took: " + (stop - start).TotalMilliseconds + " ms");
            
            con.Open();
            DatabaseManager dbMan = new DatabaseManager(con, provider);
            if (dbMan.TableExists(typeof(DBTest))) {
                dbMan.DropTable(typeof(DBTest));
            }
            dbMan.InitTable(typeof(DBTest));
            
            // WARMUP
            TestLowLevel(con, 1);
            dbMan.EmptyTable(typeof(DBTest));
            TestHighLevel(dbMan, 1);
            dbMan.EmptyTable(typeof(DBTest));

            // RUN
            
            // HARD CLEAN UP
            dbMan.DropTable(typeof(DBTest));
            dbMan.InitTable(typeof(DBTest));

            // SHOWTIME
            start = DateTime.UtcNow;
            TestLowLevel(con, count);
            stop = DateTime.UtcNow;
            double sqlAvg = (stop - start).TotalMilliseconds / count;
            
            // BREAK
            Thread.Sleep(5000);
            
            // HARD CLEAN UP
            dbMan.DropTable(typeof(DBTest));
            dbMan.InitTable(typeof(DBTest));
            
            // BREAK
            Thread.Sleep(5000);
            
            // SHOWTIME
            Console.WriteLine("--- INSERT (" + count + ") ---");
            start = DateTime.UtcNow;
            TestHighLevel(dbMan, count);
            stop = DateTime.UtcNow;
            double queryAvg = (stop - start).TotalMilliseconds / count;
            
            Console.WriteLine("raw SQL INSERTs avg: " + sqlAvg + " ms/query");
            Console.WriteLine("query.Add() avg: " + queryAvg  + " ms/query");
            
            Query<DBTest> query = new Query<DBTest>(dbMan);
            // warm up
            IList<DBTest> tests = query.GetAll(null, "pk_int32");
            
            Console.WriteLine("--- SELECT pk_int32 ---");
            start = DateTime.UtcNow;
            tests = query.GetAll(null, "pk_int32");
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            Console.WriteLine("query.GetAll() avg: " + (stop - start).TotalMilliseconds / tests.Count  + " ms/row");
            
            Console.WriteLine("--- SELECT * ---");
            start = DateTime.UtcNow;
            tests = query.GetAll(null, "*");
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            Console.WriteLine("query.GetAll() avg: " + (stop - start).TotalMilliseconds / tests.Count  + " ms/row");
            
            Console.WriteLine("--- SELECT pk_int32 WHERE pk_int32 = 1 ---");
            start = DateTime.UtcNow;
            DBTest template = new DBTest();
            template.PKInt32 = 1;
            tests = query.GetAll(template, "pk_int32");
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            
            int limit = count / 4;
            int offset = count / 2;
            GetOptions options;
            
            Console.WriteLine("--- SELECT pk_int32 LIMIT " + limit + " ---");
            start = DateTime.UtcNow;
            options = new GetOptions();
            options.SelectFields = new string[] { "pk_int32" };
            options.Limit = limit;
            tests = query.GetAll(null, options);
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            Console.WriteLine("query.GetAll() avg: " + (stop - start).TotalMilliseconds / tests.Count  + " ms/row");
            
            Console.WriteLine("--- SELECT pk_int32 LIMIT " + limit + " OFFSET " + offset + " ---");
            start = DateTime.UtcNow;
            options = new GetOptions();
            options.SelectFields = new string[] { "pk_int32" };
            options.Limit = limit;
            options.Offset = offset;
            tests = query.GetAll(null, options);
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            Console.WriteLine("query.GetAll() avg: " + (stop - start).TotalMilliseconds / tests.Count  + " ms/row");
            
            Console.WriteLine("--- SELECT pk_int32 ORDER BY pk_int32 ---");
            start = DateTime.UtcNow;
            options = new GetOptions();
            options.SelectFields.Add("pk_int32");
            options.OrderBy.Add("pk_int32", OrderByDirection.Ascending);
            tests = query.GetAll(null, options);
            stop = DateTime.UtcNow;
            Console.WriteLine("query.GetAll() rows: " + tests.Count);
            Console.WriteLine("query.GetAll() took: " + (stop - start).TotalMilliseconds + " ms");
            Console.WriteLine("query.GetAll() avg: " + (stop - start).TotalMilliseconds / tests.Count  + " ms/row");
            
            Console.WriteLine("--- UPDATE string_column WHERE pk_int32 = 1 ---");
            start = DateTime.UtcNow;
            template = new DBTest();
            template.PKInt32 = 1;
            template.StringColumn = "foobar";
            int rows = query.SetAll(template);
            stop = DateTime.UtcNow;
            Console.WriteLine("query.SetAll() rows: " + rows);
            Console.WriteLine("query.SetAll() took: " + (stop - start).TotalMilliseconds + " ms");

            con.Close();
            con.Dispose();
        }
        
        private static void TestLowLevel(IDbConnection con, int count)
        {
            for (int i = 0; i < count; i++) {
                string now = DateTime.UtcNow.ToString("s");
                string sql = String.Format("INSERT INTO test_table " +
                                           "(pk_int32, int32_column_fixed, " +
                                           " double_column, string_column, " +
                                           " decimal_column, datetime_column, " +
                                           " datetime_column_notnullable, " +
                                           " int32_column, single_column, " +
                                           " boolean_column) " +
                                           "VALUES ("+
                                           " {0}, 0, {1}, 'abc', 0, '{2}', "+
                                           " '{3}', 0, 0, 'TRUE')",
                                           i, i, now, now);
                IDbCommand com = con.CreateCommand();
                com.CommandText = sql;
                com.ExecuteNonQuery();
            }
        }

        private static void TestHighLevel(DatabaseManager dbMan, int count)
        {
            Query<DBTest> query = new Query<DBTest>(dbMan);
            for (int i = 0; i < count; i++) {
                DateTime now = DateTime.UtcNow;
                DBTest test = new DBTest();
                test.PKInt32 = i;
                test.StringColumn = "abc";
                test.DateTimeColumn = now;
                test.DateTimeColumnNotNullable = now;
                test.BooleanColumn = true;
                test.DoubleColumn = i;
                query.Add(test);
            }
        }
    }
}