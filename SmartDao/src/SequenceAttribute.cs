using System;

namespace Meebey.SmartDao
{
    public class SequenceAttribute : Attribute
    {
        public int? Seed { get; set; }
        public int? Increment { get; set; }

        public SequenceAttribute()
        {
        }
    }
}
