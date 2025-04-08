using UnityEngine;

namespace UniKart
{
    public class KartCamera : MonoBehaviour
    {
        public Transform Target;

        public Vector3 OffsetPosition = new Vector3(0f, 2f, -5f);

        public Vector3 OffsetRotation = new Vector3(0f, 0f, 0f);

        public float ChaseDistance = 1f;

        public float TiltRatio = 0.5f;

        private Vector3 _pivot;

        private Quaternion _targetTiltRot = Quaternion.identity;

        private Quaternion _tiltRot = Quaternion.identity;

        private void Start()
        {
            _pivot = Target.position - Target.forward * ChaseDistance;
        }

        private void LateUpdate()
        {
            var groundNormal = Vector3.up;

            _pivot = Target.position + (_pivot - Target.position).normalized * ChaseDistance;

            // Horizontal rotation
            var camToKart = Target.position - _pivot;
            var horizontalCamToKart = camToKart - Vector3.Project(camToKart, groundNormal);
            var rot = Quaternion.LookRotation(horizontalCamToKart);

            // Tilt rotation
            _targetTiltRot = Quaternion.LookRotation(new Vector3(0, Vector3.Dot(camToKart, groundNormal), horizontalCamToKart.magnitude));
            _targetTiltRot = Quaternion.Lerp(Quaternion.identity, _targetTiltRot, TiltRatio);

            var diffAngle = Quaternion.Angle(_tiltRot, _targetTiltRot);
            _tiltRot = Quaternion.RotateTowards(_tiltRot, _targetTiltRot, diffAngle * 0.1f);
            rot = rot * _tiltRot;

            transform.rotation = rot * Quaternion.Euler(OffsetRotation);
            transform.position = Target.position + rot * OffsetPosition;
        }
    }
}
