using System;

namespace ResolumeController
{
    public class EnvelopeMutator : ResolumeMutator<Single>
    {
        float _lengthInMillis;
        public EnvelopeMutator(float initialValue, float lengthInMillis) : base(initialValue)
        {
            _lengthInMillis = lengthInMillis;
        }

        public override bool TryGetValue(out float v)
        {
            TimeSpan age = DateTime.Now.Subtract(Timestamp);
            if (age.TotalMilliseconds > _lengthInMillis)
            {
                v = 0;
                return false;
            }
            //Math.Sin(age.TotalMilliseconds
            //float result = 0;
            float result = (float)(
                InitalValue - (InitalValue * (age.TotalMilliseconds / _lengthInMillis))
                );
            v = result;
            return true;
        }


    }
}
