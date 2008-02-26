using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class PostgreSqlProvider : AnsiSqlProvider
    {
        public override bool HasLimitSupport {
            get {
                return true;
            }
        }
        
        public override bool HasOffsetSupport {
            get {
                return true;
            }
        }
        
        public PostgreSqlProvider()
        {
        }
        
        /*
        // PostgreSQL handles quoted names case-sensitive, this is bad
        public override string GetColumnName(string columnName)
        {
            return columnName;
        }
        
        // PostgreSQL handles quoted names case-sensitive, this is bad
        public override string GetTableName(string tableName)
        {
            return tableName;
        }
        
        public override string CreateCreateTableStatement(string tableName,
                                                          IList<string> columnNames,
                                                          IList<Type> columnTypes,
                                                          IList<int> columnLengths,
                                                          IList<bool?> columnIsNullables,
                                                          IList<string> primaryKeys)
        {
            // HACK: we need to quote the table and column names, else
            // postgresql will ignore our casing and make everything lower-case
            tableName = String.Format("\"{0}\"", tableName);
            for (int i = 0; i < columnNames.Count; i++) {
                columnNames[i] = String.Format("\"{0}\"", columnNames[i]);
            }
            for (int i = 0; i < primaryKeys.Count; i++) {
                primaryKeys[i] = String.Format("\"{0}\"", primaryKeys[i]);
            }
            
            return base.CreateCreateTableStatement(tableName,
                                                   columnNames,
                                                   columnTypes,
                                                   columnLengths,
                                                   columnIsNullables,
                                                   primaryKeys);
        }
        */

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
            
            StringBuilder limitClause;
            limitClause = new StringBuilder(25); // LIMIT + OFFSET + 2 * 3 digits
            limitClause.AppendFormat(" LIMIT {0}", limit);
            if (offset != null) {
                limitClause.AppendFormat(" OFFSET {0}", offset);
            }
            
            return sql + limitClause.ToString();
        }
    }
}
