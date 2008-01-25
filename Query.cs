using System;
using System.Data;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class Query<T>
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        private Type            _TableType;
        private string          _TableName;
        private IList<KeyValuePair<ColumnAttribute, FieldInfo>> _Fields;
        private DatabaseManager _DatabaseManager;
        
        public Query(DatabaseManager dbManager)
        {
            _DatabaseManager = dbManager;
            
            _TableType = typeof(T);
            object[] attrs = _TableType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attrs == null || attrs.Length == 0) {
                throw new ArgumentException("T", "Type does not contain a TableAttribute.");
            }
            
            TableAttribute attr = (TableAttribute) attrs[0];
            _TableName = attr.Name;
        }
        
        private void InitFields()
        {
            if (_Fields != null) {
                return;
            }
            
#if LOG4NET
            _Logger.Debug("InitFields(): initializing fields...");
            DateTime start = DateTime.UtcNow;
#endif
            FieldInfo[] fields = _TableType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            _Fields = new List<KeyValuePair<ColumnAttribute, FieldInfo>>(fields.Length);
            bool foundColumn = false;
            foreach (FieldInfo field in fields) {
                object[] columnAttrs = field.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrs == null || columnAttrs.Length == 0) {
                    continue;
                }
                foundColumn = true;
                
                ColumnAttribute columnAttr = (ColumnAttribute) columnAttrs [0];
                _Fields.Add(new KeyValuePair<ColumnAttribute, FieldInfo>(columnAttr, field));
            }
            if (!foundColumn) {
                throw new ArgumentException("Type does not contain any ColumnAttribute.", "_TableType");
            }
#if LOG4NET
            _Logger.Debug("InitFields(): took: " + (DateTime.UtcNow - start).TotalMilliseconds + " ms");
#endif
        }
        
        public IList<T> Add(IList<T> entry)
        {
            // TODO: implement multi-inserts here
            return null;
        }
        
        public T Add(T entry)
        {
            InitFields();
            
            List<string> columnNames = new List<string>(_Fields.Count);
            List<object> columnValues = new List<object>(_Fields.Count);
            foreach (KeyValuePair<ColumnAttribute, FieldInfo> pair in _Fields) {
                ColumnAttribute column = pair.Key;
                FieldInfo field = pair.Value;
                
                object value = field.GetValue(entry);
                if (value == null) {
                    continue;
                }
                columnNames.Add(column.Name);
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
        
        public IList<T> GetAll(T template, params string[] columns)
        {
            // TODO: implement me
            return null;
        }
    }
}
