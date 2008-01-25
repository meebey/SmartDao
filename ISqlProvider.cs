using System;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public interface ISqlProvider
    {
        string GetCreateTableStatement(string tableName, IList<string> columnNames, IList<Type> columnTypes, IList<int> columnLengths, IList<string> primaryKeys);
        string GetDropTableStatement(string tableName);
        string GetInsertStatement(string tableName, IList<string> columnNames);
        string GetSelectStatement(string tableName, IList<string> columnNames, string whereClause);
        string GetDeleteStatement(string tableName, string whereClause);
        string GetDataTypeName(DbType dbType);
        string GetColumnName(string columnName);
        string GetTableName(string tableName);
        string GetStatementSeparator();
    }
}
