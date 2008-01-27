using System;

namespace Meebey.SmartDao
{
    [Table("test_table")]
    [Column(Name = "unused_column", Type = typeof(string))]
    public class DBTest
    {
        private Int32?    _PKInt32;
        private string    _StringColumn;
        private string    _StringColumnNonFixed;
        private Int32?    _Int32Column;
        private Int32?    _Int32ColumnFixed;
        private Decimal?  _DecimalColumnFixed;
        private Single?   _SingleColumn;
        private Double?   _DoubleColumn;
        private DateTime? _DateTimeColumn;
        
        [PrimaryKey]
        [Column(Name = "pk_int32")]
        public Int32? PKInt32 {
            get {
                return _PKInt32;
            }
            set {
                _PKInt32 = value;
            }
        }

        [Column(Name = "string_column",
                OldNames = new string[] { "str_col" },
                Default = "foo",
                Length = 32)]
        public string StringColumn {
            get {
                return _StringColumn;
            }
            set {
                _StringColumn = value;
            }
        }

        [Column(Name = "string_column_nonfixed",
                Length = -1)]
        public string StringColumnNonFixed {
            get {
                return _StringColumnNonFixed;
            }
            set {
                _StringColumnNonFixed = value;
            }
        }
        
        [Column(Name = "int32_column")]
        public Nullable<int> Int32Column {
            get {
                return _Int32Column;
            }
            set {
                _Int32Column = value;
            }
        }

        [Column(Name = "int32_column_fixed",
                Length = 4)]
        public Nullable<int> Int32ColumnFixed {
            get {
                return _Int32ColumnFixed;
            }
            set {
                _Int32ColumnFixed = value;
            }
        }

        [Column(Name = "decimal_column")]
        public Nullable<decimal> DecimalColumnFixed {
            get {
                return _DecimalColumnFixed;
            }
            set {
                _DecimalColumnFixed = value;
            }
        }

        [Column(Name = "single_column")]
        public Nullable<float> SingleColumn {
            get {
                return _SingleColumn;
            }
            set {
                _SingleColumn = value;
            }
        }

        [Column(Name = "double_column")]
        public Nullable<double> DoubleColumn {
            get {
                return _DoubleColumn;
            }
            set {
                _DoubleColumn = value;
            }
        }

        [Column(Name = "datetime_column")]
        public DateTime? DateTimeColumn {
            get {
                return _DateTimeColumn;
            }
            set {
                _DateTimeColumn = value;
            }
        }

        public DBTest()
        {
        }
    }
}
