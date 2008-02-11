using System;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class MicrosoftSqlProvider : AnsiSqlProvider
    {
        public override bool HasLimitSupport {
            get {
                return true;
            }
        }
        
        public override bool HasOffsetSupport {
            get {
                return false;
            }
        }
        
        public MicrosoftSqlProvider()
        {
        }
        
        public override string GetDataTypeName(DbType dbType)
        {
            switch (dbType) {
                case DbType.DateTime:
                    return "DATETIME";
            }
            
            return base.GetDataTypeName(dbType);
        }
        
        public override string CreateSelectStatement(string schemaName,
                                                     string tableName,
                                                     IList<string> selectColumnNames, 
                                                     string whereClause,
                                                     IList<string> orderByColumnNames,
                                                     IList<string> orderByColumnDirections,
                                                     int? limit,
                                                     int? offset)
        {
            // LIMIT is RDBMS-specific, thus we implement it here
            string sql = base.CreateSelectStatement(schemaName, tableName,
                                                    selectColumnNames,
                                                    whereClause,
                                                    orderByColumnNames,
                                                    orderByColumnDirections,
                                                    null, null);
            if (limit == null) {
                return sql;
            }
            // TODO: check MSSQL version
            if (offset != null) {
                throw new NotSupportedException("Microsoft SQL Server doesn't support offset in SELECT statements.");
            }
            
            // remove SELECT part
            sql = sql.Substring(7);
            sql += String.Format("SELECT TOP {0} ", limit);
            return sql;
        }
        
        // Microsoft SQL supports ANSI quotes correctly, didn't expect that one
        /*
        public override string GetTableName(string tableName)
        {
            return String.Format("[{0}]", tableName);
        }

        public override string GetColumName(string columName)
        {
            return String.Format("[{0}]", columName);
        }
        */
    }
}
