using UnityEngine;

namespace _GameAssets.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "VehicleSettingsSo", menuName = "Scriptable Objects/VehicleSettingsSo")]
    public class VehicleSettingsSo : ScriptableObject
    {
        [Header("Suspension Settings")]
        [SerializeField] private float springRestLength;
        [SerializeField] private float springStrength;
        [SerializeField] private float springDamper;
        [Header("Wheel Paddings")]
        [SerializeField] private float wheelsPaddingX;
        [SerializeField] private float wheelsPaddingZ;

        [Header("Handling Settings")] 
        [SerializeField] private float steerAngle;
        [SerializeField] private float frontWheelsGridFactor;
        [SerializeField] private float rearWheelsGridFactor;
        
        [Header("Body")]
        [SerializeField] private float tireMass;

        [Header("Power")] 
        [SerializeField] private float acceleratePower;
        [SerializeField] private float maxSpeed;
        [SerializeField] private float maxReverseSpeed;
        [SerializeField] private float brakePower;
        
        [Header("Air Resistance")]
        [SerializeField] private float airResistance;
        
        public float AirResistance => airResistance;
        public float BrakePower => brakePower;
        public float WheelsPaddingsX => wheelsPaddingX;
        public float WheelsPaddingsZ => wheelsPaddingZ;
        public float SpringRestLength => springRestLength;
        public float SpringStrength => springStrength;
        public float SpringDamper => springDamper;
        public float SteerAngle => steerAngle;
        public float FrontWheelsGridFactor => frontWheelsGridFactor;
        public float RearWheelsGridFactor => rearWheelsGridFactor;
        public float TireMass => tireMass;
        public float AcceleratePower => acceleratePower;
        public float MaxSpeed => maxSpeed;
        public float MaxReverseSpeed => maxReverseSpeed;
    }
}
