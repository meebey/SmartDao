using System;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class SqliteProvider : AnsiSqlProvider
    {
        public SqliteProvider()
        {
        }
        
        public override string GetDataTypeName(DbType dbType)
        {
            switch (dbType) {
                case DbType.Int32:
                    return "INT4";
            }
            
            return base.GetDataTypeName(dbType);
        }
        
        public override string CreateTableExistsStatement(string tableName)
        {
            return String.Format("SELECT COUNT(*) " +
                                 "FROM sqlite_master " +
                                 "WHERE type = 'table' AND name = '{0}'", tableName);
        }
    }
}
