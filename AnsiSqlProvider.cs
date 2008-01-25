using System;
using System.Text;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class SqlProvider
    {
        public SqlProvider()
        {
        }
        
        public virtual string GetCreateStatement(string tableName, IList<string> columnNames, IList<Type> columnTypes, IList<int> columnLengths, IList<string> primaryKeys)
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
                    throw new ApplicationException("Unsupported column data type: " + type);
                }
                
                if (type.IsValueType) {
                    if (IsNullableType(type)) {
                        sql.Append(" NULL");
                    } else {
                        sql.Append(" NOT NULL");
                    }
                }
                
                sql.Append(", \n");
            }
            sql.Remove(sql.Length - 3, 3);
            sql.Append(" )");
            return sql.ToString();
        }
        
        public virtual string GetInsertStatement(string tableName, IList<string> columnNames)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (columnNames == null) {
                throw new ArgumentNullException("columnNames");
            }

            if (columnNames.Count == 0) {
                throw new ArgumentException("columnNames and columnValues must not be empty.");
            }
            
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(GetTableName(tableName));
            sql.Append(" (");
            
            StringBuilder values = new StringBuilder();
            int paramaterNumber = 0;
            for (int idx = 0; idx < columnNames.Count; idx++) {
                string name = columnNames[idx];
                string parameterName = String.Format("@{0}", paramaterNumber++);
                
                sql.AppendFormat("{0}, ", GetColumnName(name));
                values.AppendFormat("{0}, ", parameterName);
            }
            sql.Remove(sql.Length - 2, 2);
            sql.Append(") VALUES (");
            sql.Append(values);
            sql.Remove(sql.Length - 2, 2);
            sql.Append(")");
            
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
            
        public virtual string GetStatementSeparator()
        {
            return ";";
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
