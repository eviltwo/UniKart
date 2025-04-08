using UnityEngine;

namespace UniKart
{
    public class KartCamera : MonoBehaviour
    {
        public Kart Kart;

        public Vector3 OffsetPosition = new Vector3(0f, 2f, -5f);

        public Vector3 OffsetRotation = new Vector3(0f, 0f, 0f);

        public float ChaseDistance = 1f;

        public float TiltRatio = 0.5f;

        public float TiltAngleSpeed = 0.1f;

        private Vector3 _pivot;

        private Quaternion _targetTiltRot = Quaternion.identity;

        private Quaternion _tiltRot = Quaternion.identity;

        private float _currentTiltRatio = 1f;

        private void Start()
        {
            _pivot = Kart.transform.transform.position - Kart.transform.forward * ChaseDistance;
        }

        private void LateUpdate()
        {
            var groundNormal = Vector3.up;

            _pivot = Kart.transform.position + (_pivot - Kart.transform.position).normalized * ChaseDistance;

            // Horizontal rotation
            var kartForward = Kart.transform.forward;
            var kartForwardHorizontal = kartForward - Vector3.Project(kartForward, groundNormal);
            var rot = Quaternion.LookRotation(kartForwardHorizontal);

            // Tilt rotation
            var pivotForward = Kart.transform.position - _pivot;
            var pivotForwardHorizontal = pivotForward - Vector3.Project(pivotForward, groundNormal);
            _targetTiltRot = Quaternion.LookRotation(new Vector3(0, Vector3.Dot(pivotForward, groundNormal), pivotForwardHorizontal.magnitude));
            _currentTiltRatio = Mathf.MoveTowards(_currentTiltRatio, Kart.IsGrounded ? TiltRatio : TiltRatio * 0.5f, Time.deltaTime);
            _targetTiltRot = Quaternion.Lerp(Quaternion.identity, _targetTiltRot, _currentTiltRatio);

            var diffAngle = Quaternion.Angle(_tiltRot, _targetTiltRot);
            _tiltRot = Quaternion.RotateTowards(_tiltRot, _targetTiltRot, diffAngle * TiltAngleSpeed);
            rot = rot * _tiltRot;

            // Apply
            transform.rotation = rot * Quaternion.Euler(OffsetRotation);
            transform.position = Kart.transform.position + rot * OffsetPosition;
        }
    }
}
