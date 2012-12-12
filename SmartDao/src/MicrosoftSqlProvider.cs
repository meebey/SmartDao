using System;
using System.Data;
using System.Text;
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
                case DbType.Boolean:
                    return "BIT";
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
            sql = String.Format("SELECT TOP {0} {1} ", limit, sql);

            return sql;
        }

        public override string CreateSelectVersionStatement()
        {
            return "SELECT @@VERSION";
        }

        public override string CreateInsertStatement(string tableName,
                                                     IList<string> columnNames,
                                                     IList<string> columnValues)
        {
            string sql = base.CreateInsertStatement(tableName,
                                                    columnNames,
                                                    columnValues);

            sql = String.Format(
                "{0}{1} SELECT IDENT_CURRENT('{2}')",
                sql,
                GetStatementSeparator(),
                tableName
            );

            return sql;
        }

        public override string CreateTableColumnExpression(string columnName,
                                                           string columnType,
                                                           int? columnLength,
                                                           bool isPrimaryKey,
                                                           bool isSequence,
                                                           bool isNullable)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetColumnName(columnName), columnType);
            if (columnLength != null) {
                sql.AppendFormat("({0})", columnLength);
            }
            if (isPrimaryKey) {
                sql.Append(" PRIMARY KEY");
                if (isSequence) {
                    sql.Append(" IDENTITY");
                }
            } else if (!isNullable) {
                sql.Append(" NOT NULL");
            }
            return sql.ToString();
        }

        /*
        public override string CreateSequenceStatement(string tableName,
                                                       string columnName,
                                                       int? seed,
                                                       int? increment)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (columnName == null) {
                throw new ArgumentNullException("columnName");
            }

            var sql = String.Format(
                "ALTER TABLE {0} ALTER COLUMN {1} IDENTITY",
                GetTableName(tableName),
                GetColumnName(columnName)
            );
            if (seed != null || increment != null) {
                sql += String.Format(
                    "({0}, {1})",
                    seed == null ? 0 : seed,
                    increment == null ? 1 : increment
                );
            }
            return sql;
        }
        */

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
