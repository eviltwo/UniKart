using UnityEngine;

namespace UniKart
{
    public class KartModelController : MonoBehaviour
    {
        public Kart Kart;

        public Transform Root;

        public float RootRotationSpeed = 1f;

        public Transform Body;

        public float BodyVelocityModifier = 1f;

        public float BodySpringLength = 0.5f;

        public float BodySpringLengthMin = 0.5f;

        public float BodySpringLengthMax = 1.5f;

        public float BodySpringAngleMax = 30f;

        public float BodySpringStrength = 0.5f;

        public float BodySpringDamper = 0.5f;

        [System.Serializable]
        public class WheelInfo
        {
            public Transform Model;
            public float Radius;
            public bool IsSteerable;
            public bool IsDriveable;
            public bool IsFront;
        }

        public WheelInfo[] Wheels;

        private Quaternion _defaultLocalRotation = Quaternion.identity;

        private Quaternion _animatedRootRotation = Quaternion.identity;

        private float _rootSteeringAngle;

        private float _wheelSteeringAngle;

        private Vector3 _defaultBodyLocalPosition;

        private Vector3 _bodyPivot;

        private Vector3 _bodyLastVelocity;

        private Vector3 _bodyVelocity;

        private void Start()
        {
            _defaultLocalRotation = Root.localRotation;
            _defaultBodyLocalPosition = Body.localPosition;
        }

        private void LateUpdate()
        {
            // Root animation
            var currentRot = Root.parent.rotation * _defaultLocalRotation;
            var hrzCrtForward = currentRot * Vector3.forward - Vector3.Project(currentRot * Vector3.forward, Kart.GroundNormal);
            var hrzAnimForward = _animatedRootRotation * Vector3.forward - Vector3.Project(_animatedRootRotation * Vector3.forward, Kart.GroundNormal);
            var hrzAngle = Vector3.SignedAngle(hrzAnimForward, hrzCrtForward, Kart.GroundNormal);
            var hrzFitRot = Quaternion.AngleAxis(hrzAngle, Kart.GroundNormal);
            _animatedRootRotation = hrzFitRot * _animatedRootRotation;
            _animatedRootRotation = Quaternion.Lerp(_animatedRootRotation, currentRot, RootRotationSpeed * Time.deltaTime);
            _rootSteeringAngle = Mathf.Lerp(_rootSteeringAngle, 0, 2 * Time.deltaTime);
            _rootSteeringAngle = Mathf.MoveTowards(_rootSteeringAngle, Kart.KartInput.GetSteering() * 10, 20 * Time.deltaTime);
            var steeringRot = Quaternion.AngleAxis(_rootSteeringAngle, Vector3.up);
            Root.rotation = _animatedRootRotation * steeringRot;

            var sphereCollider = Kart.Collider;
            var sphereCenter = Kart.transform.position + Kart.transform.rotation * Vector3.Scale(sphereCollider.center, Kart.transform.localScale);
            Root.position = sphereCenter + Vector3.Scale(_animatedRootRotation * Vector3.down * sphereCollider.radius, Kart.transform.localScale);

            // Wheels animation
            var floorPoint = sphereCenter - Vector3.Scale(Kart.GroundNormal * sphereCollider.radius, Kart.transform.localScale);
            _wheelSteeringAngle = Mathf.MoveTowards(_wheelSteeringAngle, 0, 2 * Time.deltaTime);
            _wheelSteeringAngle = Mathf.MoveTowards(_wheelSteeringAngle, Kart.KartInput.GetSteering() * 20, 180 * Time.deltaTime);
            var wheelRot = Quaternion.AngleAxis(_wheelSteeringAngle, Vector3.up);
            var rootPlane = new Plane(Kart.GroundNormal, floorPoint);
            foreach (var wheel in Wheels)
            {
                if (wheel.IsSteerable)
                {
                    wheel.Model.localRotation = wheelRot;
                }

                if (wheel.IsFront)
                {
                    var defaultLocalPos = wheel.Model.localPosition;
                    defaultLocalPos.y = wheel.Radius;
                    var defaultPos = wheel.Model.parent.TransformPoint(defaultLocalPos);
                    const float margin = 1f;
                    if (rootPlane.Raycast(new Ray(defaultPos + Root.up * margin, -Root.up), out var distance))
                    {
                        var height = margin - distance + wheel.Radius;
                        if (height > 0)
                        {
                            var wheelLocalPos = defaultLocalPos;
                            wheelLocalPos.y = wheel.Radius + height;
                            wheel.Model.localPosition = wheelLocalPos;
                        }
                        else
                        {
                            wheel.Model.position = defaultPos;
                        }
                    }
                    else
                    {
                        wheel.Model.position = defaultPos;
                    }
                }
            }

            // Body animation
            {

                var worldVelocity = Kart.Rigidbody.GetPointVelocity(Vector3.up * BodySpringLength);
                var velocityDiff = worldVelocity - _bodyLastVelocity;
                _bodyLastVelocity = worldVelocity;
                var localVelocityDiff = Quaternion.Inverse(Body.parent.rotation) * velocityDiff * BodyVelocityModifier;
                _bodyVelocity -= localVelocityDiff;

                // Spring
                var springForce = (Vector3.up * BodySpringLength - _bodyPivot) * BodySpringStrength;
                _bodyVelocity += springForce * Time.deltaTime;

                // Calculate position
                _bodyPivot += _bodyVelocity * Time.deltaTime;

                // Dumping
                _bodyVelocity = Vector3.Lerp(_bodyVelocity, Vector3.zero, BodySpringDamper * Time.deltaTime);

                // Clamp angles
                var angle = Vector3.Angle(Vector3.up, _bodyPivot);
                if (angle > BodySpringAngleMax)
                {
                    var length = _bodyPivot.magnitude;
                    _bodyPivot = Vector3.RotateTowards(Vector3.up, _bodyPivot.normalized, BodySpringAngleMax * Mathf.Deg2Rad, 0) * length;
                }

                // Clamp length
                var newLength = Mathf.Clamp(_bodyPivot.magnitude, BodySpringLengthMin, BodySpringLengthMax);
                _bodyPivot = _bodyPivot.normalized * newLength;

                // Apply
                Body.localPosition = _bodyPivot;
                Body.localRotation = Quaternion.FromToRotation(Vector3.up, _bodyPivot);
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
