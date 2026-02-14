using System;
using UnityEngine;

public class VehicleData : MonoBehaviour
{
    public GameObject steeringWheel;
    public string vehicleName;
    public int maxHealth;
    public int currentHealth;
    public float maxSpeed;
    public float acceleration;
    public float handlingSpeed;
    public float MaxTurnAngle;
    public float brakeForce;
    public float handBrakeForce;
    public int fuelCapacity;
    public int currentFuel;
    public Vector3 centerOfMass;
    public Light[] headLights, brakeLights, reverseLights;
    public enum Axel { Front, Rear, All }
    public Axel driveAxel;
    [Serializable]
    public struct Wheel
    {
        public GameObject wheelObject;
        public WheelCollider wheelCollider;
        public GameObject wheelEffects; 
        public Axel axel;
    }
    public Wheel[] wheels;
    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass;
    }
    public bool IsAlive() => currentHealth > 0;
    
    public void TakeDamage(int damage) => currentHealth = Mathf.Max(0, currentHealth - damage);
    
    public void ConsumeFuel(int amount) => currentFuel = Mathf.Max(0, currentFuel - amount);
}