using System.Collections;
using UnityEngine;

namespace UniKart
{
    public class CustomInterporation : MonoBehaviour
    {
        private Transform _transform;
        private Rigidbody _rigidbody;

        private Vector3 _lastRigPosition;
        private Vector3 _lastRigVelocity;
        private Quaternion _lastRigRotation;

        private void OnEnable()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _lastRigPosition = _rigidbody.position;
            _lastRigVelocity = _rigidbody.linearVelocity;
            _lastRigRotation = _rigidbody.rotation;
        }

        private void Update()
        {
            StartCoroutine(Interporate());
        }

        private IEnumerator Interporate()
        {
            var deltaTime = Time.time - Time.fixedTime;
            var t = deltaTime / Time.fixedDeltaTime;
            var a = (_rigidbody.linearVelocity - _lastRigVelocity) / Time.fixedDeltaTime;
            var position = _lastRigPosition + _lastRigVelocity * deltaTime + 0.5f * a * deltaTime * deltaTime;
            var rotation = Quaternion.SlerpUnclamped(_lastRigRotation, _rigidbody.rotation, t);
            _transform.position = position;
            _transform.rotation = rotation;

            yield return new WaitForEndOfFrame();

            _transform.position = _rigidbody.position;
            _transform.rotation = _rigidbody.rotation;
        }
    }
}
