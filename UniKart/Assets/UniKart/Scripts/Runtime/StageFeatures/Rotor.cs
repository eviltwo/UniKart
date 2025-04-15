using UnityEngine;

namespace UniKart.StageFeatures
{
    [RequireComponent(typeof(Rigidbody))]
    public class Rotor : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;

        public float AngleSpeed = 90f;

        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        private void FixedUpdate()
        {
            Quaternion rotation = Quaternion.AngleAxis(AngleSpeed * Time.fixedDeltaTime, Axis);
            _rigidbody.MoveRotation(_rigidbody.rotation * rotation);
        }
    }
}
