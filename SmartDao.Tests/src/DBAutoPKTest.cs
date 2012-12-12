
using System;

namespace Meebey.SmartDao.Tests
{
    [Table("pk_test_table")]
    public class DBAutoPKTest
    {
        private Int32?    f_PKInt32;
        private string    f_StringColumn;

        [PrimaryKey]
        [Sequence]
        [Column(Name = "pk_int32")]
        public Int32? PKInt32 {
            get {
                return f_PKInt32;
            }
            set {
                f_PKInt32 = value;
            }
        }

        [Column(Name = "string_column",
                Length = 32)]
        public string StringColumn {
            get {
                return f_StringColumn;
            }
            set {
                f_StringColumn = value;
            }
        }
    }
}
