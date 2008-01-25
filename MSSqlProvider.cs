using System;
using System.Data;

namespace Meebey.SmartDao
{
    public class MSSqlProvider : AnsiSqlProvider
    {
        public MSSqlProvider()
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
    }
}
