using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class AnsiSqlProvider : ISqlProvider
    {
        private static IDictionary<Type, DbType> f_SupportedCliTypes;
        private string  f_VersionString;
        private Version f_Version;

        public virtual bool HasLimitSupport {
            get {
                return false;
            }
        }

        public virtual bool HasOffsetSupport {
            get {
                return false;
            }
        }

        public virtual string VersionString {
            get {
                return f_VersionString;
            }
            set {
                f_VersionString = value;
            }
        }

        public virtual Version Version {
            get {
                return f_Version;
            }
            set {
                f_Version = value;
            }
        }

        static AnsiSqlProvider() {
            f_SupportedCliTypes = new Dictionary<Type, DbType>(9);
            f_SupportedCliTypes.Add(typeof(String),    DbType.String);
            f_SupportedCliTypes.Add(typeof(Boolean),   DbType.Boolean);
            f_SupportedCliTypes.Add(typeof(Boolean?),  DbType.Boolean);
            f_SupportedCliTypes.Add(typeof(Int16),     DbType.Int16);
            f_SupportedCliTypes.Add(typeof(Int16?),    DbType.Int16);
            f_SupportedCliTypes.Add(typeof(Int32),     DbType.Int32);
            f_SupportedCliTypes.Add(typeof(Int32?),    DbType.Int32);
            f_SupportedCliTypes.Add(typeof(Int64),     DbType.Int64);
            f_SupportedCliTypes.Add(typeof(Int64?),    DbType.Int64);
            f_SupportedCliTypes.Add(typeof(Single),    DbType.Single);
            f_SupportedCliTypes.Add(typeof(Single?),   DbType.Single);
            f_SupportedCliTypes.Add(typeof(Double),    DbType.Double);
            f_SupportedCliTypes.Add(typeof(Double?),   DbType.Double);
            f_SupportedCliTypes.Add(typeof(Decimal),   DbType.Decimal);
            f_SupportedCliTypes.Add(typeof(Decimal?),  DbType.Decimal);
            f_SupportedCliTypes.Add(typeof(DateTime),  DbType.DateTime);
            f_SupportedCliTypes.Add(typeof(DateTime?), DbType.DateTime);
        }
        
        public AnsiSqlProvider()
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
                                              
        public virtual DbType GetDBType(Type type)
        {
            DbType res;
            if (!f_SupportedCliTypes.TryGetValue(type, out res)) { 
                throw new NotSupportedException("Type is not supported: " + type);
            }
            return res;
        }
        
        public virtual string GetColumnName(string columnName)
        {
            return String.Format("\"{0}\"", columnName);
        }
        
        public virtual string GetTableName(string tableName)
        {
            return String.Format("\"{0}\"", tableName);
        }
        
        public virtual string GetTableName(string schemaName, string tableName)
        {
            if (schemaName == null) {
                return GetTableName(tableName);
            }
            return String.Format("{0}.{1}", GetTableName(schemaName), GetTableName(tableName));
        }
        
        public virtual string GetStatementSeparator()
        {
            return ";";
        }        
        
        public virtual string GetParameterCharacter()
        {
            return "@";
        }

        public virtual string CreateSelectVersionStatement()
        {
            return null;
        }

        public virtual string CreateCreateTableStatement(string tableName,
                                                         IList<string> columnNames,
                                                         IList<Type> columnTypes,
                                                         IList<int> columnLengths,
                                                         IList<bool> columnIsNullables,
                                                         IList<string> primaryKeys,
                                                         IList<string> sequences)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (columnNames == null) {
                throw new ArgumentNullException("columnNames");
            }
            if (columnTypes == null) {
                throw new ArgumentNullException("columnTypes");
            }
            if (columnLengths == null) {
                throw new ArgumentNullException("columnSizes");
            }
            if (primaryKeys == null) {
                throw new ArgumentNullException("primaryKeys");
            }
            if (sequences == null) {
                throw new ArgumentNullException("sequences");
            }

            if (!(columnNames.Count == columnTypes.Count &&
                  columnNames.Count == columnLengths.Count)) {
                throw new ArgumentException("columnNames, columnTypes and columnLengths must have the same size.");
            }

            if (columnNames.Count == 0) {
                throw new ArgumentException("columnNames, columnTypes and columnLengths must not be empty.");
            }

            StringBuilder sql = new StringBuilder("CREATE TABLE ");
            sql.AppendFormat("{0} (\n", GetTableName(tableName));
            for (int idx = 0; idx < columnNames.Count; idx++) {
                string name = columnNames[idx];
                Type type = columnTypes[idx];
                int length = columnLengths[idx];
                bool isNullable = columnIsNullables[idx];
                bool isPrimaryKey = primaryKeys.Contains(name);
                bool isSequence = sequences.Contains(name);

                if (type.IsValueType && !IsNullableType(type)) {
                    throw new NotSupportedException("Value type: " + type + " must be nullable for column: " + name);
                }

                if (!f_SupportedCliTypes.ContainsKey(type)) {
                    throw new NotSupportedException("Unsupported column data type: " + type);
                }

                string columnType = GetDataTypeName(GetDBType(type));
                int? columnLength = null;
                if (type == typeof(string)) {
                    // TODO: CHAR support
                    if (length == -1) {
                        columnType = "TEXT";
                    } else if (length != 0) {
                        columnLength = length;
                    }
                }

                sql.Append(
                    CreateTableColumnExpression(name, columnType, columnLength,
                                                isPrimaryKey,
                                                isSequence,
                                                isNullable)
                );

                sql.Append(", \n");
            }
            sql.Remove(sql.Length - 3, 3);

            /*
            if (primaryKeys.Count > 0) {
                sql.Append(", \nPRIMARY KEY (");
                foreach (string pk in primaryKeys) {
                    sql.AppendFormat("{0}, ", GetColumnName(pk));
                }
                sql.Remove(sql.Length - 2, 2);
                sql.Append(")");
            }
            */

            // CREATE UNIQUE INDEX title_idx ON films (title);
            // CONSTRAINT valid_discount CHECK (price > discounted_price)
            // UNIQUE (product_no)
            // FOREIGN KEY (b, c) REFERENCES other_table (c1, c2)
            sql.Append(")");
            return sql.ToString();
        }

        public virtual string CreateTableColumnExpression(string columnName,
                                                          string columnType,
                                                          int? columnLength,
                                                          bool isPrimaryKey,
                                                          bool isSequence,
                                                          bool isNullable)
        {
            if (isSequence) {
                throw new NotSupportedException();
            }

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetColumnName(columnName), columnType);
            if (columnLength != null) {
                sql.AppendFormat("({0})", columnLength);
            }
            if (isPrimaryKey) {
                sql.Append(" PRIMARY KEY");
            } else if (!isNullable) {
                sql.Append(" NOT NULL");
            }
            return sql.ToString();
        }

        public virtual string CreateInsertStatement(string tableName,
                                                    IList<string> columnNames,
                                                    IList<string> columnValues)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (columnNames == null) {
                throw new ArgumentNullException("columnNames");
            }
            if (columnValues == null) {
                throw new ArgumentNullException("columnValues");
            }

            if (columnNames.Count != columnValues.Count) {
                throw new ArgumentException("columnNames and columnValues must have the same size.");
            }
            
            if (columnNames.Count == 0) {
                throw new ArgumentException("columnNames, columnOperators and columnValues must not be empty.");
            }
            
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(GetTableName(tableName));
            sql.Append(" (");

            StringBuilder values = new StringBuilder();
            for (int idx = 0; idx < columnNames.Count; idx++) {
                string name = columnNames[idx];
                string value = columnValues[idx];
                
                sql.AppendFormat("{0}, ", GetColumnName(name));
                values.AppendFormat("{0}, ", value);
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(") VALUES (");
            sql.Append(values);
            sql.Remove(sql.Length - 2, 2);
            sql.Append(")");
            
            return sql.ToString();
        }
        
        /*
         * ALTER TABLE [ ONLY ] name [ * ]
         * RENAME [ COLUMN ] column TO new_column
         * 
         * ALTER TABLE name
         * RENAME TO new_name
         */
        public virtual string CreateDropTableStatement(string tableName)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }

            StringBuilder sql = new StringBuilder("DROP TABLE ");
            sql.Append(GetTableName(tableName));
            
            return sql.ToString();
        }
        
        public virtual string CreateTableExistsStatement(string tableName)
        {
            return CreateSelectStatement("information_schema", "tables",
                                         new string[] { "COUNT(*)" },
                                         new string[] { "table_name" },
                                         new string[] { "=" },
                                         new string[] { String.Format("'{0}'", tableName) },
                                         null, null, null, null);
        }
        
        /*
        public virtual string GetLastInsertedPrimaryKey()
        {
           case MSSQL:
               // the insert worked and all primary keys are null means IDENTITY was used
               // Now we can fetch the used IDENTITY value
               sql.append("SELECT IDENT_CURRENT('" + tablename + "') AS id;");
               break;

           case MySQL:
               sql.append("SELECT LAST_INSERT_ID() AS id;");
               break;

           case PostgreSQL:
               sql.append("SELECT CURRVAL('" + tablename.getTableName() + "_" + pkFieldName + "_seq') AS id;"); 
               break;
        }
        */
        
        public virtual string CreateSelectStatement(string schemaName,
                                                    string tableName,
                                                    IList<string> selectColumnNames,
                                                    IList<string> whereColumnNames,
                                                    IList<string> whereColumnOperators,
                                                    IList<string> whereColumnValues,
                                                    IList<string> orderByColumnNames,
                                                    IList<string> orderByColumnDirection,
                                                    int? limit,
                                                    int? offset)
        {
            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("columnNames, columnOperators and columnValues must have the same size.");
                }
            }
            
            StringBuilder whereClause = null;
            if (whereColumnNames != null && whereColumnNames.Count > 0) {
                whereClause = new StringBuilder();
                for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                    whereClause.AppendFormat("{0} {1} {2} AND ",
                                             GetColumnName(whereColumnNames[idx]),
                                             whereColumnOperators[idx],
                                             whereColumnValues[idx]);
                }
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            return CreateSelectStatement(schemaName, tableName,
                                         selectColumnNames,
                                         whereClause != null ? whereClause.ToString() : null,
                                         orderByColumnNames,
                                         orderByColumnDirection,
                                         limit,
                                         offset);
        }
        
        public virtual string CreateSelectStatement(string schemaName,
                                                    string tableName,
                                                    IList<string> selectColumnNames, 
                                                    string whereClause,
                                                    IList<string> orderByColumnNames,
                                                    IList<string> orderByColumnDirections,
                                                    int? limit,
                                                    int? offset)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            
            StringBuilder sql = new StringBuilder("SELECT ");
            if (selectColumnNames != null && selectColumnNames.Count > 0) {
                for (int idx = 0; idx < selectColumnNames.Count; idx++) {
                    string name = selectColumnNames[idx];
                    if (name == "*" || (name.Contains("(") && name.EndsWith(")"))) {
                        // don't quote "*" and function calls
                        sql.AppendFormat("{0}, ", name);
                    } else {
                        sql.AppendFormat("{0}, ", GetColumnName(name));
                    }
                }
                sql.Remove(sql.Length - 2, 2);
            } else {
                sql.Append("*");
            }
            
            sql.Append(" FROM ");
            sql.Append(GetTableName(schemaName, tableName));
            
            if (whereClause != null && whereClause.Length > 0) {
                sql.AppendFormat(" WHERE {0}", whereClause);
            }
            
            if (orderByColumnNames != null && orderByColumnNames.Count > 0) {
                if (orderByColumnDirections == null) {
                    throw new ArgumentException("orderByColumnDirection must not be null if orderByColumnNames is not null.");
                }
                if (orderByColumnNames.Count != orderByColumnDirections.Count) {
                    throw new ArgumentException("orderByColumnNames and orderByColumnDirection must have the same size.");
                }
                
                sql.Append(" ORDER BY ");
                for (int idx = 0; idx < orderByColumnNames.Count; idx++) {
                    string direction = orderByColumnDirections[idx];
                    sql.Append(GetColumnName(orderByColumnNames[idx]));
                    if (direction != null) {
                        sql.AppendFormat(" {0}", direction);
                    }
                    sql.Append(", ");
                }
                sql.Remove(sql.Length - 2, 2);
            }
            
            // LIMIT is not part of ANSI-SQL, thus can't be handled here
            return sql.ToString();
        }
        
        public virtual string CreateUpdateStatement(string tableName,
                                                    IList<string> setColumnNames,
                                                    IList<string> setColumnValues,
                                                    IList<string> whereColumnNames,
                                                    IList<string> whereColumnOperators,
                                                    IList<string> whereColumnValues)
        {
            if (setColumnNames == null) {
                throw new ArgumentNullException("setColumnNames");
            }
            if (setColumnValues == null) {
                throw new ArgumentNullException("setColumnValues");
            }

            if (setColumnNames.Count != setColumnValues.Count) {
                throw new ArgumentException("setColumnNames and setColumnValues must have the same size.");
            }
            
            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("columnNames, columnOperators and columnValues must have the same size.");
                }
            }
            
            StringBuilder whereClause = null;
            if (whereColumnNames != null && whereColumnNames.Count > 0) {
                whereClause = new StringBuilder();
                for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                    whereClause.AppendFormat("{0} {1} {2} AND ",
                                             GetColumnName(whereColumnNames[idx]),
                                             whereColumnOperators[idx],
                                             whereColumnValues[idx]);
                }
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            return CreateUpdateStatement(tableName, setColumnNames, setColumnValues,
                                         whereClause != null ? whereClause.ToString() : null);
        }
        
        public virtual string CreateUpdateStatement(string tableName,
                                                    IList<string> setColumnNames,
                                                    IList<string> setColumnValues,
                                                    string whereClause)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (setColumnNames == null) {
                throw new ArgumentNullException("setColumnNames");
            }
            if (setColumnValues == null) {
                throw new ArgumentNullException("setColumnValues");
            }
            if (setColumnValues == null) {
                throw new ArgumentNullException("setColumnValues");
            }
            
            if (setColumnNames.Count == 0) {
                throw new ArgumentException("setColumnNames must not be empty.");
            }
            
            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(GetTableName(tableName));
            sql.Append(" SET ");
            for (int idx = 0; idx < setColumnNames.Count; idx++) {
                sql.AppendFormat("{0} = {1}, ", GetColumnName(setColumnNames[idx]), setColumnValues[idx]);
            }
            sql.Remove(sql.Length - 2, 2);
            
            if (whereClause != null && whereClause.Length != 0) {
                sql.AppendFormat(" WHERE {0}", whereClause);
            }

            return sql.ToString();
        }
        
        public virtual string CreateDeleteStatement(string tableName, string whereClause)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            
            StringBuilder sql = new StringBuilder("DELETE FROM ");
            sql.Append(GetTableName(tableName));
            if (whereClause != null) {
                sql.AppendFormat(" WHERE {0}", whereClause);
            }
            
            return sql.ToString();
        }
        
        public virtual string CreateDeleteStatement(string tableName,
                                                    IList<string> whereColumnNames,
                                                    IList<string> whereColumnOperators,
                                                    IList<string> whereColumnValues)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }

            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("columnNames, columnOperators and columnValues must have the same size.");
                }
            }
            
            StringBuilder whereClause = null;
            if (whereColumnNames != null && whereColumnNames.Count > 0) {
                whereClause = new StringBuilder();
                for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                    whereClause.AppendFormat("{0} {1} {2} AND ",
                                             GetColumnName(whereColumnNames[idx]),
                                             whereColumnOperators[idx],
                                             whereColumnValues[idx]);
                }
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            return CreateDeleteStatement(tableName,
                                         whereClause != null ? whereClause.ToString() : null);
        }

        public virtual string CreateSequenceStatement(string tableName,
                                                      string columnName,
                                                      int? seed,
                                                      int? increment)
        {
            throw new NotImplementedException();
        }

        private static bool IsNullableType(Type type)
        {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
