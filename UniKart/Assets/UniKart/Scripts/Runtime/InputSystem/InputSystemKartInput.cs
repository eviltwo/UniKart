using UnityEngine.InputSystem;

namespace UniKart
{
    public class InputSystemKartInput : KartInput
    {
        public PlayerInput PlayerInput;

        public InputActionReference ThrottleAction;

        public InputActionReference BrakeAction;

        public InputActionReference SteeringAction;

        public InputActionReference DriftAction;

        private float _throttle;

        private float _brake;

        private float _steering;

        private bool _drift;

        private void Awake()
        {
            ThrottleAction?.action.Enable();
            BrakeAction?.action.Enable();
            SteeringAction?.action.Enable();
            DriftAction?.action.Enable();
        }

        private void OnEnable()
        {
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered += OnActionTriggerd;
            }
        }

        private void OnDisable()
        {
            if (PlayerInput != null)
            {
                PlayerInput.onActionTriggered -= OnActionTriggerd;
            }
        }

        private void OnActionTriggerd(InputAction.CallbackContext context)
        {
            if (context.action == ThrottleAction?.action)
            {
                _throttle = context.ReadValue<float>();
            }
            else if (context.action == BrakeAction?.action)
            {
                _brake = context.ReadValue<float>();
            }
            else if (context.action == SteeringAction?.action)
            {
                _steering = context.ReadValue<float>();
            }
            else if (context.action == DriftAction?.action)
            {
                _drift = context.ReadValueAsButton();
            }
        }

        public override float GetThrottle()
        {
            return _throttle;
        }

        public override float GetBrake()
        {
            return _brake;
        }

        public override float GetSteering()
        {
            return _steering;
        }

        public override bool GetDrift()
        {
            return _drift;
        }
    }
}
