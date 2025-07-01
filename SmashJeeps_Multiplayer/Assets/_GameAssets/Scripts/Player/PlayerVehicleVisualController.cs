using System.Collections.Generic;
using _GameAssets.Scripts.Enums;
using UnityEngine;

namespace _GameAssets.Scripts.Player
{
    public class PlayerVehicleVisualController : MonoBehaviour
    {
        [SerializeField] private Transform _jeepVisualTransform;
        [SerializeField] private Collider _playerCollider;
        [SerializeField] private Transform _wheelFrontLeft, _wheelFrontRight, _wheelBackLeft, _wheelBackRight;
        [SerializeField] private float _wheelsSpinSpeed, _wheelYWhenSpringMin, _wheelYWhenSpringMax;
        [SerializeField] private ParticleSystem[] _smokeParticles;

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

            _wheelFrontLeftRoll = _wheelFrontLeft.localRotation;
            _wheelFrontRightRoll = _wheelFrontRight.localRotation;

            _springsRestLength = _playerVehicleController.VehicleSettings.SpringRestLength;
            _steerAngle = _playerVehicleController.VehicleSettings.SteerAngle;
        }

        private void Update()
        {
            UpdateVisualStates();
            RotateWheels();
            SetSuspension();
        }
        private void RotateWheels()
        {
            if (_springsCurrentLength[WheelType.FrontLeft] < _springsRestLength)
                _wheelFrontLeftRoll *=
                    Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.FrontRight] < _springsRestLength)
                _wheelFrontRightRoll *=
                    Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.BackLeft] < _springsRestLength)
                _wheelBackLeft.localRotation *=
                    Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            if (_springsCurrentLength[WheelType.BackRight] < _springsRestLength)
                _wheelBackRight.localRotation *=
                    Quaternion.AngleAxis(_forwardSpeed * _wheelsSpinSpeed * Time.deltaTime, Vector3.right);

            _wheelFrontLeft.localRotation =
                Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontLeftRoll;
            _wheelFrontRight.localRotation =
                Quaternion.AngleAxis(_steerInput * _steerAngle, Vector3.up) * _wheelFrontRightRoll;
        }

        private void SetSuspension()
        {
            var springFrontLeftRatio = _springsCurrentLength[WheelType.FrontLeft] / _springsRestLength;
            var springFrontRightRatio = _springsCurrentLength[WheelType.FrontRight] / _springsRestLength;
            var springBackLeftRatio = _springsCurrentLength[WheelType.BackLeft] / _springsRestLength;
            var springBackRightRatio = _springsCurrentLength[WheelType.BackRight] / _springsRestLength;

            _wheelFrontLeft.localPosition = new Vector3(_wheelFrontLeft.localPosition.x,
                _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontLeftRatio,
                _wheelFrontLeft.localPosition.z);

            _wheelFrontRight.localPosition = new Vector3(_wheelFrontRight.localPosition.x,
                _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springFrontRightRatio,
                _wheelFrontRight.localPosition.z);

            _wheelBackRight.localPosition = new Vector3(_wheelBackRight.localPosition.x,
                _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackRightRatio,
                _wheelBackRight.localPosition.z);

            _wheelBackLeft.localPosition = new Vector3(_wheelBackLeft.localPosition.x,
                _wheelYWhenSpringMin + (_wheelYWhenSpringMax - _wheelYWhenSpringMin) * springBackLeftRatio,
                _wheelBackLeft.localPosition.z);
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