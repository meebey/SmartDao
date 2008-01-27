using System;

namespace Meebey.SmartDao
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IndexAttribute : Attribute
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
        
        public IndexAttribute()
        {
        }
        
        public IndexAttribute(string name)
        {
            _Name = name;
        }
    }
}
