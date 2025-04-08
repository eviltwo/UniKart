using UnityEngine;

namespace UniKart
{
    public class KartModelController : MonoBehaviour
    {
        public Kart Kart;

        public Transform Root;

        public float RootRotationSpeed = 1f;

        public Transform Body;

        [System.Serializable]
        public class WheelInfo
        {
            public Transform Model;
            public float Radius;
            public bool IsSteerable;
            public bool IsDriveable;
            public bool IsForward;
        }

        public WheelInfo[] Wheels;

        private Quaternion _defaultLocalRotation = Quaternion.identity;

        private Quaternion _animatedRootRotation = Quaternion.identity;

        private float _bodySteeringAngle;

        private float _wheelSteeringAngle;

        private void Start()
        {
            _defaultLocalRotation = Body.localRotation;
        }

        private void LateUpdate()
        {
            var currentRot = Root.parent.rotation * _defaultLocalRotation;
            var hrzCrtForward = currentRot * Vector3.forward - Vector3.Project(currentRot * Vector3.forward, Kart.GroundNormal);
            var hrzAnimForward = _animatedRootRotation * Vector3.forward - Vector3.Project(_animatedRootRotation * Vector3.forward, Kart.GroundNormal);
            var hrzAngle = Vector3.SignedAngle(hrzAnimForward, hrzCrtForward, Kart.GroundNormal);
            var hrzFitRot = Quaternion.AngleAxis(hrzAngle, Kart.GroundNormal);
            _animatedRootRotation = hrzFitRot * _animatedRootRotation;
            _animatedRootRotation = Quaternion.Lerp(_animatedRootRotation, currentRot, RootRotationSpeed * Time.deltaTime);
            _bodySteeringAngle = Mathf.MoveTowards(_bodySteeringAngle, 0, 40 * Time.deltaTime);
            _bodySteeringAngle = Mathf.MoveTowards(_bodySteeringAngle, Kart.KartInput.GetSteering() * 10, 90 * Time.deltaTime);
            var steeringRot = Quaternion.AngleAxis(_bodySteeringAngle, Vector3.up);
            Root.rotation = _animatedRootRotation * steeringRot;

            var sphereCollider = Kart.Collider;
            var sphereCenter = Kart.transform.position + Kart.transform.rotation * Vector3.Scale(sphereCollider.center, Kart.transform.localScale);
            Root.position = sphereCenter + Vector3.Scale(_animatedRootRotation * Vector3.down * sphereCollider.radius, Kart.transform.localScale);

            var floorPoint = sphereCenter - Vector3.Scale(Kart.GroundNormal * sphereCollider.radius, Kart.transform.localScale);
            Debug.DrawLine(sphereCenter, floorPoint, Color.red);

            _wheelSteeringAngle = Mathf.MoveTowards(_wheelSteeringAngle, 0, 90 * Time.deltaTime);
            _wheelSteeringAngle = Mathf.MoveTowards(_wheelSteeringAngle, Kart.KartInput.GetSteering() * 20, 180 * Time.deltaTime);
            var wheelRot = Quaternion.AngleAxis(_wheelSteeringAngle, Vector3.up);
            foreach (var wheel in Wheels)
            {
                if (wheel.IsSteerable)
                {
                    wheel.Model.localRotation = wheelRot;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var wheel in Wheels)
            {
                if (wheel.Model == null)
                {
                    continue;
                }

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(wheel.Model.position, wheel.Radius);
            }
        }
    }
}
