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
            }
            if (columnNames.Count == 0) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "tableType");
            }
            
            string sql = _SqlProvider.GetCreateStatement(tableAttribute.Name,
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
        
        public virtual Query<T> CreateQuery<T>()
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
    }
}
