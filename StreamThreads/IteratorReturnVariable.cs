using System;

namespace StreamThreads
{
    public class IteratorReturnVariable<T>
    {
        private T? _value;
        private bool _hasvalue = false;

        public T? Value
        {
            get
            {
                if (!_hasvalue)
                    throw new InvalidOperationException();

                return _value;
            } 
            
            set
            {
                _value = value;
                _hasvalue = true;
            }
        }

        public bool HasValue()
        {
            return _hasvalue;
        }

        public static implicit operator T(IteratorReturnVariable<T> v)
        {
            return v.Value!;
        }

        public override string ToString()
        {
            return $"{_value}";
        }
    }
}