using UnityEngine;

namespace UniKart.StageFeatures
{
    public class BoostPad : MonoBehaviour
    {
        public float AccelerationMultiplier = 5f;

        public float MaxSpeedMultiplier = 1.5f;

        public float Duration = 2f;

        private void OnTriggerEnter(Collider other)
        {
            DealBoost(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            DealBoost(collision.collider);
        }

        private void DealBoost(Collider other)
        {
            if (other.attachedRigidbody != null
                && other.attachedRigidbody.TryGetComponent<Kart>(out var kart))
            {
                kart.Boost(AccelerationMultiplier, MaxSpeedMultiplier, Duration);
            }
        }
    }
}
