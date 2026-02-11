using UnityEngine;

public class VehicleData : MonoBehaviour
{
    public GameObject br, bl, fr, fl, steeringWheel;
    public WheelCollider brCol, blCol, frCol, flCol;
    public string vehicleName;
    public int maxHealth;
    public int currentHealth;
    public float speed;
    public float acceleration;
    public float turnSpeed;
    public int fuelCapacity;
    public int currentFuel;
    public Vector3 centerOfMass;
    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass;
    }
    public VehicleData(string name, int health, float spd, float accel, float turn, int fuel)
    {
        vehicleName = name;
        maxHealth = health;
        currentHealth = health;
        speed = spd;        
        acceleration = accel;
        turnSpeed = turn;
        fuelCapacity = fuel;
        currentFuel = fuel;
    }

    public bool IsAlive() => currentHealth > 0;
    
    public void TakeDamage(int damage) => currentHealth = Mathf.Max(0, currentHealth - damage);
    
    public void ConsumeFuel(int amount) => currentFuel = Mathf.Max(0, currentFuel - amount);
}