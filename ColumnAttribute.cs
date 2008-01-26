using System;

namespace Meebey.SmartDao
{
    public class ColumnAttribute : Attribute
    {
        private string   _Name;
        private string   _Default;
        private string[] _OldNames;
        private int      _Length;
        private bool     _IsNullable;
        
        public string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public string Default {
            get {
                return _Default;
            }
            set {
                _Default = value;
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

        public int Length {
            get {
                return _Length;
            }
            set {
                _Length = value;
            }
        }
        
        public bool IsNullable {
            get {
                return _IsNullable;
            }
            set {
                _IsNullable = value;
            }
        }
        
        public ColumnAttribute()
        {
        }
        
        public ColumnAttribute(string name, string[] oldNames, string @default, int length)
        {
            _Name = name;
            _OldNames = oldNames;
            _Default = @default;
            _Length = length;
        }
    }
}
