using System;
using System.Data;

namespace Meebey.SmartDao
{
    public class MicrosoftSqlProvider : AnsiSqlProvider
    {
        public override bool HasLimitSupport {
            get {
                return true;
            }
        }
        
        public override bool HasOffsetSupport {
            get {
                return false;
            }
        }
        
        public MicrosoftSqlProvider()
        {
        }
        
        public override string GetDataTypeName(DbType dbType)
        {
            switch (dbType) {
                case DbType.DateTime:
                    return "DATETIME";
            }
            
            return base.GetDataTypeName(dbType);
        }
        
        // Microsoft SQL supports ANSI quotes correctly, didn't expect that one
        /*
        public override string GetTableName(string tableName)
        {
            return String.Format("[{0}]", tableName);
        }

        public override string GetColumName(string columName)
        {
            return String.Format("[{0}]", columName);
        }
        */
    }
}
