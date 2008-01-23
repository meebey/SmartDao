using System;
using System.Data;
using System.Text;
using System.Reflection;

namespace Meebey.SmartDao
{
    public class DatabaseManager
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        private IDbConnection _DBConnection;
        private SqlProvider   _SqlProvider;
        
        public SqlProvider SqlProvider {
            get {
                return _SqlProvider;
            }
        }

        public IDbConnection DBConnection {
            get {
                return _DBConnection;
            }
        }
        
        public DatabaseManager(IDbConnection dbConnection) : this(dbConnection, null)
        {
        }
        
        public DatabaseManager(IDbConnection dbConnection, SqlProvider sqlProvider)
        {
            if (dbConnection == null) {
                throw new ArgumentNullException("dbConnection");
            }

            _DBConnection = dbConnection;
            if (sqlProvider == null) {
                _SqlProvider = new SqlProvider();
            }
        }
        
        public void InitTables(Assembly assembly)
        {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }
            
            Type[] types = assembly.GetTypes();
            foreach (Type type in types) {
                object[] attrs = type.GetCustomAttributes(typeof(TableAttribute), true);
                if (attrs == null || attrs.Length == 0) {
                    continue; 
                }
                
                TableAttribute attr = (TableAttribute) attrs[0];
                InitTable(type, attr);
            }
        }
        
        public void CreateTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }

            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("Type does not contain a TableAttribute.", "tableType");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            CreateTable(tableType, attr);
        }
        
        private bool IsNullableType(Type type)
        {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
        
        private void CreateTable(Type tableType, TableAttribute tableAttribute)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            if (tableAttribute == null) {
                throw new ArgumentNullException("tableAttribute");
            }
            
            IDbCommand com = _DBConnection.CreateCommand();
            IDbDataParameter param;

            StringBuilder sql = new StringBuilder("CREATE TABLE ");
            sql.AppendFormat("{0} ( ", _SqlProvider.GetTableName(tableAttribute.Name));
            
            /*
            param = com.CreateParameter();
            param.ParameterName = "@table_name";
            param.DbType = DbType.String;
            param.Value = tableAttribute.Name;
            com.Parameters.Add(param);
            */
            
            FieldInfo[] fields = tableType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            bool foundColumn = false;
            foreach (FieldInfo field in fields) {
                object[] columnAttrs = field.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrs == null || columnAttrs.Length == 0) {
                    continue;
                }
                foundColumn = true;
                
                ColumnAttribute columnAttr = (ColumnAttribute) columnAttrs [0];
                sql.AppendFormat("{0} ", _SqlProvider.GetColumnName(columnAttr.Name));
                
                /*
                param = com.CreateParameter();
                param.ParameterName = "@column_name";
                param.DbType = DbType.String;
                param.Value = columnAttr.Name;
                com.Parameters.Add(param);
                */
                
                if (field.FieldType == typeof(string)) {
                    // TODO: CHAR support
                    if (columnAttr.Length == -1) {
                        sql.Append("TEXT");
                    } else {
                        sql.Append(_SqlProvider.GetDataTypeName(DbType.String));
                        if (columnAttr.Length != 0) {
                            sql.AppendFormat("({0})", columnAttr.Length);
                        }
                    }
                } else if (field.FieldType == typeof(Boolean) ||
                           field.FieldType == typeof(Boolean?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Boolean));
                } else if (field.FieldType == typeof(Int32) ||
                           field.FieldType == typeof(Int32?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Int32));
                } else if (field.FieldType == typeof(Int64) ||
                           field.FieldType == typeof(Int64?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Int64));
                } else if (field.FieldType == typeof(Decimal) ||
                           field.FieldType == typeof(Decimal?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Decimal));
                } else if (field.FieldType == typeof(Single) ||
                           field.FieldType == typeof(Single?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Single));
                } else if (field.FieldType == typeof(Double) ||
                           field.FieldType == typeof(Double?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.Double));
                } else if (field.FieldType == typeof(DateTime) ||
                           field.FieldType == typeof(DateTime?)) {
                    sql.Append(_SqlProvider.GetDataTypeName(DbType.DateTime));
                } else {
                    throw new ApplicationException("Unsupported column data type: " + field.FieldType);
                }
                
                if (field.FieldType.IsValueType) {
                    if (IsNullableType(field.FieldType)) {
                        sql.Append(" NULL");
                    } else {
                        sql.Append(" NOT NULL");
                    }
                }
                
                sql.Append(", ");
            }
            if (!foundColumn) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "tableType");
            }
            sql.Remove(sql.Length - 2, 2);
                
            sql.Append(" );");
            com.CommandText = sql.ToString();
#if LOG4NET
            _Logger.Debug("CreateTable(): SQL: " + com.CommandText);
#endif
            com.ExecuteNonQuery();
        }
        
        public void InitTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }

            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("tableType", "does not contain TableAttribute.");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            InitTable(tableType, attr);
        }
        
        private void InitTable(Type tableType, TableAttribute tableAttribute)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            if (tableAttribute == null) {
                throw new ArgumentNullException("tableAttribute");
            }

            if (!TableExists(tableAttribute.Name)) {
#if LOG4NET
                _Logger.Debug("InitTable(): creating table: " + tableAttribute.Name);
#endif
                CreateTable(tableType, tableAttribute);
            } else {
#if LOG4NET
                _Logger.Debug("InitTable(): table exists: " + tableAttribute.Name);
#endif
                // verify structure
            }
        }

        public virtual bool TableExists(string tableName)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            
            IDbCommand com = _DBConnection.CreateCommand();
            com.CommandText = "SELECT COUNT(*) "+
                              "FROM INFORMATION_SCHEMA.TABLES " +
                              "WHERE TABLE_NAME = @table_name";
            IDbDataParameter param = com.CreateParameter();
            param.ParameterName = "@table_name";
            param.DbType = DbType.String;
            param.Value = tableName;
            com.Parameters.Add(param);
            
            Int64 count = (Int64) com.ExecuteScalar();
            return count > 0;
        }
    }
}
