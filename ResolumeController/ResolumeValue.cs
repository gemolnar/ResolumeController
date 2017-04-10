using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
    public abstract class ResolumeValue
    {
        protected ulong _updateCount;
        public ulong UpdateCount { get { return _updateCount; } }
        public DateTime TimeStamp { get; protected set; } = DateTime.Now;
        public TimeSpan Age => DateTime.Now.Subtract(TimeStamp);
        public abstract object LooselyTypedValue { get; }
    }
}
