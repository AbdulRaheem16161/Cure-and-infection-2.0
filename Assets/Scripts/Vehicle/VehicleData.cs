using System;
using UnityEngine;
using UnityEngine.Serialization;

public class VehicleData : MonoBehaviour
{
    [Header("Visual References")]
    public GameObject steeringWheel;
    public ParticleSystem hitParticles;
    public ParticleSystem smokeParticles;
    public ParticleSystem fireParticles;
    public ParticleSystem exhaustParticles;
    public Transform driverSeat;
    public Transform passengerSeat;
    public Transform exhaustPoint;

    [Header("Vehicle Stats")]
    public string vehicleName;
    public float maxHealth;
    public float currentHealth;
    public float currentSpeed = 0f;
    public float maxSpeed;
    public float acceleration;
    public float handlingSpeed;
    public float maxTurnAngle;
    public float brakeForce;
    public float handBrakeForce;
    public float engineBrakeForce;
    public float fuelCapacity;
    public float currentFuel;
    public Vector3 centerOfMass;

    [Header("State")]
    public Axel driveAxel;
    public HealthState currentHealthState;
    public enum HealthState { Healthy, Damaged, Smoking, Critical }

    [Header("Lights")]
    public Light[] headLights;
    public Light[] brakeLights;
    public Light[] reverseLights;

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

    [Header("Wheels")]
    public Wheel[] wheels;

    [Header("Audio")]
    public AudioClip engineSfx;
    public AudioClip revSfx;
    public AudioClip brakeSfx;
    public AudioClip hornSfx;
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