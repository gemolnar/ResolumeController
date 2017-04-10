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
        public override object LooselyTypedValue => _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                var sinceLastUpdate = DateTime.Now.Subtract(TimeStamp);
                TimeStamp = DateTime.Now;
                _updateCount++;
                _value = value;
            }
        }


        public ResolumeValue(T value)
        {
            Value = value;
        } 

        public override string ToString()
        {
            return $"[@{TimeStamp.ToString("mm:ss.fff")}] ({typeof(T).Name}){Value}";
        }

    }
}
