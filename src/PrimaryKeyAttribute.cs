using System;

namespace Meebey.SmartDao
{
    public class PrimaryKeyAttribute : Attribute
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
        
        public PrimaryKeyAttribute()
        {
        }
        
        public PrimaryKeyAttribute(string name)
        {
            _Name = name;
        }
    }
}
