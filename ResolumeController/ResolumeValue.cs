using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolumeController
{
    public abstract class ResolumeValue
    {
        public DateTime TimeStamp { get; protected set; } = DateTime.Now;
        public TimeSpan Age => DateTime.Now.Subtract(TimeStamp);
        public abstract TCast GetValueAS<TCast>() where TCast : class;
    }
}
