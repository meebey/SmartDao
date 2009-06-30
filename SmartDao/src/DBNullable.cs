using System;

namespace Meebey.SmartDao
{
    public sealed class DBNullable<T> :
                        IEquatable<T>,
                        IEquatable<DBNullable<T>>
                        where T : IEquatable<T>
    {
        private T    f_Value;
        private bool f_IsNull;
        
        public T Value {
            get {
                return f_Value;
            }
        }
        
        public bool IsNull {
            get {
                return f_IsNull;
            }
        }
        
        public DBNullable()
        {
            f_IsNull = true;
        }
        
        public DBNullable(T value)
        {
            f_Value = value;
            if (value == null) { 
                f_IsNull = true;
            }
        }
        
        public static implicit operator DBNullable<T> (T value)
        {
            return new DBNullable<T>(value);
        }
        
        public static explicit operator T (DBNullable<T> value)
        {
            // TODO: should we return default(T) or throw exception here?
            if ((object) value == null || value.IsNull) {
                throw new InvalidOperationException();
            }
            
            return value.Value;
        }
        
        public override int GetHashCode()
        {
            if (f_IsNull) {
                return default(T).GetHashCode();
            }
            
            return f_Value.GetHashCode();
        }
        
        public bool Equals(DBNullable<T> value)
        {
            if (value == null) {
                return false;
            }
            
            if (f_IsNull && value.IsNull) {
                return  true;
            }
            if (f_IsNull || value.IsNull) {
                return  false;
            }
            
            return f_Value.Equals(value.Value);
        }
        
        public bool Equals(T value)
        {
            if (value == null) {
                return false;
            }
            
            return Equals((DBNullable<T>) value);
        }
        
        public override bool Equals(object o)
        {
            if (o is DBNullable<T>) {
                return Equals((DBNullable<T>) o);
            }
            
            if (o is T) {
                return Equals((T) o);
            }
            
            return false;
        }
        
        public static bool operator == (DBNullable<T> x, DBNullable<T> y)
        {
            if ((object) x == null && (object) y == null) {
                return true;
            }
            if ((object) x == null || (object) y == null) {
                return false;
            }
            
            return x.Equals(y);
        }
        
        public static bool operator != (DBNullable<T> x, DBNullable<T> y)
        {
            return !(x == y);
        }
    }
}
