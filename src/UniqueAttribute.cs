using System;

namespace Meebey.SmartDao
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
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
        
        public UniqueAttribute()
        {
        }
        
        public UniqueAttribute(string name)
        {
            _Name = name;
        }
    }
}
