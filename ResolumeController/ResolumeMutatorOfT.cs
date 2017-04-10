namespace ResolumeController
{
    public abstract class ResolumeMutator<T> : ResolumeMutator
    {    
        public T InitalValue { get; private set; }
        public ResolumeMutator(T initialValue)
        {
            InitalValue = initialValue;
        }

        public abstract bool TryGetValue(out T v);
        public override bool TryGetValue(out object v)
        {
            T returnValue;
            bool success = TryGetValue(out returnValue);
            v = returnValue;
            return success;
        }
    }
}
