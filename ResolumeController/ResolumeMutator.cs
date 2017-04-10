using System;

namespace ResolumeController
{
    public abstract class ResolumeMutator
    {
        public DateTime Timestamp { get; private set; }
        public ResolumeMutator()
        {
            Timestamp = DateTime.Now;
        }
        public abstract bool TryGetValue(out object v);
    }
}
