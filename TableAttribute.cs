using System;

namespace Meebey.SmartDao
{
    public class TableAttribute : Attribute
    {
        private string _Name;
        
        public string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }
        
        public TableAttribute()
        {
        }
        
        public TableAttribute(string name)
        {
            _Name = name;
        }
    }
}
