using System;
using System.Collections.Generic;
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
        private ISqlProvider   _SqlProvider;
        
        public ISqlProvider SqlProvider {
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
        
        public DatabaseManager(IDbConnection dbConnection, ISqlProvider sqlProvider)
        {
            if (dbConnection == null) {
                throw new ArgumentNullException("dbConnection");
            }

            _DBConnection = dbConnection;
            if (sqlProvider == null) {
                _SqlProvider = new AnsiSqlProvider();
            } else {
                _SqlProvider = sqlProvider;
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
        
        public void EmptyTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            
            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("Type does not contain a TableAttribute.", "tableType");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            
            string sql = _SqlProvider.GetDeleteStatement(attr.Name, null);
#if LOG4NET
            _Logger.Debug("EmptyTable(): SQL: " + sql);
#endif
            IDbCommand com = _DBConnection.CreateCommand();
            com.CommandText = sql;
            com.ExecuteNonQuery();
        }
        
        public void DropTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            
            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("Type does not contain a TableAttribute.", "tableType");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            
            string sql = _SqlProvider.GetDropTableStatement(attr.Name);
#if LOG4NET
            _Logger.Debug("DropTable(): SQL: " + sql);
#endif
            IDbCommand com = _DBConnection.CreateCommand();
            com.CommandText = sql;
            com.ExecuteNonQuery();
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
        
        private void CreateTable(Type tableType, TableAttribute tableAttribute)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            if (tableAttribute == null) {
                throw new ArgumentNullException("tableAttribute");
            }
            

            FieldInfo[] fields = tableType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            List<string> columnNames       = new List<string>(fields.Length); 
            List<Type>   columnTypes       = new List<Type>(fields.Length); 
            List<int>    columnLengths     = new List<int>(fields.Length);
            List<string> primaryKeyColumns = new List<string>();
            foreach (FieldInfo field in fields) {
                object[] columnAttrs = field.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrs == null || columnAttrs.Length == 0) {
                    continue;
                }
                ColumnAttribute columnAttr = (ColumnAttribute) columnAttrs [0];
                
                columnNames.Add(columnAttr.Name);
                columnTypes.Add(field.FieldType);
                columnLengths.Add(columnAttr.Length);
            
                object[] pkAttrs = field.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (pkAttrs == null || pkAttrs.Length == 0) {
                    continue;
                }
                PrimaryKeyAttribute pkAttr = (PrimaryKeyAttribute) pkAttrs [0];
                primaryKeyColumns.Add(columnAttr.Name);
            }
            if (columnNames.Count == 0) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "tableType");
            }
            
            string sql = _SqlProvider.GetCreateTableStatement(tableAttribute.Name,
                                                              columnNames,
                                                              columnTypes,
                                                              columnLengths,
                                                              primaryKeyColumns);
#if LOG4NET
            _Logger.Debug("CreateTable(): SQL: " + sql);
#endif
            IDbCommand com = _DBConnection.CreateCommand();
            com.CommandText = sql;
            com.ExecuteNonQuery();
        }
        
        public void InitTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }

            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("tableType", "Type does not contain TableAttribute.");
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

            if (!TableExists(tableType)) {
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

        public virtual bool TableExists(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            
            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("tableType", "Type does not contain TableAttribute.");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            
            List<string> columns = new List<string>(new string[] {"COUNT(*)"});
            string sql = _SqlProvider.GetSelectStatement("INFORMATION_SCHEMA.TABLES", columns, "TABLE_NAME = @table_name");
#if LOG4NET
            _Logger.Debug("TableExists(): SQL: " + sql);
#endif
            IDbCommand com = _DBConnection.CreateCommand();
            com.CommandText = sql;
            IDbDataParameter param = com.CreateParameter();
            param.ParameterName = "@table_name";
            param.DbType = DbType.String;
            param.Value = attr.Name;
            com.Parameters.Add(param);
            
            object res = com.ExecuteScalar();
            if (res is Int32) {
                return (Int32) res > 0; 
            } else if (res is Int64) {
                return (Int64) res > 0;
            }
            
            throw new NotSupportedException("Unsupported type returned by COUNT(*): " + res.GetType());
        }
        
        public virtual Query<T> CreateQuery<T>() where T : new()
        {
            return new Query<T>(this);
        }
        
        public virtual IDbCommand CreateInsertCommand(string tableName,
                                                      IList<string> columnNames,
                                                      IList<object> columnValues) {
            string sql = _SqlProvider.GetInsertStatement(tableName, columnNames);
            IDbCommand command = _DBConnection.CreateCommand();
            command.CommandText = sql;
            for (int idx = 0; idx < columnValues.Count; idx++) {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = String.Format("@{0}", idx);
                // HACK: map CLI type to DbType
                parameter.DbType = DbType.String;
                parameter.Value = columnValues[idx];
                command.Parameters.Add(parameter);
            }
            return command;
        }

        public virtual IDbCommand CreateSelectCommand(string tableName,
                                                      IList<string> selectColumnNames,
                                                      IList<string> whereColumnNames,
                                                      IList<object> whereColumnValues) {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            if (selectColumnNames == null) {
                throw new ArgumentNullException("selectColumnNames");
            }
            if (whereColumnNames == null) {
                throw new ArgumentNullException("whereColumnNames");
            }
            if (whereColumnValues == null) {
                throw new ArgumentNullException("whereColumnValues");
            }
            
            StringBuilder whereClause = new StringBuilder();
            IDbCommand command = _DBConnection.CreateCommand();
            for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                string name = whereColumnNames[idx];
                object value = whereColumnValues[idx];
                
                whereClause.Append(_SqlProvider.GetColumnName(name));
                if (value is string) {
                    string strValue = (string) value;
                    if (strValue.StartsWith("%") || strValue.EndsWith("%")) {
                        whereClause.Append(" LIKE ");
                    } else {
                        whereClause.Append(" = ");
                    }
                } else {
                    whereClause.Append(" = ");
                }
                whereClause.AppendFormat("@{0} AND ", idx);
                
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = String.Format("@{0}", idx);
                // HACK: map CLI type to DbType
                parameter.DbType = DbType.String;
                parameter.Value = value;
                command.Parameters.Add(parameter);
            }
            if (whereColumnNames.Count > 0) {
                whereClause.Remove(whereClause.Length - 4, 4);
            }
            
            string sql = _SqlProvider.GetSelectStatement(tableName, selectColumnNames, whereClause.ToString());
            command.CommandText = sql;
            return command;
        }
    }
}
