using System;
using System.Data;

namespace Meebey.SmartDao
{
    public class SqlProvider
    {
        public SqlProvider()
        {
        }
        
        public virtual string GetDataTypeName(DbType dbType)
        {
            switch (dbType) {
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return "CHARACTER";
                case DbType.AnsiString:
                case DbType.String:
                    return "CHARACTER VARYING";
                case DbType.Int16:
                    return "SMALLINT";
                case DbType.Int32:
                    return "INTEGER";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.Decimal:
                    return "NUMERIC";
                case DbType.Boolean:
                    return "BOOLEAN";
                case DbType.Single:
                    return "REAL";
                case DbType.Double:
                    return "DOUBLE PRECISION";
                case DbType.DateTime:
                    return "TIMESTAMP";
                default:
                    throw new NotSupportedException("DbType is not supported: " + dbType);
            }
        }
                                              
        public virtual string GetColumnName(string columnName)
        {
            return columnName;
        }
        
        public virtual string GetTableName(string tableName)
        {
            return tableName;
        }
    }
}
