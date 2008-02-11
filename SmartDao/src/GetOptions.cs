using System;
using System.Collections.Generic;

namespace Meebey.SmartDao
{
    public class GetOptions
    {
        private IList<string>                         _SelectFields;
        private IDictionary<string, OrderByDirection> _OrderBy;
        private int?                                  _Limit;
        private int?                                  _Offset;
        
        public IList<string> SelectFields {
            get {
                return _SelectFields;
            }
            set {
                _SelectFields = value;
            }
        }

        public IDictionary<string, OrderByDirection> OrderBy {
            get {
                return _OrderBy;
            }
            set {
                _OrderBy = value;
            }
        }
        
        public Nullable<int> Limit {
            get {
                return _Limit;
            }
            set {
                _Limit = value;
            }
        }

        public Nullable<int> Offset {
            get {
                return _Offset;
            }
            set {
                _Offset = value;
            }
        }
    }
}
