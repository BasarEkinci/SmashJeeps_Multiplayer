using System.Collections.Generic;
using _GameAssets.Scripts.Enums;
using _GameAssets.Scripts.Extensions;
using _GameAssets.Scripts.ScriptableObjects;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace _GameAssets.Scripts.Player
{
    public class PlayerVehicleController : NetworkBehaviour
    {
        private class SpringData
        {
            public float CurrentLength;
            public float CurrentVelocity;
        }

        #region Public Fields
        public VehicleSettingsSo VehicleSettings => vehicleSettings;
        public Vector3 VehicleForward => _vehicleTransform.forward;
        public Vector3 Velocity => vehicleRigidbody.linearVelocity;
        #endregion
        #region Serialized Fields
        [Header("References")] 
        [SerializeField] private VehicleSettingsSo vehicleSettings;
        [SerializeField] private Rigidbody vehicleRigidbody;
        [SerializeField] private BoxCollider vehicleCollider;
        #endregion        
        #region Private Fields
        private static readonly WheelType[] Wheels = new WheelType[]
        {
            WheelType.FrontLeft, WheelType.FrontRight, WheelType.BackLeft, WheelType.BackRight
        };

        private static readonly WheelType[] FrontWheels = new WheelType[]
            { WheelType.FrontLeft, WheelType.FrontRight };

        private static readonly WheelType[] BackWheels = new WheelType[] { WheelType.BackLeft, WheelType.BackRight };

        private Transform _vehicleTransform;
        private Dictionary<WheelType, SpringData> _springData;

        private float _steerInput;
        private float _accelerateInput;
        #endregion
        #region Unity Methods
        private void Awake()
        {
            _vehicleTransform = transform;

            _springData = new Dictionary<WheelType, SpringData>();
            foreach (WheelType wheel in Wheels)
            {
                _springData.Add(wheel, new());
            }
        }
        public override void OnNetworkSpawn()
        {
            vehicleRigidbody.isKinematic = true;
            SetOwnerRigidbodyAsync();
        }
        private void Update()
        {
            if (!IsOwner) return;
            SetSteerInput(Input.GetAxis("Horizontal"));
            SetAccelerateInput(Input.GetAxis("Vertical"));
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            UpdateSuspension();
            UpdateSteering();
            UpdateAccelerate();
            UpdateBreaks();
            UpdateAirResistance();
        }
        #endregion
        #region Helper Methods

         private void SetSteerInput(float steerInput)
        {
            _steerInput = Mathf.Clamp(steerInput, -1.0f, 1.0f);
        }

        private void SetAccelerateInput(float accelerateInput)
        {
            _accelerateInput = Mathf.Clamp(accelerateInput, -1.0f, 1.0f);
        }

        public float GetSpringCurrentLength(WheelType wheel)
        {
            return _springData[wheel].CurrentLength;
        }

        private void CastSpring(WheelType wheel)
        {
            Vector3 position = GetSpringPosition(wheel);

            float previousLength = _springData[wheel].CurrentLength;

            float currentLength;

            if (Physics.Raycast(position, -_vehicleTransform.up, out var hit, vehicleSettings.SpringRestLength))
            {
                currentLength = hit.distance;
            }
            else
            {
                currentLength = vehicleSettings.SpringRestLength;
            }

            _springData[wheel].CurrentVelocity = (currentLength - previousLength) / Time.fixedDeltaTime;
            _springData[wheel].CurrentLength = currentLength;
        }

        private Vector3 GetSpringRelativePosition(WheelType wheel)
        {
            Vector3 boxSize = vehicleCollider.size;
            float boxBottom = boxSize.y * -0.5f;

            float paddingX = vehicleSettings.WheelsPaddingsX;
            float paddingZ = vehicleSettings.WheelsPaddingsZ;

            return wheel switch
            {
                WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom,
                    boxSize.z * (0.5f - paddingZ)),
                WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom,
                    boxSize.z * (0.5f - paddingZ)),
                WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom,
                    boxSize.z * (paddingZ - 0.5f)),
                WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom,
                    boxSize.z * (paddingZ - 0.5f)),
                _ => default,
            };
        }

        private Vector3 GetSpringPosition(WheelType wheel)
        {
            return _vehicleTransform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativePosition(wheel));
        }

        private Vector3 GetSpringHitPosition(WheelType wheel)
        {
            Vector3 vehicleDown = -_vehicleTransform.up;
            return GetSpringPosition(wheel) + _springData[wheel].CurrentLength * vehicleDown;
        }

        private Vector3 GetWheelRollDirection(WheelType wheel)
        {
            bool frontWheel = wheel == WheelType.FrontLeft || wheel == WheelType.FrontRight;

            if (frontWheel)
            {
                var steerQuaternion = Quaternion.AngleAxis(_steerInput * vehicleSettings.SteerAngle, Vector3.up);
                return steerQuaternion * _vehicleTransform.forward;
            }
            else
            {
                return _vehicleTransform.forward;
            }
        }

        private Vector3 GetWheelSlideDirection(WheelType wheel)
        {
            Vector3 forward = GetWheelRollDirection(wheel);
            return Vector3.Cross(_vehicleTransform.up, forward);
        }

        private Vector3 GetWheelTorqueRelativePosition(WheelType wheel)
        {
            Vector3 boxSize = vehicleCollider.size;

            float paddingX = vehicleSettings.WheelsPaddingsX;
            float paddingZ = vehicleSettings.WheelsPaddingsZ;

            return wheel switch
            {
                WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0.0f, boxSize.z * (0.5f - paddingZ)),
                WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), 0.0f, boxSize.z * (0.5f - paddingZ)),
                WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0.0f, boxSize.z * (paddingZ - 0.5f)),
                WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), 0.0f, boxSize.z * (paddingZ - 0.5f)),
                _ => default,
            };

        }

        private Vector3 GetWheelTorquePosition(WheelType wheel)
        {
            return _vehicleTransform.localToWorldMatrix.MultiplyPoint3x4(GetWheelTorqueRelativePosition(wheel));
        }

        private float GetWheelGripFactor(WheelType wheel)
        {
            bool frontWheel = wheel == WheelType.FrontLeft || wheel == WheelType.FrontRight;
            return frontWheel ? vehicleSettings.FrontWheelsGridFactor : vehicleSettings.RearWheelsGridFactor;
        }

        private bool IsGrounded(WheelType wheel)
        {
            return _springData[wheel].CurrentLength < vehicleSettings.SpringRestLength;
        }

        #endregion
        #region Car State Methods
        private void UpdateSuspension()
        {
            foreach (WheelType id in _springData.Keys)
            {
                CastSpring(id);
                float currentLength = _springData[id].CurrentLength;
                float currentVelocity = _springData[id].CurrentVelocity;

                float force = SpringMathExtensions.CalculateForceDamped(currentLength, currentVelocity,
                    vehicleSettings.SpringRestLength, vehicleSettings.SpringStrength,
                    vehicleSettings.SpringDamper);

                vehicleRigidbody.AddForceAtPosition(force * _vehicleTransform.up, GetSpringPosition(id));
            }
        }
        private void UpdateAirResistance()
        {
            vehicleRigidbody.AddForce(vehicleCollider.size.magnitude * vehicleSettings.AirResistance * -vehicleRigidbody.linearVelocity);
        }
        private void UpdateBreaks()
        {
            float forwardSpeed = Vector3.Dot(_vehicleTransform.forward, vehicleRigidbody.linearVelocity);
            float speed = Mathf.Abs(forwardSpeed);

            float brakesRatio;

            const float ALMOST_STOPPING_SPEED = 2.0f;
            
            bool almostStopping = speed < ALMOST_STOPPING_SPEED;
            if (almostStopping)
            {
                brakesRatio = 1.0f;
            }
            else
            {
                bool accelerateContrary =
                    !Mathf.Approximately(_accelerateInput, 0.0f) &&
                    Vector3.Dot(_accelerateInput * _vehicleTransform.forward, vehicleRigidbody.linearVelocity) < 0.0f;
                if (accelerateContrary)
                {
                    brakesRatio = 1.0f;
                }
                // NO ACCELERATE INPUT
                else if (Mathf.Approximately(_accelerateInput, 0.0f))
                {
                    brakesRatio = 0.1f;
                }
                else
                {
                    return;
                }
            }

            foreach (WheelType wheel in BackWheels)
            {
                if (!IsGrounded(wheel))
                {
                    continue;
                }

                Vector3 springPosition = GetSpringPosition(wheel);
                Vector3 rollDirection = GetWheelRollDirection(wheel);
                float rollVelocity = Vector3.Dot(rollDirection, vehicleRigidbody.GetPointVelocity(springPosition));

                float desiredVelocityChange = -rollVelocity * vehicleSettings.BrakePower * brakesRatio;
                float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

                Vector3 force = desiredAcceleration * vehicleSettings.TireMass * rollDirection;
                vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheel));
            }
        }
        private void UpdateSteering()
        {
            foreach (WheelType wheel in Wheels)
            {
                if (!IsGrounded(wheel))
                {
                    continue;
                }

                Vector3 springPosition = GetSpringPosition(wheel);

                Vector3 slideDirection = GetWheelSlideDirection(wheel);
                float slideVelocity = Vector3.Dot(slideDirection, vehicleRigidbody.GetPointVelocity(springPosition));

                float desiredVelocityChange = -slideVelocity * GetWheelGripFactor(wheel);
                float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

                Vector3 force = desiredAcceleration * vehicleSettings.TireMass * slideDirection;
                vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheel));
            }
        }

        private void UpdateAccelerate()
        {
            if (Mathf.Approximately(_accelerateInput, 0.0f))
            {
                return;
            }

            float forwardSpeed = Vector3.Dot(_vehicleTransform.forward, vehicleRigidbody.linearVelocity);
            bool movingForward = forwardSpeed > 0.0f;
            float speed = Mathf.Abs(forwardSpeed);

            if (movingForward && speed > vehicleSettings.MaxSpeed)
            {
                return;
            }
            if (!movingForward && speed > vehicleSettings.MaxReverseSpeed)
            {
                return;
            }

            foreach (WheelType wheel in Wheels)
            {
                if (!IsGrounded(wheel))
                {
                    continue;
                }

                Vector3 position = GetWheelTorquePosition(wheel);
                Vector3 wheelForward = GetWheelRollDirection(wheel);
                vehicleRigidbody.AddForceAtPosition(_accelerateInput * vehicleSettings.AcceleratePower * wheelForward,
                    position);
            }
        }
        #endregion
        
        private async void SetOwnerRigidbodyAsync()
        {
            if (IsOwner) await UniTask.DelayFrame(1);
            vehicleRigidbody.isKinematic = false;
        }
    }
}