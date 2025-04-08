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
            var groundNormal = Vector3.up;
            var lastCameraPosition = transform.position;

            var camToKart = Target.position - lastCameraPosition;
            camToKart.y = 0f;
            var horizontalRot = Quaternion.LookRotation(camToKart);

            transform.rotation = horizontalRot * Quaternion.Euler(OffsetRotation);
            transform.position = Target.position + horizontalRot * OffsetPosition;
        }
    }
}
