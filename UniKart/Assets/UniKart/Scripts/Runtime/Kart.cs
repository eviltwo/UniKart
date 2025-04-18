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

        [Header("Basic Settings")]
        public float MaxSpeed = 20f;

        public float Acceleration = 10f;

        public float SteeringAngle = 60f;

        public float DriftAngleMin = 10f;

        public float DriftAngleMax = 90f;

        public float SlopeAngleLimit = 45f;

        public bool JumpOnDrift = true;

        public float JumpForce = 2f;

        [Header("Advanced Settings")]
        public float WheelDynamicFriction = 0.5f;

        public float WheelStaticFriction = 3.0f;

        [Range(0, 1)]
        public float DriftFrictionMultiplier = 0.2f;

        public float AirSteeringAngleMultiplier = 0.25f;

        public float AirSteeringDelay = 0.3f;

        public float AirSteeringTransitionDuration = 0.5f;

        public bool OverrideGravity = true;

        public Vector3 Gravity = Vector3.up * -9.81f;

        private KartEngine _engine;

        private KartGroundDetector _groundDetector;

        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;

        private float _airElapsedTime;

        private Vector3 _lastGroundNormal;

        public Vector3 GroundNormal => _lastGroundNormal;

        private bool _isFixedUpdateFrame;

        private bool _isDriftInputLast;

        private bool _isJumpRequired;

        public event Action OnJump;

        private bool _isDrifting;

        public bool IsDrifting => _isDrifting;

        private float _driftDirection;

        public float DriftDirection => _driftDirection;

        private bool _isBoosting;

        private float _boostElapsedTime;

        private float _boostDuration;

        public bool IsBoosting => _isBoosting;

        private FrictionCalculator _forwardFrictionCalc = new FrictionCalculator();
        private FrictionCalculator _sidewaysFrictionCalc = new FrictionCalculator();

        private void Awake()
        {
            _engine = new KartEngine();
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

            _engine.MaxSpeed = MaxSpeed;
            _engine.Acceleration = Acceleration;

            if (_isBoosting)
            {
                throttle = 1;
                _boostElapsedTime += deltaTime;
                if (_boostElapsedTime >= _boostDuration)
                {
                    _isBoosting = false;
                    _engine.AccelerationMultiplier = 1.0f;
                    _engine.MaxSpeedMultiplier = 1.0f;
                }
            }

            if (OverrideGravity)
            {
                Rigidbody.useGravity = false;
                Rigidbody.AddForce(Gravity, ForceMode.Acceleration);
            }

            _isGrounded = _groundDetector.ContactCount > 0;
            var groundNormal = _groundDetector.GetGroundNormal();
            var groundVelocity = _groundDetector.GetGroundVelocity();
            if (!_isGrounded)
            {
                groundNormal = _lastGroundNormal;
                groundVelocity = Vector3.zero;
            }

            _groundDetector.ClearContacts();

            _engine.SetThrottle(throttle);
            _engine.Update(deltaTime);
            var engineSpeed = _engine.Speed;

            if (_isGrounded)
            {
                // forward speed
                var relativeVelocity = Rigidbody.linearVelocity - groundVelocity;
                var forward = Rigidbody.rotation * Vector3.forward;
                var relativeForwardSpeed = Vector3.Dot(relativeVelocity, forward);
                var speedDiff = relativeForwardSpeed - engineSpeed;
                _forwardFrictionCalc.DynamicFriction = WheelDynamicFriction;
                _forwardFrictionCalc.StaticFriction = WheelStaticFriction;
                _forwardFrictionCalc.Update(speedDiff);
                Rigidbody.AddForce(forward * _forwardFrictionCalc.FrictionVelocity * Rigidbody.mass, ForceMode.Acceleration);


                // sideways speed
                var sideways = Rigidbody.rotation * Vector3.right;
                var relativeSidewaysSpeed = Vector3.Dot(relativeVelocity, sideways);
                var sidewaysDiff = relativeSidewaysSpeed;
                _sidewaysFrictionCalc.DynamicFriction = WheelDynamicFriction * (_isDrifting ? DriftFrictionMultiplier : 1f);
                _sidewaysFrictionCalc.StaticFriction = WheelStaticFriction * (_isDrifting ? 0f : 1f);
                _sidewaysFrictionCalc.Update(sidewaysDiff);
                Rigidbody.AddForce(sideways * _sidewaysFrictionCalc.FrictionVelocity * Rigidbody.mass, ForceMode.Acceleration);
            }

            // Jump
            if (_isJumpRequired)
            {
                _isJumpRequired = false;
                if (_isGrounded)
                {
                    Rigidbody.AddForce(groundNormal * JumpForce, ForceMode.VelocityChange);
                    OnJump?.Invoke();
                }
            }

            // Drift
            if (_isDrifting)
            {
                if (!drift)
                {
                    _isDrifting = false;
                }
            }
            else if (drift && Mathf.Abs(steering) > 0.5f)
            {
                _isDrifting = true;
                _driftDirection = Mathf.Sign(steering);
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
            var angle = _isDrifting ? Mathf.Lerp(DriftAngleMin, DriftAngleMax, Mathf.InverseLerp(-1, 1, steering * _driftDirection)) * _driftDirection : SteeringAngle * steering;
            if (!_isGrounded)
            {
                _airElapsedTime += deltaTime;
                var multiplier = Mathf.Lerp(1f, AirSteeringAngleMultiplier, (_airElapsedTime - AirSteeringDelay) / AirSteeringTransitionDuration);
                angle *= multiplier;
            }
            else
            {
                _airElapsedTime = 0f;
            }

            var deltaAngle = angle * deltaTime;
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

            if (JumpOnDrift)
            {
                var driftInput = KartInput.GetDrift();
                _isJumpRequired |= !_isDriftInputLast && driftInput;
                _isDriftInputLast = driftInput;
            }
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

        /// <summary>
        /// To prevent the kart from being blown away by small bumps, vertical impact is reduced.
        /// </summary>
        private void ReduceHop(Collision collision)
        {
            // Reduce upward impact completely
            var rbUpV = Vector3.Dot(Rigidbody.linearVelocity, _lastGroundNormal);
            var impUpV = Vector3.Dot(collision.impulse, _lastGroundNormal) / Rigidbody.mass;
            var usingUpV = Mathf.Min(rbUpV, impUpV);
            if (usingUpV > 0)
            {
                Rigidbody.linearVelocity -= _lastGroundNormal * usingUpV;

                // Reduce horizontal impact.
                // Because, keep horizontal speed is important for kart.
                var usingRatio = Mathf.Clamp01(usingUpV / impUpV);
                var hrzV = collision.impulse - Vector3.Project(collision.impulse, _lastGroundNormal);
                Rigidbody.linearVelocity -= hrzV / Rigidbody.mass * usingRatio * 0.5f; // 1.0 is too much
            }
        }

        public void Boost(float accelerationMultiplier, float maxSpeedMultiplier, float duration)
        {
            _isBoosting = true;
            _boostElapsedTime = 0f;
            _boostDuration = duration;
            _engine.AccelerationMultiplier = accelerationMultiplier;
            _engine.MaxSpeedMultiplier = maxSpeedMultiplier;
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
            if (Mathf.Abs(diffV) < vThreshold)
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

                var v = collision.rigidbody == null ? Vector3.zero : collision.rigidbody.GetPointVelocity(contact.point);
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
        public float MaxSpeed { get; set; } = 20f;

        public float Acceleration { get; set; } = 10f;

        public float AccelerationMultiplier { get; set; } = 1.0f;

        public float MaxSpeedMultiplier { get; set; } = 1.0f;

        private float _throttle;

        private float _speed;

        public float Speed => _speed;

        public void SetThrottle(float throttle)
        {
            _throttle = throttle;
        }

        public void Update(float deltaTime)
        {
            _speed = Mathf.MoveTowards(_speed, MaxSpeed * MaxSpeedMultiplier * _throttle, Acceleration * AccelerationMultiplier * deltaTime);
        }
    }
}
