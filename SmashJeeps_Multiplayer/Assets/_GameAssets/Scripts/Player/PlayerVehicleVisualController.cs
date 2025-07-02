using System.Collections.Generic;
using _GameAssets.Scripts.Enums;
using Unity.Netcode;
using UnityEngine;

namespace _GameAssets.Scripts.Player
{
    public class PlayerVehicleVisualController : NetworkBehaviour
    {
        [SerializeField] private Transform jeepVisualTransform;
        [SerializeField] private Collider playerCollider;
        [SerializeField] private Transform wheelFrontLeft, wheelFrontRight, wheelBackLeft, wheelBackRight;
        [SerializeField] private float wheelsSpinSpeed, wheelYWhenSpringMin, wheelYWhenSpringMax;
        [SerializeField] private ParticleSystem[] smokeParticles;

        private PlayerVehicleController _playerVehicleController;
        private Quaternion _wheelFrontLeftRoll;
        private Quaternion _wheelFrontRightRoll;

        private float _forwardSpeed;
        private float _steerInput;
        private float _steerAngle;
        private float _springsRestLength;

        private Dictionary<WheelType, float> _springsCurrentLength = new()
        {
            { WheelType.FrontLeft, 0.0f },
            { WheelType.FrontRight, 0.0f },
            { WheelType.BackLeft, 0.0f },
            { WheelType.BackRight, 0.0f }
        };


        public void Start()
        {
            _playerVehicleController = GetComponent<PlayerVehicleController>();

            _wheelFrontLeftRoll = wheelFrontLeft.localRotation;
            _wheelFrontRightRoll = wheelFrontRight.localRotation;

            _springsRestLength = _playerVehicleController.VehicleSettings.SpringRestLength;
            _steerAngle = _playerVehicleController.VehicleSettings.SteerAngle;
        }

        private void Update()
        {
            if (!IsOwner) return;
            UpdateVisualStates();
            RotateWheels();
            SetSuspension();
        }
        private void RotateWheels()
        {
            if (_springsCurrentLength[WheelType.FrontLeft] < _springsRestLength)
                _wheelFrontLeftRoll *=
                    Quaternion.AngleAxis(_forwardSpeed * wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.FrontRight] < _springsRestLength)
                _wheelFrontRightRoll *=
                    Quaternion.AngleAxis(_forwardSpeed * wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.BackLeft] < _springsRestLength)
                wheelBackLeft.localRotation *=
                    Quaternion.AngleAxis(_forwardSpeed * wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.BackRight] < _springsRestLength)
                wheelBackRight.localRotation *=
                    Quaternion.AngleAxis(_forwardSpeed * wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            wheelFrontLeft.localRotation =
                Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontLeftRoll;
            wheelFrontRight.localRotation =
                Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontRightRoll;
        }

        private void SetSuspension()
        {
            var springFrontLeftRatio = _springsCurrentLength[WheelType.FrontLeft] / _springsRestLength;
            var springFrontRightRatio = _springsCurrentLength[WheelType.FrontRight] / _springsRestLength;
            var springBackLeftRatio = _springsCurrentLength[WheelType.BackLeft] / _springsRestLength;
            var springBackRightRatio = _springsCurrentLength[WheelType.BackRight] / _springsRestLength;

            wheelFrontLeft.localPosition = new Vector3(wheelFrontLeft.localPosition.x,
                wheelYWhenSpringMin + (wheelYWhenSpringMax - wheelYWhenSpringMin) * springFrontLeftRatio,
                wheelFrontLeft.localPosition.z);

            wheelFrontRight.localPosition = new Vector3(wheelFrontRight.localPosition.x,
                wheelYWhenSpringMin + (wheelYWhenSpringMax - wheelYWhenSpringMin) * springFrontRightRatio,
                wheelFrontRight.localPosition.z);

            wheelBackRight.localPosition = new Vector3(wheelBackRight.localPosition.x,
                wheelYWhenSpringMin + (wheelYWhenSpringMax - wheelYWhenSpringMin) * springBackRightRatio,
                wheelBackRight.localPosition.z);

            wheelBackLeft.localPosition = new Vector3(wheelBackLeft.localPosition.x,
                wheelYWhenSpringMin + (wheelYWhenSpringMax - wheelYWhenSpringMin) * springBackLeftRatio,
                wheelBackLeft.localPosition.z);
        }

        private void UpdateVisualStates()
        {
            _steerInput = Input.GetAxis("Horizontal");

            var forwardSpeed = Vector3.Dot(_playerVehicleController.VehicleForward, _playerVehicleController.Velocity);
            _forwardSpeed = forwardSpeed;

            _springsCurrentLength[WheelType.FrontLeft] =
                _playerVehicleController.GetSpringCurrentLength(WheelType.FrontLeft);
            _springsCurrentLength[WheelType.FrontRight] =
                _playerVehicleController.GetSpringCurrentLength(WheelType.FrontRight);
            _springsCurrentLength[WheelType.BackLeft] =
                _playerVehicleController.GetSpringCurrentLength(WheelType.BackLeft);
            _springsCurrentLength[WheelType.BackRight] =
                _playerVehicleController.GetSpringCurrentLength(WheelType.BackRight);
        }
    }
}