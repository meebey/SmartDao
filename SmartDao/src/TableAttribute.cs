using System;

namespace Meebey.SmartDao
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        private string   _Name;
        private string[] _OldNames;
        
        public string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public string[] OldNames {
            get {
                return _OldNames;
            }
            set {
                _OldNames = value;
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
