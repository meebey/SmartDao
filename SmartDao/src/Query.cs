using System;
using System.Data;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class Query<T> where T : new()
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.GetGenericTypeDefinition());
#endif
        private DatabaseManager f_DatabaseManager;
        private Type            f_TableType;
        private string          f_TableName;
        private IDictionary<string, PropertyInfo> f_ColumnToProperties;
        private IList<string>                     f_PrimaryKeyColumns;
        private IList<string>                     f_SequenceColumns;
        private IDictionary<string, string>       f_PropertyToColumn;

        public Query(DatabaseManager dbManager)
        {
            if (dbManager == null) {
                throw new ArgumentNullException("dbManager");
            }
            
            f_DatabaseManager = dbManager;
            f_TableType = typeof(T);
        }
        
        private void InitFields()
        {
            if (f_ColumnToProperties != null) {
                return;
            }
            
#if LOG4NET
            DateTime start, stop;
            start = DateTime.UtcNow;
#endif
            object[] tableAttrs = f_TableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttrs == null || tableAttrs.Length == 0) {
                throw new ArgumentException("T", "Type does not contain a TableAttribute.");
            }
            
            TableAttribute tableAttr = (TableAttribute) tableAttrs[0];
            f_TableName = tableAttr.Name;
            
            PropertyInfo[] properties = f_TableType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            f_ColumnToProperties = new Dictionary<string, PropertyInfo>(properties.Length);
            f_PropertyToColumn   = new Dictionary<string, string>(properties.Length);
            f_PrimaryKeyColumns  = new List<string>();
            f_SequenceColumns    = new List<string>();
            bool foundColumn = false;
            foreach (PropertyInfo property in properties) {
                object[] columnAttrs = property.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrs == null || columnAttrs.Length == 0) {
                    continue;
                }
                foundColumn = true;
                
                ColumnAttribute columnAttr = (ColumnAttribute) columnAttrs[0];
                if (property.PropertyType.IsValueType &&
                    !(property.PropertyType.IsGenericType &&
                      property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                    throw new NotSupportedException("Properties with ColumnAttribute (" + columnAttr.Name + ") must be reference types or nullable value types.");
                }
                string columnName;
                if (columnAttr.Name != null) {
                    columnName = columnAttr.Name;
                } else {
                    columnName = property.Name;
                }
#if LOG4NET
                _Logger.Debug(String.Format("InitFields(): found Column Property: '{0}' (Column Name: '{1}')",
                                            property.Name, columnName));
#endif
                f_ColumnToProperties.Add(columnName, property);
                f_PropertyToColumn.Add(property.Name, columnName);
                
                object[] pkAttrs = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (pkAttrs != null && pkAttrs.Length > 0) {
#if LOG4NET
                    _Logger.Debug(String.Format("InitFields(): found PrimaryKey Property: '{0}'",
                                                property.Name));
#endif
                    f_PrimaryKeyColumns.Add(columnName);
                }
                object[] seqAttrs = property.GetCustomAttributes(typeof(SequenceAttribute), true);
                if (seqAttrs != null && seqAttrs.Length > 0) {
#if LOG4NET
                    _Logger.Debug(String.Format("InitFields(): found Sequence Property: '{0}'",
                                                property.Name));
#endif
                    f_SequenceColumns.Add(columnName);
                }
            }
            if (!foundColumn) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "_TableType");
            }
#if LOG4NET
            stop = DateTime.UtcNow;
            _Logger.Debug("InitFields(): took: " + (stop - start).TotalMilliseconds + " ms");
#endif
        }
        
        private string ParseCommandParameters(IDbCommand cmd)
        {
            if (cmd == null) {
                throw new ArgumentNullException("cmd");
            }
            
            string parsed_sql = cmd.CommandText;
            foreach (IDataParameter param in cmd.Parameters) {
                string value;
                if (param.Value is DBNull) {
                    value = "NULL";
                } else if (param.Value is string ||
                           param.Value is DateTime) {
                    value = String.Format("'{0}'", param.Value);
                } else {
                    value = param.Value.ToString();
                }
                parsed_sql = parsed_sql.Replace(param.ParameterName, value);
            }
            
            return parsed_sql;
        }

        private T CreateDBObject(IDataReader reader)
        {
            T row = new T();
            for (int i = 0; i < reader.FieldCount; i++) {
                if (reader.IsDBNull(i)) {
                    // don't need to set null values
                    continue;
                }
    
                string columnName = reader.GetName(i);
                object columnValue = reader.GetValue(i);

                //
                SetPropertyValue(row, columnName, columnValue);
            }

            return row;
        }

        private void SetPropertyValue(T row, string columnName, object columnValue)
        {
            if (row == null) {
                throw new ArgumentNullException("row");
            }
            if (columnName == null) {
                throw new ArgumentNullException("columnName");
            }
            if (columnValue == null) {
                throw new ArgumentNullException("columnValue");
            }

            PropertyInfo property;
            if (!f_ColumnToProperties.TryGetValue(columnName, out property)) {
                throw new InvalidOperationException("Property for column could not be found: " + columnName);
            }
            if (!property.PropertyType.IsAssignableFrom(columnValue.GetType())) {
                // make sure to use the unboxed Nullable<> type here
                Type targetType;
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    targetType = property.PropertyType.GetGenericArguments()[0];
                } else {
                    targetType = property.PropertyType;
                }
                bool convertible = false;
                try {
                    columnValue = Convert.ChangeType(columnValue, targetType);
                    convertible = true;
                } catch (InvalidCastException) {
                } catch (FormatException) {
                }
                if (!convertible) {
                    throw new InvalidOperationException(
                                String.Format("Column: '{0}.{1}' with type: {2} is not convertible to property: '{3}.{4}' with type: {5}",
                                              f_TableName, columnName, columnValue.GetType(), f_TableType.Name, property.Name, targetType));
                }
            }

            property.SetValue(row, columnValue, null);
        }

        public IList<T> Add(IList<T> entry)
        {
            // TODO: implement multi-inserts here
            throw new NotImplementedException();
        }

        public T Add(T entry)
        {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }

            InitFields();
            
            List<string> columnNames  = new List<string>(f_ColumnToProperties.Count);
            List<object> columnValues = new List<object>(f_ColumnToProperties.Count);
            foreach (KeyValuePair<string, PropertyInfo> kv in f_ColumnToProperties) {
                string columnName = kv.Key;
                PropertyInfo property = kv.Value;
                
                object value = property.GetValue(entry, null);
                if (value == null) {
                    continue;
                }
                columnNames.Add(columnName);
                columnValues.Add(value);
            }

            var pkValues = new Dictionary<string, object>(f_PrimaryKeyColumns.Count);
            bool isPrimaryKeySequence = false;
            string pkSequenceColumn = null;
            Type pkSequenceColumnType = null;
            foreach (string pkColumn in f_PrimaryKeyColumns) {
                PropertyInfo property = f_ColumnToProperties[pkColumn];

                var pkValue = property.GetValue(entry, null);
                if (pkValue == null &&
                    f_SequenceColumns.Contains(pkColumn)) {
                    isPrimaryKeySequence = true;
                    pkSequenceColumn = pkColumn;
                    // make sure to use the unboxed Nullable<> type here
                    pkSequenceColumnType = property.PropertyType.GetGenericArguments()[0];
                    break;
                }
                pkValues.Add(pkColumn, pkValue);
            }

            using (IDbCommand cmd = f_DatabaseManager.CreateInsertCommand(
                                        f_TableName,
                                        columnNames,
                                        columnValues)) {
#if LOG4NET
                _Logger.Debug("Add(): SQL: " + cmd.CommandText);
                _Logger.Debug("Add(): parsed SQL: " + ParseCommandParameters(cmd));

                DateTime start, stop;
                start = DateTime.UtcNow;
#endif

                T pkEntry = new T();
                if (isPrimaryKeySequence) {
                    // this table uses auto generated keys
                    // try to obtain the generated key and write it back
                    object pkValue = cmd.ExecuteScalar();
                    /*
                    using (IDataReader reader = cmd.ExecuteReader()) {
                        // HACK: some RDBMSs return an empty result set first,
                        // while others do not. Thus we have to try to obtain
                        // the key in both ways.
                        if (reader.Read() || (reader.NextResult() && reader.Read())) {
                            pkValue = reader.GetValue(0);
                        }
                    }
                    */
                    if (pkValue == null) {
                        throw new DataNotFoundException("Couldn't obtain auto-generated key from INSERT");
                    }
                    pkValues.Add(
                        pkSequenceColumn,
                        Convert.ChangeType(pkValue,
                                           pkSequenceColumnType)
                    );
                } else {
                    cmd.ExecuteNonQuery();
                }

                foreach (KeyValuePair<string, object> pkColumn in pkValues) {
                    SetPropertyValue(pkEntry, pkColumn.Key, pkColumn.Value);
                }

#if LOG4NET
                stop = DateTime.UtcNow;
                _Logger.Debug("Add(): query took: " + (stop - start).TotalMilliseconds + " ms");
#endif
                return pkEntry;
            }
        }

        public void SetSingle(T record)
        {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            InitFields();

            foreach (string pkColumn in f_PrimaryKeyColumns) {
                PropertyInfo property = f_ColumnToProperties[pkColumn];

                object value = property.GetValue(record, null);
                if (value == null) {
                    throw new MissingPrimaryKeyException("Property: " + property.Name + " must not be null.");
                }
            }

            int res = SetAll(record);
            if (res == 0) {
                throw new DataNotFoundException();
            }
            if (res > 1) {
                throw new TooMuchDataUpdatedException();
            }
        }

        /*
        public int SetAll(T template, T value)
        {
            // support update of PK
        }
        */

        public int SetAll(T entry)
        {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }
            
            InitFields();
            
            List<string> setColumnNames       = new List<string>();
            List<object> setColumnValues      = new List<object>();
            List<string> whereColumnNames     = new List<string>(f_PrimaryKeyColumns.Count);
            List<string> whereColumnOperators = new List<string>(f_PrimaryKeyColumns.Count);
            List<object> whereColumnValues    = new List<object>(f_PrimaryKeyColumns.Count);
            foreach (KeyValuePair<string, PropertyInfo> kv in f_ColumnToProperties) {
                string columnName = kv.Key;
                PropertyInfo property = kv.Value;

                object value = property.GetValue(entry, null);
                if (value == null) {
                    continue;
                }
                
                if (f_PrimaryKeyColumns.Contains(columnName)) {
                    whereColumnNames.Add(columnName);
                    whereColumnOperators.Add("=");
                    whereColumnValues.Add(value);
                } else {
                    setColumnNames.Add(columnName);
                    setColumnValues.Add(value);
                }
            }
            
            using (IDbCommand cmd = f_DatabaseManager.CreateUpdateCommand(
                                        f_TableName,
                                        setColumnNames,
                                        setColumnValues,
                                        whereColumnNames,
                                        whereColumnOperators,
                                        whereColumnValues)) {
#if LOG4NET
                _Logger.Debug("SetAll(): SQL: " + cmd.CommandText);
                _Logger.Debug("SetAll(): parsed SQL: " + ParseCommandParameters(cmd));

                DateTime start, stop;
                start = DateTime.UtcNow;
#endif
                int res = cmd.ExecuteNonQuery();
#if LOG4NET
                _Logger.Debug("SetAll(): affected rows: " + res);
                stop = DateTime.UtcNow;
                _Logger.Debug("SetAll(): query took: " + (stop - start).TotalMilliseconds + " ms");
#endif
                return res;
            }
        }

        public T GetSingle(T template, params string[] selectColumns)
        {
            if (template == null) {
                throw new ArgumentNullException("template");
            }

            InitFields();

            foreach (string pkColumn in f_PrimaryKeyColumns) {
                PropertyInfo property = f_ColumnToProperties[pkColumn];

                object value = property.GetValue(template, null);
                if (value == null) {
                    throw new MissingPrimaryKeyException("Property: " + property.Name + " must not be null.");
                }
            }

            IList<T> res = GetAll(template, selectColumns);
            if (res.Count == 0) {
                throw new DataNotFoundException();
            }

            return res[0];
        }

        public T GetFirst(T template, GetOptions options)
        {
            if (options == null) {
                throw new ArgumentNullException("options");
            }

            options.Limit = 1;
            IList<T> res = GetAll(template, options);
            if (res.Count == 0) {
                throw new DataNotFoundException();
            }

            return res[0];
        }

        public T GetFirst(T template, params string[] selectColumns)
        {
            GetOptions options = new GetOptions();
            options.SelectFields = selectColumns;
            return GetFirst(template, options);
        }

        /*
        public int GetCount(T template, GetOptions options)
        {
            if (options == null) {
                throw new ArgumentNullException("options");
            }
            
            options.SelectFields = new string { "COUNT(*)" };
            IList<T> res = GetAll(template, options);
            return res
        }
        */

        public IList<T> GetAll(T template, params string[] selectColumns)
        {
            GetOptions options = new GetOptions();
            options.SelectFields = selectColumns;
            return GetAll(template, options);
        }

        public IList<T> GetAll(T template, GetOptions options)
        {
            if (options == null) {
                throw new ArgumentNullException("options");
            }

            InitFields();

            List<string> whereColumnNames     = null;
            List<string> whereColumnOperators = null;
            List<object> whereColumnValues    = null;
            if (template != null) {
                whereColumnNames     = new List<string>(f_ColumnToProperties.Count);
                whereColumnOperators = new List<string>(f_ColumnToProperties.Count);
                whereColumnValues    = new List<object>(f_ColumnToProperties.Count);
                foreach (KeyValuePair<string, PropertyInfo> kv in f_ColumnToProperties) {
                    string columnName = kv.Key;
                    PropertyInfo property = kv.Value;

                    string @operator = "=";
                    object value = property.GetValue(template, null);
                    if (value == null) {
                        continue;
                    }

                    if (value is string) {
                        string strValue = (string) value;
                        if (strValue.StartsWith("%") || strValue.EndsWith("%")) {
                            @operator = "LIKE";
                        }
                    }

                    whereColumnNames.Add(columnName);
                    whereColumnOperators.Add(@operator);
                    whereColumnValues.Add(value);
                }
            }

            IList<string> orderByColumns    = null;
            IList<string> orderByDirections = null;
            if (options.OrderBy != null && options.OrderBy.Count > 0) {
                orderByColumns    = new List<string>(options.OrderBy.Count);
                orderByDirections = new List<string>(options.OrderBy.Count);
                foreach (KeyValuePair<string, OrderByDirection> entry in options.OrderBy) {
                    // BUG: resolve property name to column name
                    orderByColumns.Add(entry.Key);
                    string direction;
                    switch (entry.Value) {
                        case OrderByDirection.Ascending:
                            direction = "ASC";
                            break;
                        case OrderByDirection.Descending:
                            direction = "DESC";
                            break;
                        default:
                            throw new NotSupportedException("Unsupported OrderByDirection value: " + entry.Value);
                    }
                    orderByDirections.Add(direction);
                }
            }

            IList<string> selectColumnNames = null;
            if (options.SelectFields != null &&  options.SelectFields.Count > 0) {
                // resolve property name to column name
                selectColumnNames = new List<string>(options.SelectFields.Count);
                foreach (string propertyName in options.SelectFields) {
                    // pass * as is
                    if (propertyName == "*") {
                        selectColumnNames.Add(propertyName);
                        continue;
                    }

                    string columnName;
                    if (!f_PropertyToColumn.TryGetValue(propertyName, out columnName)) {
                        throw new InvalidOperationException("Property for column in SelectFields could not be found: " + propertyName);
                    }
                    selectColumnNames.Add(columnName);
                }
            }

            // only pass offset and/or limit parameter if the RDBMS actually supports it
            int? offset = options.Offset;
            if (!f_DatabaseManager.SqlProvider.HasOffsetSupport) {
                offset = null;
            }
            int? limit = options.Limit;
            if (!f_DatabaseManager.SqlProvider.HasLimitSupport) {
                limit = null;
            }

            // in case the RDMBS only supports limit without offset then we have
            // to adjust the limit value to include the offset section
            if (limit != null &&
                options.Offset != null &&
                !f_DatabaseManager.SqlProvider.HasOffsetSupport) {
                limit += options.Offset;
            }

            using (IDbCommand cmd = f_DatabaseManager.CreateSelectCommand(
                                        f_TableName,
                                        selectColumnNames,
                                        whereColumnNames,
                                        whereColumnOperators,
                                        whereColumnValues,
                                        orderByColumns,
                                        orderByDirections,
                                        limit,
                                        offset)) {
#if LOG4NET
                _Logger.Debug("GetAll(): SQL: " + cmd.CommandText);
                _Logger.Debug("GetAll(): parsed SQL: " + ParseCommandParameters(cmd));
        
                DateTime start, stop;
                start = DateTime.UtcNow;
#endif
                using (IDataReader reader = cmd.ExecuteReader()) {
#if LOG4NET
                    stop = DateTime.UtcNow;
                    _Logger.Debug("GetAll(): query took: " + (stop - start).TotalMilliseconds + " ms");
#endif
                    List<T> rows = new List<T>();
                    if (options.Offset != null && options.Offset > 0 &&
                        !f_DatabaseManager.SqlProvider.HasOffsetSupport) {
#if LOG4NET
                        _Logger.Debug("GetAll(): emulating offset of: " + options.Offset);
#endif
                        // RDBMS doesn't support OFFSET, so emulate it
                        for (int i = 0; i < options.Offset; i++) {
                            reader.Read();
                        }
                    }
                    while (reader.Read()) {
                        T row = CreateDBObject(reader);
                        rows.Add(row);
    
                        if (options.Limit != null &&
                            !f_DatabaseManager.SqlProvider.HasLimitSupport) {
                            // RDBMS doesn't support LIMIT, so emulate it
                            if (rows.Count >= options.Limit) {
    #if LOG4NET
                                _Logger.Debug("GetAll(): emulating limit of: " + options.Limit);
    #endif
                                break;
                            }
                        }
                    }
    
                    return rows;
                }
            }
        }

        public void RemoveSingle(T template)
        {
            if (template == null) {
                throw new ArgumentNullException("template");
            }

            InitFields();

            foreach (string pkColumn in f_PrimaryKeyColumns) {
                PropertyInfo property = f_ColumnToProperties[pkColumn];

                object value = property.GetValue(template, null);
                if (value == null) {
                    throw new MissingPrimaryKeyException("Property: " + property.Name + " must not be null.");
                }
            }

            int count = RemoveAll(template);
            if (count == 0) {
                throw new DataNotFoundException();
            }
            if (count > 1) {
                throw new TooMuchDataRemovedException();
            }
        }

        public int RemoveAll(T template)
        {
            InitFields();

            List<string> whereColumnNames     = null;
            List<string> whereColumnOperators = null;
            List<object> whereColumnValues    = null;
            if (template != null) {
                whereColumnNames     = new List<string>(f_ColumnToProperties.Count);
                whereColumnOperators = new List<string>(f_ColumnToProperties.Count);
                whereColumnValues    = new List<object>(f_ColumnToProperties.Count);
                foreach (KeyValuePair<string, PropertyInfo> kv in f_ColumnToProperties) {
                    string columnName = kv.Key;
                    PropertyInfo property = kv.Value;

                    object value = property.GetValue(template, null);
                    if (value == null) {
                        continue;
                    }

                    whereColumnNames.Add(columnName);
                    whereColumnOperators.Add("=");
                    whereColumnValues.Add(value);
                }
            }

            using (IDbCommand cmd = f_DatabaseManager.CreateDeleteCommand(
                                        f_TableName,
                                        whereColumnNames,
                                        whereColumnOperators,
                                        whereColumnValues)) {
#if LOG4NET
                _Logger.Debug("RemoveAll(): SQL: " + cmd.CommandText);
                _Logger.Debug("RemoveAll(): parsed SQL: " + ParseCommandParameters(cmd));
    
                DateTime start, stop;
                start = DateTime.UtcNow;
#endif
                int res = cmd.ExecuteNonQuery();
#if LOG4NET
                _Logger.Debug("RemoveAll(): affected rows: " + res);
                stop = DateTime.UtcNow;
                _Logger.Debug("RemoveAll(): query took: " + (stop - start).TotalMilliseconds + " ms");
#endif
                return res;
            }
        }
    }
}
