using System.Collections;
using UnityEngine;

namespace UniKart
{
    public class CustomInterporation : MonoBehaviour
    {
        private Transform _transform;
        private Rigidbody _rigidbody;

        private Vector3 _lastRigPosition;
        private Quaternion _lastRigRotation;

        private void OnEnable()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            _lastRigPosition = _rigidbody.position;
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
            var position = Vector3.LerpUnclamped(_lastRigPosition, _rigidbody.position, t);
            var rotation = Quaternion.LerpUnclamped(_lastRigRotation, _rigidbody.rotation, t);
            _transform.position = position;
            _transform.rotation = rotation;

            yield return new WaitForEndOfFrame();

            _transform.position = _rigidbody.position;
            _transform.rotation = _rigidbody.rotation;
        }
    }
}
