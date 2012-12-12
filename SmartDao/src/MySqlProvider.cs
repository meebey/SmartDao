using System;
using System.Data;
using System.Text;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class MySqlProvider : AnsiSqlProvider
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
        
        public MySqlProvider()
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
        
        public override string GetTableName(string tableName)
        {
            return String.Format("`{0}`", tableName);
        }
        
        public override string GetColumnName (string columnName)
        {
            return String.Format("`{0}`", columnName);
        }
        
        public override string GetParameterCharacter()
        {
            return "?";
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
        
        public override string CreateCreateTableStatement(string tableName,
                                                          IList<string> columnNames,
                                                          IList<Type> columnTypes,
                                                          IList<int> columnLengths,
                                                          IList<bool> columnIsNullables,
                                                          IList<string> primaryKeys,
                                                          IList<string> sequences)
        {
            string sql = base.CreateCreateTableStatement(tableName,
                                                         columnNames,
                                                         columnTypes,
                                                         columnLengths,
                                                         columnIsNullables,
                                                         primaryKeys,
                                                         sequences);
            //sql = sql.Substring(0, sql.Length - 1);
            sql += "\nENGINE = InnoDB";
            return sql;
        }
    }
}
