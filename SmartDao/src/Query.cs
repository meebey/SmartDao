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
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        private DatabaseManager _DatabaseManager;
        private Type            _TableType;
        private string          _TableName;
        private IDictionary<string, PropertyInfo> _ColumnToProperties;
        private IList<string>                     _PrimaryKeyColumns;
        
        public Query(DatabaseManager dbManager)
        {
            if (dbManager == null) {
                throw new ArgumentNullException("dbManager");
            }
            
            _DatabaseManager = dbManager;
            _TableType = typeof(T);
        }
        
        private void InitFields()
        {
            if (_ColumnToProperties != null) {
                return;
            }
            
            object[] tableAttrs = _TableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttrs == null || tableAttrs.Length == 0) {
                throw new ArgumentException("T", "Type does not contain a TableAttribute.");
            }
            
            TableAttribute tableAttr = (TableAttribute) tableAttrs[0];
            _TableName = tableAttr.Name;
            
            PropertyInfo[] properties = _TableType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            _ColumnToProperties = new Dictionary<string, PropertyInfo>(properties.Length);
            _PrimaryKeyColumns = new List<string>();
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
                _ColumnToProperties.Add(columnName, property);
                
                object[] pkAttrs = property.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
                if (pkAttrs != null && pkAttrs.Length > 0) {
                    _PrimaryKeyColumns.Add(columnName);
                }
            }
            if (!foundColumn) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "_TableType");
            }
        }
        
        public IList<T> Add(IList<T> entry)
        {
            // TODO: implement multi-inserts here
            return null;
        }
        
        public T Add(T entry)
        {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }
            
            InitFields();
            
            List<string> columnNames = new List<string>(_ColumnToProperties.Count);
            List<object> columnValues = new List<object>(_ColumnToProperties.Count);
            foreach (KeyValuePair<string, PropertyInfo> kv in _ColumnToProperties) {
                string columnName = kv.Key;
                PropertyInfo property = kv.Value;
                
                object value = property.GetValue(entry, null);
                if (value == null) {
                    continue;
                }
                columnNames.Add(columnName);
                columnValues.Add(value);
            }
            
            IDbCommand cmd = _DatabaseManager.CreateInsertCommand(_TableName, columnNames, columnValues);
#if LOG4NET
            _Logger.Debug("Add(): SQL: " + cmd.CommandText);
#endif
            cmd.ExecuteNonQuery();
            
            // TODO: copy pk into a fresh object
            return entry;
        }
        
        public int SetAll(T entry)
        {
            if (entry == null) {
                throw new ArgumentNullException("entry");
            }
            
            InitFields();
            
            List<string> setColumnNames       = new List<string>();
            List<object> setColumnValues      = new List<object>();
            List<string> whereColumnNames     = new List<string>(_PrimaryKeyColumns.Count);
            List<string> whereColumnOperators = new List<string>(_PrimaryKeyColumns.Count);
            List<object> whereColumnValues    = new List<object>(_PrimaryKeyColumns.Count);
            foreach (KeyValuePair<string, PropertyInfo> kv in _ColumnToProperties) {
                string columnName = kv.Key;
                PropertyInfo property = kv.Value;

                object value = property.GetValue(entry, null);
                if (value == null) {
                    continue;
                }
                
                if (_PrimaryKeyColumns.Contains(columnName)) {
                    whereColumnNames.Add(columnName);
                    whereColumnOperators.Add("=");
                    whereColumnValues.Add(value);
                } else {
                    setColumnNames.Add(columnName);
                    setColumnValues.Add(value);
                }
            }
            
            IDbCommand cmd = _DatabaseManager.CreateUpdateCommand(_TableName, setColumnNames, setColumnValues, whereColumnNames, whereColumnOperators, whereColumnValues);
#if LOG4NET
            _Logger.Debug("Set(): SQL: " + cmd.CommandText);
#endif
            int res = cmd.ExecuteNonQuery();
#if LOG4NET
            _Logger.Debug("Set(): affected rows: " + res);
#endif
            return res;
        }
        
        public IList<T> GetAll(T template, params string[] selectColumns)
        {
            if (selectColumns == null) {
                throw new ArgumentNullException("selectColumns");
            }
            
            InitFields();

            List<string> columnNames     = new List<string>(_ColumnToProperties.Count);
            List<string> columnOperators = new List<string>(_ColumnToProperties.Count);
            List<object> columnValues    = new List<object>(_ColumnToProperties.Count);
            if (template != null) {
                foreach (KeyValuePair<string, PropertyInfo> kv in _ColumnToProperties) {
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
                    
                    columnNames.Add(columnName);
                    columnOperators.Add(@operator);
                    columnValues.Add(value);
                }
            }
            
            IDbCommand cmd = _DatabaseManager.CreateSelectCommand(_TableName,
                                                                  selectColumns,
                                                                  columnNames,
                                                                  columnOperators,
                                                                  columnValues,
                                                                  null,
                                                                  null,
                                                                  null,
                                                                  null);
#if LOG4NET
            _Logger.Debug("GetAll(): SQL: " + cmd.CommandText);
#endif
            using (IDataReader reader = cmd.ExecuteReader()) {
                List<T> rows = new List<T>();
                while (reader.Read()) {
                    T row = new T();
                    rows.Add(row);
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (reader.IsDBNull(i)) {
                            continue;
                        }
                        
                        string name = reader.GetName(i);
                        object value = reader.GetValue(i);
                        
                        PropertyInfo property = _ColumnToProperties[name];
                        if (property == null) {
                            throw new InvalidOperationException("Field for column could not be found: " + name);
                        }
                        if (!property.PropertyType.IsAssignableFrom(value.GetType())) {
                            throw new InvalidOperationException(
                                        String.Format("Field type: {0} of {1} doesn't match column type: {2} for column: {3}",
                                                      property.PropertyType, row.GetType() , value.GetType(), name));
                        }
                        
                        property.SetValue(row, value, null);
                    }
                }
                
                return rows;
            }                        
        }
    }
}
