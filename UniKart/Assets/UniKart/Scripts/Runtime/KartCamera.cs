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

        private void Start()
        {
            _pivot = Target.position - Target.forward * ChaseDistance;
        }

        private void LateUpdate()
        {
            var groundNormal = Vector3.up;
            var lastCameraPosition = transform.position;

            _pivot = Target.position + (_pivot - Target.position).normalized * ChaseDistance;

            // Horizontal rotation
            var camToKart = Target.position - _pivot;
            var horizontalCamToKart = camToKart - Vector3.Project(camToKart, groundNormal);
            var rot = Quaternion.LookRotation(horizontalCamToKart);

            // Tilt rotation
            var tiltRot = Quaternion.LookRotation(new Vector3(0, Vector3.Dot(camToKart, groundNormal), horizontalCamToKart.magnitude));
            rot = Quaternion.Slerp(rot, rot * tiltRot, TiltRatio);

            transform.rotation = rot * Quaternion.Euler(OffsetRotation);
            transform.position = Target.position + rot * OffsetPosition;
        }
    }
}
