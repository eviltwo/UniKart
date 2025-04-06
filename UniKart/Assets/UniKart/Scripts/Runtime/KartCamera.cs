using UnityEngine;

namespace UniKart
{
    public class KartCamera : MonoBehaviour
    {
        public Transform Target;

        public Vector3 OffsetPosition = new Vector3(0f, 2f, -5f);

        public Vector3 OffsetRotation = new Vector3(0f, 0f, 0f);

        public float TiltRatio = 0.5f;

        private void LateUpdate()
        {
            var horizontalRotation = Quaternion.FromToRotation(Target.up, Vector3.up) * Target.rotation;
            var mergedRot = Quaternion.Slerp(horizontalRotation, Target.rotation, TiltRatio);
            transform.rotation = mergedRot * Quaternion.Euler(OffsetRotation);

            transform.position = Target.position + mergedRot * OffsetPosition;
        }
    }
}
