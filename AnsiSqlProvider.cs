using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class AnsiSqlProvider : ISqlProvider
    {
        public AnsiSqlProvider()
        {
        }
        
        public virtual string CreateCreateTableStatement(string tableName, IList<string> columnNames, IList<Type> columnTypes, IList<int> columnLengths, IList<bool?> columnIsNullables, IList<string> primaryKeys)
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
                bool? isNullable = columnIsNullables[idx];
                
                if (type.IsValueType && !IsNullableType(type)) {
                    throw new NotSupportedException("Value type: " + type + " must be nullable for column: " + name);
                }
                
                sql.AppendFormat("{0} ", GetColumnName(name));
                if (type == typeof(string)) {
                    // TODO: CHAR support
                    if (length == -1) {
                        sql.Append("TEXT");
                    } else {
                        sql.Append(GetDataTypeName(DbType.String));
                        if (length != 0) {
                            sql.AppendFormat("({0})", length);
                        }
                    }
                } else if (type == typeof(Boolean) ||
                           type == typeof(Boolean?)) {
                    sql.Append(GetDataTypeName(DbType.Boolean));
                } else if (type == typeof(Int32) ||
                           type == typeof(Int32?)) {
                    sql.Append(GetDataTypeName(DbType.Int32));
                } else if (type == typeof(Int64) ||
                           type == typeof(Int64?)) {
                    sql.Append(GetDataTypeName(DbType.Int64));
                } else if (type == typeof(Decimal) ||
                           type == typeof(Decimal?)) {
                    sql.Append(GetDataTypeName(DbType.Decimal));
                } else if (type == typeof(Single) ||
                           type == typeof(Single?)) {
                    sql.Append(GetDataTypeName(DbType.Single));
                } else if (type == typeof(Double) ||
                           type == typeof(Double?)) {
                    sql.Append(GetDataTypeName(DbType.Double));
                } else if (type == typeof(DateTime) ||
                           type == typeof(DateTime?)) {
                    sql.Append(GetDataTypeName(DbType.DateTime));
                } else {
                    throw new NotSupportedException("Unsupported column data type: " + type);
                }
                
                if (!isNullable) {
                    sql.Append(" NOT NULL");
                }
                
                sql.Append(", \n");
            }
            sql.Remove(sql.Length - 3, 3);
            
            if (primaryKeys.Count > 0) {
                sql.Append(", \nPRIMARY KEY (");
                foreach (string pk in primaryKeys) {
                    sql.AppendFormat("{0}, ", pk);
                }
                sql.Remove(sql.Length - 2, 2);
                sql.Append(")");
            }
            // CREATE UNIQUE INDEX title_idx ON films (title);
            // CONSTRAINT valid_discount CHECK (price > discounted_price)
            // UNIQUE (product_no)
            // FOREIGN KEY (b, c) REFERENCES other_table (c1, c2)
            sql.Append(")");
            return sql.ToString();
        }
        
        public virtual string CreateInsertStatement(string tableName, IList<string> columnNames, IList<string> columnValues)
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
            return CreateSelectStatement("INFORMATION_SCHEMA", "TABLES",
                                         new string[] {"COUNT(*)"},
                                         String.Format("TABLE_NAME = '{0}'", tableName));
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
                                                    IList<string> whereColumnValues)
        {
            if (whereColumnNames == null) {
                throw new ArgumentNullException("whereColumnNames");
            }
            if (whereColumnOperators == null) {
                throw new ArgumentNullException("whereColumnOperators");
            }
            if (whereColumnValues == null) {
                throw new ArgumentNullException("whereColumnValues");
            }

            if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                  whereColumnOperators.Count == whereColumnValues.Count)) {
                throw new ArgumentException("columnNames, columnOperators and columnValues must have the same size.");
            }
            
            if (whereColumnNames.Count == 0) {
                throw new ArgumentException("whereColumnNames must not be empty.");
            }

            StringBuilder whereClause = new StringBuilder();
            for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                whereClause.AppendFormat("{0} {1} {2}, ",
                                         GetColumnName(whereColumnNames[idx]),
                                         whereColumnOperators[idx],
                                         whereColumnValues[idx]);
            }
            whereClause.Remove(whereClause.Length - 2, 2);
            return CreateSelectStatement(schemaName, tableName, selectColumnNames, whereClause.ToString());
        }
        
        public virtual string CreateSelectStatement(string schemaName, string tableName, IList<string> selectColumnNames, string whereClause)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (selectColumnNames == null) {
                throw new ArgumentNullException("selectColumnNames");
            }
            
            if (selectColumnNames.Count == 0) {
                throw new ArgumentException("selectColumnNames must not be empty.");
            }
            
            StringBuilder sql = new StringBuilder("SELECT ");
            for (int idx = 0; idx < selectColumnNames.Count; idx++) {
                sql.AppendFormat("{0}, ", GetColumnName(selectColumnNames[idx]));
            }
            sql.Remove(sql.Length - 2, 2);
            
            sql.Append(" FROM ");
            sql.Append(GetTableName(schemaName, tableName));
            
            if (whereClause != null && whereClause.Length != 0) {
                sql.AppendFormat(" WHERE {0}", whereClause);
            }
            
            return sql.ToString();
        }
        
        public virtual string CreateUpdateStatement(string tableName, IList<string> setColumnNames, IList<string> setColumnValues,  string whereClause)
        {
            return null;
        }
        
        public virtual string CreateDeleteStatement(string tableName, string whereClause)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            
            StringBuilder sql = new StringBuilder("DELETE FROM ");
            sql.Append(GetTableName(tableName));
            if (whereClause != null) {
                sql.AppendFormat("WHERE {0}", whereClause);
            }
            
            return sql.ToString();
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
        
        private static bool IsNullableType(Type type)
        {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
