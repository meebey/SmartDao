using System;

namespace Meebey.SmartDao
{
    [Table("test_table")]
    public class DBTest
    {
        [Column(Name = "string_column",
                OldNames = new string[] { "str_col" },
                Default = "foo",
                Length = 32)]
        private string _StringColumn;
        
        [Column(Name = "string_column_nonfixed",
                Length = -1)]
        private string _StringColumnNonFixed;
        
        [Column(Name = "int32_column")]
        private Int32? _Int32Column;
        
        [Column(Name = "int32_column_fixed",
                Length = 4)]
        private Int32? _Int32ColumnFixed;
        
        [Column(Name = "decimal_column")]
        private Decimal? _DecimalColumnFixed;
        
        [Column(Name = "single_column")]
        private Single? _SingleColumn;

        [Column(Name = "double_column")]
        private Double? _DoubleColumn;
        
        [Column(Name = "datetime_column")]
        private DateTime? _DateTimeColumn;
        
        [PrimaryKey]
        [Column(Name = "pk_int32")]
        private Int32? _PKInt32;
        
        public string StringColumn {
            get {
                return _StringColumn;
            }
            set {
                _StringColumn = value;
            }
        }

        public DateTime? DateTimeColumn {
            get {
                return _DateTimeColumn;
            }
            set {
                _DateTimeColumn = value;
            }
        }

        public Int32? PKInt32 {
            get {
                return _PKInt32;
            }
            set {
                _PKInt32 = value;
            }
        }
        
        public DBTest()
        {
        }
    }
}
