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
        private ISqlProvider  _SqlProvider;
        IDbTransaction Transaction { get; set; }

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

            try {
                string versionSql = _SqlProvider.CreateSelectVersionStatement();
                if (!String.IsNullOrEmpty(versionSql)) {
                    using (IDbCommand com = _DBConnection.CreateCommand()) {
                        com.CommandText = versionSql;
                        object res = com.ExecuteScalar();
                        if (res is string) {
                            _SqlProvider.VersionString = (string) res;
                        }
                    }
                }
            } catch {
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

            string sql = _SqlProvider.CreateDeleteStatement(GetTableName(tableType), null);
#if LOG4NET
            _Logger.Debug("EmptyTable(): SQL: " + sql);
#endif
            using (IDbCommand com = _DBConnection.CreateCommand()) {
                com.CommandText = sql;
                com.ExecuteNonQuery();
            }
        }
        
        public void DropTable(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            
            string sql = _SqlProvider.CreateDropTableStatement(GetTableName(tableType));
#if LOG4NET
            _Logger.Debug("DropTable(): SQL: " + sql);
#endif
            using (IDbCommand com = _DBConnection.CreateCommand()) {
                com.CommandText = sql;
                com.ExecuteNonQuery();
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

            var properties = tableType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var columnNames       = new List<string>(properties.Length);
            var columnTypes       = new List<Type>(properties.Length);
            var columnLengths     = new List<int>(properties.Length);
            var columnIsNullables = new List<bool>(properties.Length);
            var primaryKeyColumns = new List<string>();
            var sequenceColumns    = new List<string>();
            var sequenceAttributes = new Dictionary<string, SequenceAttribute>();
            foreach (PropertyInfo property in properties) {
                object[] columnAttrs = property.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrs == null || columnAttrs.Length == 0) {
                    continue;
                }
                ColumnAttribute columnAttr = (ColumnAttribute) columnAttrs [0];

                string columnName;
                if (columnAttr.Name != null) {
                    columnName = columnAttr.Name;
                } else {
                    columnName = property.Name;
                }
                columnNames.Add(columnName);
                columnTypes.Add(property.PropertyType);
                columnLengths.Add(columnAttr.Length);
                columnIsNullables.Add(columnAttr.IsNullable);

                object[] pkAttrs = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (pkAttrs != null && pkAttrs.Length > 0) {
                    primaryKeyColumns.Add(columnName);
                }

                object[] seqAttrs = property.GetCustomAttributes(typeof(SequenceAttribute), true);
                if (seqAttrs != null && seqAttrs.Length > 0) {
                    var seqAttr = (SequenceAttribute) seqAttrs [0];
                    sequenceAttributes.Add(columnName, seqAttr);
                    sequenceColumns.Add(columnName);
                }
            }
            if (columnNames.Count == 0) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "tableType");
            }

            string sql = _SqlProvider.CreateCreateTableStatement(GetTableName(tableType),
                                                                 columnNames,
                                                                 columnTypes,
                                                                 columnLengths,
                                                                 columnIsNullables,
                                                                 primaryKeyColumns,
                                                                 sequenceColumns);
#if LOG4NET
            _Logger.Debug("CreateTable(): SQL: " + sql);
#endif
            using (IDbCommand com = _DBConnection.CreateCommand()) {
                com.CommandText = sql;
                com.ExecuteNonQuery();
            }

            foreach (var sequence in sequenceAttributes) {
                if (primaryKeyColumns.Contains(sequence.Key)) {
                    // skip sequences on PK columns as they are already handled
                    // in CreateCreateTableStatement()
                    continue;
                }
                sql = _SqlProvider.CreateSequenceStatement(GetTableName(tableType),
                                                           sequence.Key,
                                                           sequence.Value.Seed,
                                                           sequence.Value.Increment);
#if LOG4NET
                _Logger.Debug("CreateTable(): SQL: " + sql);
#endif
                using (IDbCommand com = _DBConnection.CreateCommand()) {
                    com.CommandText = sql;
                    com.ExecuteNonQuery();
                }
            }
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
            
            string sql = _SqlProvider.CreateTableExistsStatement(GetTableName(tableType));
#if LOG4NET
            _Logger.Debug("TableExists(): SQL: " + sql);
#endif
            using (IDbCommand com = _DBConnection.CreateCommand()) {
                com.CommandText = sql;

                object res = com.ExecuteScalar();
                if (res is Int32) {
                    return (Int32) res > 0;
                } else if (res is Int64) {
                    return (Int64) res > 0;
                }

                throw new NotSupportedException("Unsupported type returned by COUNT(*): " + res.GetType());
            }
        }
        
        public virtual Query<T> CreateQuery<T>() where T : new()
        {
            return new Query<T>(this);
        }

        public virtual IDbCommand CreateInsertCommand(string tableName,
                                                      IList<string> columnNames,
                                                      IList<object> columnValues)
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

            IDbCommand command = _DBConnection.CreateCommand();
            command.Transaction = Transaction;
            List<string> parameterNames = new List<string>();
            for (int idx = 0; idx < columnValues.Count; idx++) {
                string parameterName = String.Format("{0}{1}", _SqlProvider.GetParameterCharacter(), idx);
                object value = columnValues[idx];

                // HACK: SqlConnection of Mono 1.2.6 and 1.9.1 sends DateTime incorrectly as varchar
                if (_DBConnection is System.Data.SqlClient.SqlConnection &&
                    value is DateTime) {
                    value = ((DateTime) value).ToString("s");
                }

                DbType dbType = _SqlProvider.GetDBType(value.GetType());
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.DbType = dbType;
                parameter.Value = value;
                command.Parameters.Add(parameter);
                parameterNames.Add(parameterName);
#if LOG4NET
               // _Logger.Debug("parameterName: " + parameterName + " dbType: " + dbType + " valueType: " + value.GetType() +  " value: " + value);
#endif
            }
            string sql = _SqlProvider.CreateInsertStatement(tableName, columnNames, parameterNames);
            command.CommandText = sql;
            return command;
        }

        public virtual IDbCommand CreateUpdateCommand(string tableName,
                                                      IList<string> setColumnNames,
                                                      IList<object> setColumnValues,
                                                      IList<string> whereColumnNames,
                                                      IList<string> whereColumnOperators,
                                                      IList<object> whereColumnValues)
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
            
            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("whereColumnNames, whereColumnOperators and whereColumnValues must have the same size.");
                }
            }
            
            IDbCommand command = _DBConnection.CreateCommand();
            List<string> setParameterNames   = new List<string>(setColumnValues.Count);
            for (int idx = 0; idx < setColumnNames.Count; idx++) {
                string parameterName = String.Format("{0}{1}",
                                                     _SqlProvider.GetParameterCharacter(),
                                                     idx);
                object value = setColumnValues[idx];

                // HACK: SqlConnection of Mono 1.2.6 and 1.9.1 sends DateTime incorrectly as varchar
                if (_DBConnection is System.Data.SqlClient.SqlConnection &&
                    value is DateTime) {
                    value = ((DateTime) value).ToString("s");
                }

                DbType dbType = _SqlProvider.GetDBType(value.GetType());
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.DbType = dbType;
                parameter.Value = value;
                command.Parameters.Add(parameter);
                setParameterNames.Add(parameterName);
            }

            List<string> whereParameterNames = new List<string>(whereColumnValues.Count);
            for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                string parameterName = String.Format("{0}{1}",
                                                     _SqlProvider.GetParameterCharacter(),
                                                     setColumnValues.Count + idx);
                object value = whereColumnValues[idx];

                // HACK: SqlConnection of Mono 1.2.6 and 1.9.1 sends DateTime incorrectly as varchar
                if (_DBConnection is System.Data.SqlClient.SqlConnection &&
                    value is DateTime) {
                    value = ((DateTime) value).ToString("s");
                }

                DbType dbType = _SqlProvider.GetDBType(value.GetType());
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.DbType = dbType;
                parameter.Value = value;
                command.Parameters.Add(parameter);
                whereParameterNames.Add(parameterName);
            }

            string sql = _SqlProvider.CreateUpdateStatement(tableName,
                                                            setColumnNames,
                                                            setParameterNames,
                                                            whereColumnNames,
                                                            whereColumnOperators,
                                                            whereParameterNames);
            command.CommandText = sql;
            return command;
        }

        public virtual IDbCommand CreateSelectCommand(string tableName,
                                                      IList<string> selectColumnNames,
                                                      IList<string> whereColumnNames,
                                                      IList<string> whereColumnOperators,
                                                      IList<object> whereColumnValues,
                                                      IList<string> orderByColumnNames,
                                                      IList<string> orderByColumnOrders,
                                                      int? limit,
                                                      int? offset)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }

            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("whereColumnNames, whereColumnOperators and whereColumnValues must have the same size.");
                }
            }

            IDbCommand command = _DBConnection.CreateCommand();
            command.Transaction = Transaction;
            List<string> parameterNames = null;
            if (whereColumnNames != null) {
                parameterNames = new List<string>(whereColumnNames.Count);
                for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                    string parameterName = String.Format("{0}{1}", _SqlProvider.GetParameterCharacter(), idx);
                    object value = whereColumnValues[idx];

                    // HACK: SqlConnection of Mono 1.2.6 and 1.9.1 sends DateTime incorrectly as varchar
                    if (_DBConnection is System.Data.SqlClient.SqlConnection &&
                        value is DateTime) {
                        value = ((DateTime) value).ToString("s");
                    }

                    DbType dbType = _SqlProvider.GetDBType(value.GetType());
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.DbType = dbType;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                    parameterNames.Add(parameterName);
                }
            }

            string sql = _SqlProvider.CreateSelectStatement(null, tableName,
                                                            selectColumnNames,
                                                            whereColumnNames,
                                                            whereColumnOperators,
                                                            parameterNames,
                                                            orderByColumnNames,
                                                            orderByColumnOrders,
                                                            limit,
                                                            offset);
            command.CommandText = sql;
            return command;
        }

        public virtual IDbCommand CreateDeleteCommand(string tableName,
                                                      IList<string> whereColumnNames,
                                                      IList<string> whereColumnOperators,
                                                      IList<object> whereColumnValues)
        {
            if (tableName == null) {
                throw new ArgumentNullException("tableName");
            }
            
            if (whereColumnNames != null &&
                whereColumnOperators != null &&
                whereColumnValues != null) {
                if (!(whereColumnNames.Count == whereColumnOperators.Count &&
                      whereColumnOperators.Count == whereColumnValues.Count)) {
                    throw new ArgumentException("whereColumnNames, whereColumnOperators and whereColumnValues must have the same size.");
                }
            }

            IDbCommand command = _DBConnection.CreateCommand();
            List<string> whereParameterNames = null;
            if (whereColumnNames != null && whereColumnNames.Count > 0) {
                whereParameterNames = new List<string>(whereColumnNames.Count);
                for (int idx = 0; idx < whereColumnNames.Count; idx++) {
                    string parameterName = String.Format("{0}{1}",
                                                         _SqlProvider.GetParameterCharacter(),
                                                         idx);
                    object value = whereColumnValues[idx];

                    // HACK: SqlConnection of Mono 1.2.6 and 1.9.1 sends DateTime incorrectly as varchar
                    if (_DBConnection is System.Data.SqlClient.SqlConnection &&
                        value is DateTime) {
                        value = ((DateTime) value).ToString("s");
                    }

                    DbType dbType = _SqlProvider.GetDBType(value.GetType());
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = parameterName;
                    parameter.DbType = dbType;
                    parameter.Value = value;
                    command.Parameters.Add(parameter);
                    whereParameterNames.Add(parameterName);
                }
            }

            string sql = _SqlProvider.CreateDeleteStatement(tableName,
                                                            whereColumnNames,
                                                            whereColumnOperators,
                                                            whereParameterNames);
            command.CommandText = sql;
            return command;
        }

        public virtual string GetTableName(Type tableType)
        {
            if (tableType == null) {
                throw new ArgumentNullException("tableType");
            }
            
            object[] attrs = tableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("tableType", "Type does not contain TableAttribute.");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            if (attr.Name != null) {
                return attr.Name;
            }
            
            string fullName = tableType.FullName;
            return fullName.Substring(fullName.LastIndexOf(".") + 1);
        }

        public virtual IDbTransaction BeginTransaction()
        {
            Transaction = DBConnection.BeginTransaction();
            return Transaction;
        }
    }
}
