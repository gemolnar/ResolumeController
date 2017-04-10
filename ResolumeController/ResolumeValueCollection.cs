using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
    public class ResolumeValueCollection : ConcurrentDictionary<string, ResolumeValue>
    {
        public T GetValueAs<T>(string key)
            where T : ResolumeValue
        {
            return (T)this[key];
        }

        public T GetValueOrDefaultAs<T>(string key)
            where T : ResolumeValue
        {
            ResolumeValue rv;
            if (TryGetValue(key, out rv))
            {
                try
                {
                    return (T)rv;
                }
                catch (InvalidCastException)
                {
                    throw new InvalidOperationException($"The {nameof(GetValueOrDefaultAs)}<{typeof(T).Name}>() method cannot be used on a(n) {rv.GetType().Name} value.");
                }
            }
            else
            {
                return null;
            }
        }
    }
}
