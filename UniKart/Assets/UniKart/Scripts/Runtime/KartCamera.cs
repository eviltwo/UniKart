using UnityEngine;

namespace UniKart
{
    public class KartCamera : MonoBehaviour
    {
        public Kart Kart;

        public Vector3 OffsetPosition = new Vector3(0f, 2f, -5f);

        public Vector3 OffsetRotation = new Vector3(0f, 0f, 0f);

        public float TiltDamper = 1f;

        public float TiltRatio = 0.5f;

        public float TiltRatioInAir = 0.25f;

        private Vector3 _pivot;

        private float _currentTiltRatio = 1f;

        private float _driftSwitchRatio;

        private void Start()
        {
            _pivot = Kart.transform.transform.position - Kart.transform.forward * TiltDamper;
        }

        private void LateUpdate()
        {
            var groundNormal = Vector3.up;

            _pivot = Kart.transform.position + (_pivot - Kart.transform.position).normalized * TiltDamper;

            // Horizontal rotation
            var kartForward = (Kart.transform.position - _pivot).normalized;
            var driftForward = Vector3.Lerp(Kart.transform.forward, kartForward, 0.5f);
            var targetRatio = (Kart.IsDrifting || !Kart.IsGrounded) ? 1 : 0;
            _driftSwitchRatio = Mathf.Lerp(_driftSwitchRatio, targetRatio, Time.deltaTime * 1f);
            kartForward = Vector3.Slerp(kartForward, driftForward, _driftSwitchRatio);

            var kartForwardHorizontal = kartForward - Vector3.Project(kartForward, groundNormal);
            var rot = Quaternion.LookRotation(kartForwardHorizontal);

            // Tilt rotation
            var pivotForward = Kart.transform.position - _pivot;
            var pivotForwardHorizontal = pivotForward - Vector3.Project(pivotForward, groundNormal);
            var fullTiltRot = Quaternion.LookRotation(new Vector3(0, Vector3.Dot(pivotForward, groundNormal), pivotForwardHorizontal.magnitude));
            _currentTiltRatio = Mathf.MoveTowards(_currentTiltRatio, Kart.IsGrounded ? TiltRatio : TiltRatioInAir, Time.deltaTime);
            rot = rot * Quaternion.Lerp(Quaternion.identity, fullTiltRot, _currentTiltRatio);

            // Apply
            transform.rotation = rot * Quaternion.Euler(OffsetRotation);
            transform.position = Kart.transform.position + rot * OffsetPosition;
        }
    }
}
