using System;
using UnityEngine;

namespace UniKart
{
    public class Kart : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody Rigidbody;

        public SphereCollider Collider;

        public KartInput KartInput;

        [Header("Performances")]
        public KartEngine.Performances EnginePerformances;

        public float WheelDinamicFriction = 0.8f;

        public float WheelStaticFriction = 0.8f;

        public float AngleSpeed = 90f;

        public float SlopeAngleLimit = 45f;

        private KartEngine _engine;

        private KartGroundDetector _groundDetector;

        public bool IsGrounded => _groundDetector.ContactCount > 0;

        private Vector3 _lastGroundNormal;

        private bool _isFixedUpdateFrame;

        private FrictionCalculator _forwardFrictionCalc = new FrictionCalculator();
        private FrictionCalculator _sidewaysFrictionCalc = new FrictionCalculator();

        private void Awake()
        {
            _engine = new KartEngine(EnginePerformances);
            _groundDetector = new KartGroundDetector(Collider);
        }

        private void FixedUpdate()
        {
            _isFixedUpdateFrame = true;
            if (KartInput == null)
            {
                return;
            }

            var deltaTime = Time.deltaTime;

            var throttle = KartInput.GetThrottle();
            var brake = KartInput.GetBrake();
            var steering = KartInput.GetSteering();
            var drift = KartInput.GetDrift();

            var grounded = _groundDetector.ContactCount > 0;
            var groundNormal = _groundDetector.GetGroundNormal();
            var groundVelocity = _groundDetector.GetGroundVelocity();
            if (!grounded)
            {
                groundNormal = _lastGroundNormal;
                groundVelocity = Vector3.zero;
            }

            _groundDetector.ClearContacts();

            Debug.DrawRay(Rigidbody.position, groundNormal, Color.red);

            _engine.SetThrottle(throttle);
            _engine.Update(deltaTime);
            var engineSpeed = _engine.Speed;

            if (grounded)
            {
                // forward speed
                var relativeVelocity = Rigidbody.linearVelocity - groundVelocity;
                var forward = Rigidbody.rotation * Vector3.forward;
                var relativeForwardSpeed = Vector3.Dot(relativeVelocity, forward);
                var speedDiff = relativeForwardSpeed - engineSpeed;
                _forwardFrictionCalc.DynamicFriction = WheelDinamicFriction;
                _forwardFrictionCalc.StaticFriction = WheelStaticFriction;
                _forwardFrictionCalc.Update(speedDiff);
                Rigidbody.AddForce(forward * _forwardFrictionCalc.FrictionVelocity * Rigidbody.mass, ForceMode.Acceleration);


                // sideways speed
                var sideways = Rigidbody.rotation * Vector3.right;
                var relativeSidewaysSpeed = Vector3.Dot(relativeVelocity, sideways);
                var sidewaysDiff = relativeSidewaysSpeed;
                _sidewaysFrictionCalc.DynamicFriction = WheelDinamicFriction;
                _sidewaysFrictionCalc.StaticFriction = WheelStaticFriction;
                _sidewaysFrictionCalc.Update(sidewaysDiff);
                Rigidbody.AddForce(sideways * _sidewaysFrictionCalc.FrictionVelocity * Rigidbody.mass, ForceMode.Acceleration);
            }

            _lastGroundNormal = groundNormal;
        }

        private void AfterFixedUpdate()
        {
            if (KartInput == null)
            {
                return;
            }

            var deltaTime = Time.fixedDeltaTime;
            var steering = KartInput.GetSteering();
            var deltaAngle = steering * AngleSpeed * deltaTime;
            var rotation = Rigidbody.rotation;
            rotation = Quaternion.FromToRotation(rotation * Vector3.up, _lastGroundNormal) * rotation;
            rotation = rotation * Quaternion.AngleAxis(deltaAngle, Vector3.up);
            Rigidbody.rotation = rotation;
        }

        private void Update()
        {
            if (_isFixedUpdateFrame)
            {
                _isFixedUpdateFrame = false;
                AfterFixedUpdate();
            }

            _groundDetector.SlopeAngleLimit = SlopeAngleLimit;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _groundDetector.RegisterCollision(collision);
            ReduceHop(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            _groundDetector.RegisterCollision(collision);
            ReduceHop(collision);
        }

        private void ReduceHop(Collision collision)
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.thisCollider != Collider)
                {
                    continue;
                }

                var upImpulse = Vector3.Dot(_lastGroundNormal, contact.impulse);
                var threshold = 30;
                var multiplier = 1.0f;
                if (upImpulse > threshold)
                {
                    Rigidbody.AddForce(-_lastGroundNormal * (upImpulse - threshold) * multiplier, ForceMode.Impulse);
                }
            }
        }
    }

    public class FrictionCalculator
    {
        public float DynamicFriction { get; set; } = 0.5f;

        public float StaticFriction { get; set; } = 1.0f;

        private bool _isSlipping;

        public bool IsSlipping => _isSlipping;

        public float FrictionVelocity { get; private set; }

        public void Update(float diffV)
        {
            var vThreshold = _isSlipping ? DynamicFriction : StaticFriction;
            if (diffV < vThreshold)
            {
                _isSlipping = false;
                FrictionVelocity = -diffV;
            }
            else
            {
                _isSlipping = true;
                FrictionVelocity = -diffV * DynamicFriction;
            }
        }
    }

    public class KartGroundDetector
    {
        public float SlopeAngleLimit { get; set; } = 45f;

        public Vector3 BaseNormal { get; set; } = Vector3.up;

        private readonly Collider _collider;
        private int _contactCount;
        private Vector3 _totalNormals;
        private Vector3 _totalVelocities;

        public int ContactCount => _contactCount;

        public KartGroundDetector(Collider collider)
        {
            _collider = collider;
        }

        public void RegisterCollision(Collision collision)
        {
            var v = collision.rigidbody ? collision.rigidbody.linearVelocity : Vector3.zero;
            foreach (var contact in collision.contacts)
            {
                var normal = contact.normal;
                var angle = Vector3.Angle(BaseNormal, normal);
                if (angle > SlopeAngleLimit)
                {
                    continue;
                }

                _contactCount++;
                _totalNormals += normal;
                _totalVelocities += v;
            }
        }

        public void ClearContacts()
        {
            _contactCount = 0;
            _totalNormals = Vector3.zero;
            _totalVelocities = Vector3.zero;
        }

        public Vector3 GetGroundNormal()
        {
            if (_contactCount == 0)
            {
                return Vector3.up;
            }

            var normalsAverage = _totalNormals / _contactCount;
            return normalsAverage.normalized;
        }

        public Vector3 GetGroundVelocity()
        {
            if (_contactCount == 0)
            {
                return Vector3.zero;
            }

            return _totalVelocities / _contactCount;
        }
    }

    public class KartEngine
    {
        [Serializable]
        public class Performances
        {
            public float MaxSpeed = 30f;

            public float Acceleration = 10f;
        }

        public Performances EnginePerformances { get; set; }

        private float _throttle;

        private float _speed;

        public float Speed => _speed;

        public KartEngine(Performances enginePerformances)
        {
            EnginePerformances = enginePerformances;
        }

        public void SetThrottle(float throttle)
        {
            _throttle = throttle;
        }

        public void Update(float deltaTime)
        {
            _speed = Mathf.MoveTowards(_speed, EnginePerformances.MaxSpeed * _throttle, EnginePerformances.Acceleration * deltaTime);
        }
    }
}
