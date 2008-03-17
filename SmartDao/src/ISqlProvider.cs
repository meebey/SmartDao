using System;
using System.Data;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public interface ISqlProvider
    {
        bool HasLimitSupport {
            get;
        }
        bool HasOffsetSupport {
            get;
        }

        string GetDataTypeName(DbType dbType);
        DbType GetDBType(Type type);
        string GetColumnName(string columnName);
        string GetTableName(string tableName);
        string GetTableName(string shemaName, string tableName);
        string GetStatementSeparator();
        string GetParameterCharacter();

        string CreateTableExistsStatement(string tableName);
        string CreateCreateTableStatement(string tableName,
                                          IList<string> columnNames,
                                          IList<Type> columnTypes,
                                          IList<int> columnLengths,
                                          IList<bool?> columnIsNullables,
                                          IList<string> primaryKeys);
        string CreateDropTableStatement(string tableName);
        
        string CreateInsertStatement(string tableName,
                                     IList<string> columnNames,
                                     IList<string> columnValues);
        string CreateSelectStatement(string schemaName, string tableName,
                                     IList<string> selectColumnNames,
                                     string whereClause,
                                     IList<string> orderByColumnNames,
                                     IList<string> orderByColumnOrders,
                                     int? limit,
                                     int? offset);
        string CreateSelectStatement(string schemaName, string tableName,
                                     IList<string> selectColumnNames,
                                     IList<string> whereColumnNames,
                                     IList<string> whereColumnOperators,
                                     IList<string> whereColumnValues,
                                     IList<string> orderByColumnNames,
                                     IList<string> orderByColumnOrders,
                                     int? limit,
                                     int? offset);
        string CreateUpdateStatement(string tableName,
                                     IList<string> setColumnNames,
                                     IList<string> setColumnValues,
                                     string whereClause);
        string CreateUpdateStatement(string tableName,
                                     IList<string> setColumnNames,
                                     IList<string> setColumnValues,
                                     IList<string> whereColumnNames,
                                     IList<string> whereColumnOperators,
                                     IList<string> whereColumnValues);
        string CreateDeleteStatement(string tableName,
                                     string whereClause);
        string CreateDeleteStatement(string tableName,
                                     IList<string> whereColumnNames,
                                     IList<string> whereColumnOperators,
                                     IList<string> whereColumnValues);
    }
}
