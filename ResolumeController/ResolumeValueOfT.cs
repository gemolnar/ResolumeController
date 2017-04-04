using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
    public sealed class ResolumeValue<T> : ResolumeValue
    {
        private T _value;
        public T Value {
            get {
                return _value;
            }
            set {
                TimeStamp = DateTime.Now;
                _value = value;
            }
        }

        public ResolumeValue(T value)
        {
            Value = value;
        }

        public override TCast GetValueAS<TCast>()
        {
            return (TCast)(object)Value;
        }

        public override void SetValueAS<TCast>(TCast value)
        {
            _value = (T)(object)value;
        }

        public override string ToString()
        {
            return $"{Value} [@{TimeStamp.Ticks}, {typeof(T).Name}]";
        }

    }
}
