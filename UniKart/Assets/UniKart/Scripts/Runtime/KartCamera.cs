using UnityEngine;

namespace UniKart
{
    public class KartCamera : MonoBehaviour
    {
        public Kart Kart;

        public Vector3 OffsetPosition = new Vector3(0f, 2f, -5f);

        public Vector3 OffsetRotation = new Vector3(0f, 0f, 0f);

        public float ChaseDistance = 1f;

        [Range(0, 1)]
        public float KartFitRatio = 0.5f;

        public float TiltAngleSmoothing = 1;

        private Vector3 _chacingPivot;

        private Vector3 _smoothingGroundNormal;

        private void Start()
        {
            _chacingPivot = Kart.transform.transform.position - Kart.transform.forward * ChaseDistance;
        }

        private void LateUpdate()
        {
            var worldUpward = Vector3.up;

            _chacingPivot = Kart.transform.position + (_chacingPivot - Kart.transform.position).normalized * ChaseDistance;

            // Horizontal rotation
            var moveForward = (Kart.transform.position - _chacingPivot).normalized;
            var mergedForward = Vector3.Lerp(moveForward, Kart.transform.forward, KartFitRatio);
            var mergedForwardHorizontal = mergedForward - Vector3.Project(mergedForward, worldUpward);
            var rot = Quaternion.LookRotation(mergedForwardHorizontal);

            // Tilt rotation
            var moveUpward = Vector3.Cross(moveForward, Vector3.Cross(worldUpward, moveForward));
            var targetGroundNormal = Kart.IsGrounded ? Kart.GroundNormal : moveUpward;
            _smoothingGroundNormal = Vector3.Slerp(_smoothingGroundNormal, targetGroundNormal, TiltAngleSmoothing * Time.deltaTime);
            var groundForward = Vector3.Cross(rot * Vector3.right, _smoothingGroundNormal);
            var groundForwardHorizontal = groundForward - Vector3.Project(groundForward, worldUpward);
            var tiltRot = Quaternion.LookRotation(new Vector3(0, Vector3.Dot(groundForward, worldUpward), groundForwardHorizontal.magnitude));
            rot = rot * tiltRot;

            // Apply
            transform.rotation = rot * Quaternion.Euler(OffsetRotation);
            transform.position = Kart.transform.position + rot * OffsetPosition;
        }
    }
}
