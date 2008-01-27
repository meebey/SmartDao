using System;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class MySqlProvider : AnsiSqlProvider
    {
        public MySqlProvider()
        {
        }
        
        public override string CreateCreateTableStatement(string tableName, IList<string> columnNames, IList<Type> columnTypes, IList<int> columnLengths, IList<bool?> columnIsNullables, IList<string> primaryKeys)
        {
            string sql = base.CreateCreateTableStatement(tableName, columnNames, columnTypes, columnLengths, columnIsNullables, primaryKeys);
            sql = sql.Substring(sql.Length - 1);
            sql += ",\nENGINE = InnoDB)";
            return sql;
        }
        
        public override string GetDataTypeName(DbType dbType)
        {
            switch (dbType) {
                case DbType.DateTime:
                    return "DATETIME";
            }
            
            return base.GetDataTypeName(dbType);
        }
        
        public override string GetTableName(string tableName)
        {
            return String.Format("`{0}`", tableName);
        }
        
        public override string GetParameterCharacter()
        {
            return "?";
        }
    }
}
