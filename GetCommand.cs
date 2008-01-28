using System;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class GetCommand
    {
        private string        _TableName;
        private IList<string> _SelectColumns;
        private IList<string> _OrderBy;
        private int?          _Limit;
        private int?          _Offset;
    }
}
