using System;

namespace Meebey.SmartDao
{
    public class PrimaryKeyAttribute : Attribute
    {
        public string Name { get; set; }

        public PrimaryKeyAttribute()
        {
        }

        public PrimaryKeyAttribute(string name)
        {
            Name = name;
        }
    }
}
