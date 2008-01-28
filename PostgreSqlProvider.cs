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