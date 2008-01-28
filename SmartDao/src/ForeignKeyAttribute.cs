using System;

namespace Meebey.SmartDao
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute : Attribute
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
        
        public ForeignKeyAttribute()
        {
        }
        
        public ForeignKeyAttribute(string name)
        {
            _Name = name;
        }
    }
}
