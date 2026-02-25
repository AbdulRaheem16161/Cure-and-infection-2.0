using System;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleData : MonoBehaviour
{
    public GameObject steeringWheel;
    public ParticleSystem hitParticles, smokeParticles, fireParticles, exhaustParticles;
    public Transform driverSeat, passengerSeat, exhaustPoint;
    public string vehicleName;
    public float maxHealth;
    public float currentHealth;
    public float currentSpeed = 0f;
    public float maxSpeed;
    public float acceleration;
    public float handlingSpeed;
    public float MaxTurnAngle;
    public float brakeForce;
    public float handBrakeForce;
    public float engineBrakeForce;
    public float fuelCapacity;
    public float currentFuel;
    public Vector3 centerOfMass;
    public Axel driveAxel;
    public HealthState currentHealthState;
    public enum HealthState { Healthy, Damaged, Smoking, Critical}

    public Light[] headLights, brakeLights, reverseLights;
    public enum Axel { Front, Rear, All }
        [Serializable]
    public struct Wheel
    {
        public GameObject wheelObject;
        public WheelCollider wheelCollider;
        public GameObject wheelEffects; 
        public Axel axel;
        public float brakeRatio;
    }
    public Wheel[] wheels;
    public AudioClip engineSfx, revSfx, brakeSfx, hornSfx;
    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass;
    }
    public bool IsAlive() => currentHealth > 0;
    
    public void TakeDamage(float damage) => currentHealth = Mathf.Max(0, currentHealth - damage);
    
    public void ConsumeFuel(float amount) => currentFuel = Mathf.Max(0, currentFuel - amount);
    public void AddFuel(float amount) => currentFuel = Mathf.Min(fuelCapacity, currentFuel + amount);
    public float GetFuelPercentage() => fuelCapacity > 0 ? (float)currentFuel / fuelCapacity : 0f;
}