using UnityEngine;

namespace UniKart
{
    public abstract class KartInput : MonoBehaviour
    {
        public abstract float GetThrottle();

        public abstract float GetBrake();

        public abstract float GetSteering();

        public abstract bool GetDrift();
    }
}
