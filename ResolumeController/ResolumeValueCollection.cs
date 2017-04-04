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
    }
}
