using UnityEngine;
using System.Collections;
using System;
using TMPro;
using UnityEngine.TextCore.LowLevel;


public class VehicleController : MonoBehaviour
{
    private Quaternion steeringWheelBaseRotation;
    private VehicleData data;
    private Rigidbody _rigidbody;
    private WheelFrictionCurve baseFriction;
    private float baseMaxSpeed;
    private float baseHandlingSpeed;
    private float baseMaxTurnAngle;
    private float[] baseHeadLightIntensities;
    private float speedMultiplier = 1f;
    private float handlingMultiplier = 1f;
    private bool engineOn = false;
    private bool isReverse = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        data = GetComponent<VehicleData>();
        steeringWheelBaseRotation = data.steeringWheel.transform.localRotation;

        baseFriction = data.wheels[0].wheelCollider.sidewaysFriction;
        baseFriction.stiffness = data.wheels[0].wheelCollider.sidewaysFriction.stiffness;

        baseMaxSpeed = data.maxSpeed;
        baseHandlingSpeed = data.handlingSpeed;
        baseMaxTurnAngle = data.maxTurnAngle;
        CacheHeadLightIntensities();


        if (data == null) Debug.LogError("DATA IS NULL");
    }
    void Update()
    {
        // Input
        float forwardSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).z;
        float steer = Input.GetAxis("Horizontal");
        
        if(Input.GetKeyDown(KeyCode.E) && data.currentFuel > 0 && data.IsAlive())
        {
            engineOn = !engineOn;
        }
        if (data.currentFuel <= 0)
        {
            engineOn = false;
        }
        if (!data.IsAlive())
        {
            engineOn = false;
        }

        float throttle = engineOn ? Input.GetAxis("Vertical") : 0f;
        //AudioManager.Instance.PlaySound(data.engineSfx);
        if (throttle > 0.5f)
        {
            //AudioManager.Instance.PlaySound(data.revSfx);
        }
        // honking
        //if(Input.GetKeyDown(KeyCode.H)) AudioManager.Instance.PlaySound(data.hornSfx);

        // turnoff lights
        TurnoffLights(engineOn);

        UpdateHealthState();
        ApplyHealthPerformanceAndEffects();
        
        // Drive
        Drive(throttle);
            
        ConsumeFuel(throttle);

        // steering

        WheelTurning(steer);

        steeringWheel(steer);

        // braking
        Braking(forwardSpeed);
        ApplyDamageLightDimming();

        // Update wheel meshes
        foreach (VehicleData.Wheel wheel in data.wheels)
        {            
            UpdateWheel(wheel.wheelCollider, wheel.wheelObject.transform, wheel.wheelEffects);
        }
        // Send speed
        data.currentSpeed = forwardSpeed; // Convert to km/h
    }
    private void UpdateHealthState()
    {
        float healthPercentage = data.currentHealth / data.maxHealth;
        if (healthPercentage >= 0.75f)
        {
            data.currentHealthState = VehicleData.HealthState.Healthy;
        }
        else if (healthPercentage >= 0.5f)
        {
            data.currentHealthState = VehicleData.HealthState.Damaged;
        }
        else if (healthPercentage >= 0.25f)
        {
            data.currentHealthState = VehicleData.HealthState.Smoking;
        }
        else
        {
            data.currentHealthState = VehicleData.HealthState.Critical;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 10f)
        {
            float damage = collision.relativeVelocity.magnitude * collision.relativeVelocity.magnitude * 0.01f; // Adjust this multiplier as needed
            data.TakeDamage(damage);
            Debug.Log($"Vehicle took {damage} damage!");
            PlayParticleEffect(data.hitParticles, collision.contacts[0].point, Mathf.Clamp(damage/10f, 1f, 5f));
        }
    }
    private void PlayParticleEffect(ParticleSystem effect, Vector3 position, float scale)
    {
        GameObject hitParticlesInstance = Instantiate(data.hitParticles.gameObject, position, Quaternion.identity);
        hitParticlesInstance.transform.localScale = Vector3.one * scale;
        hitParticlesInstance.GetComponent<ParticleSystem>().Play();
    }
    private void TurnoffLights(bool engineOn)
    {
        foreach (Light light in data.headLights)
        {
            light.enabled = engineOn;
        }
        foreach (Light light in data.brakeLights)
        {
            light.enabled = engineOn;
        }
        foreach (Light light in data.reverseLights)
        {
            light.enabled = engineOn;
        }
    }

    private void ConsumeFuel(float throttle)
    {
        if (throttle != 0 && data.currentFuel > 0)
        {
            float fuelConsumption = Mathf.Abs(throttle) * 0.01f; // Adjust this multiplier as needed
            data.ConsumeFuel(fuelConsumption);
        }
    }
    private void Drive(float throttle)
    {
        float maxAllowedSpeed = baseMaxSpeed * speedMultiplier;
        float torque = throttle * data.acceleration;
        torque = Mathf.Clamp(torque, -maxAllowedSpeed, maxAllowedSpeed);

        foreach (VehicleData.Wheel wheel in data.wheels)
        {
            if (wheel.axel == data.driveAxel || data.driveAxel == VehicleData.Axel.All)
            {
                wheel.wheelCollider.motorTorque = torque;
            }
        }
    }
    private void ApplyHealthPerformanceAndEffects()
    {
        switch (data.currentHealthState)
        {
            case VehicleData.HealthState.Healthy:
                speedMultiplier = 1f;
                handlingMultiplier = 1f;
                break;
            case VehicleData.HealthState.Damaged:
                speedMultiplier = 0.5f;
                handlingMultiplier = 1f;
                break;
            case VehicleData.HealthState.Smoking:
                speedMultiplier = 0.3f;
                handlingMultiplier = 0.7f;
                break;
            case VehicleData.HealthState.Critical:
                speedMultiplier = 0.1f;
                handlingMultiplier = 0.2f;
                break;
        }

        data.maxSpeed = baseMaxSpeed * speedMultiplier;
        data.handlingSpeed = baseHandlingSpeed * handlingMultiplier;
        data.maxTurnAngle = baseMaxTurnAngle * handlingMultiplier;

        bool shouldSmoke = data.currentHealthState == VehicleData.HealthState.Smoking;
        bool shouldFire = data.currentHealthState == VehicleData.HealthState.Critical;

        ToggleEffect(data.smokeParticles, shouldSmoke);
        ToggleEffect(data.fireParticles, shouldFire);
    }

    private void CacheHeadLightIntensities()
    {
        if (data.headLights == null)
        {
            baseHeadLightIntensities = Array.Empty<float>();
            return;
        }

        baseHeadLightIntensities = new float[data.headLights.Length];
        for (int i = 0; i < data.headLights.Length; i++)
        {
            Light light = data.headLights[i];
            baseHeadLightIntensities[i] = light != null ? light.intensity : 0f;
        }
    }

    private void ApplyDamageLightDimming()
    {
        bool shouldDim = data.currentHealthState == VehicleData.HealthState.Smoking ||
                         data.currentHealthState == VehicleData.HealthState.Critical;
        float dimMultiplier = shouldDim ? 0.5f : 1f;

        for (int i = 0; i < data.headLights.Length; i++)
        {
            Light light = data.headLights[i];
            if (light == null)
            {
                continue;
            }

            float baseIntensity = i < baseHeadLightIntensities.Length ? baseHeadLightIntensities[i] : light.intensity;
            light.intensity = baseIntensity * dimMultiplier;
        }

        foreach (Light light in data.brakeLights)
        {
            if (light != null)
            {
                light.intensity *= dimMultiplier;
            }
        }

        foreach (Light light in data.reverseLights)
        {
            if (light != null)
            {
                light.intensity *= dimMultiplier;
            }
        }
    }
    private void ToggleEffect(ParticleSystem effect, bool shouldPlay)
    {
        if (effect == null)
        {
            return;
        }

        if (shouldPlay)
        {
            if (!effect.isPlaying)
            {
                effect.Play();
            }
        }
        else
        {
            if (effect.isPlaying)
            {
                effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void WheelTurning(float steerInput)
    {
        float steerAngle = Mathf.Lerp
            (data.wheels[0].wheelCollider.steerAngle, steerInput * data.maxTurnAngle,
            data.handlingSpeed * Time.deltaTime);
        
        foreach (VehicleData.Wheel wheel in data.wheels)
        {
            if (wheel.axel == VehicleData.Axel.Front)
            {
                wheel.wheelCollider.steerAngle = steerAngle;
            }
        }
    }
    private void steeringWheel(float steerInput)
    {
        float steeringAngle = steerInput * 180f; 
        data.steeringWheel.transform.localRotation =
            steeringWheelBaseRotation * Quaternion.Euler(0, -steeringAngle, 0);
    }
    private void Braking(float forwardSpeed)
    {

        bool isBraking = isReverse ? Input.GetKey(KeyCode.W) : Input.GetKey(KeyCode.S);

        //reversing logic
        float minSpeedForReverse = 0.1f; // Adjust this threshold as needed
        if (isBraking && Mathf.Abs(forwardSpeed) < minSpeedForReverse && !isReverse)
        {
            isReverse = true;
        }
        else if (isReverse && isBraking && Mathf.Abs(forwardSpeed) < minSpeedForReverse)
        {
            isReverse = false;
        }
        // reverse lights
        foreach (Light light in data.reverseLights)
        {
            light.intensity = isReverse ? 2f : 0f;
        }

        //turning brakelights on/off
        foreach (Light light in data.brakeLights)
        {
            light.intensity = isBraking ? 0.8f : 0.1f;
        }

        if (isBraking)
        {
            foreach (VehicleData.Wheel wheel in data.wheels)
            {
                wheel.wheelCollider.motorTorque = 0;
                wheel.wheelCollider.brakeTorque = data.brakeForce*wheel.brakeRatio;
                wheel.wheelCollider.motorTorque = 0;
            }
           
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            foreach (VehicleData.Wheel wheel in data.wheels)
            {
                if (wheel.axel == VehicleData.Axel.Rear)
                {
                    wheel.wheelCollider.brakeTorque = data.handBrakeForce;
                    wheel.wheelCollider.motorTorque = 0;
                    WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                    friction.stiffness = 0.5f;
                    wheel.wheelCollider.sidewaysFriction = friction;
                }
            }
        }
        else
        {
            foreach (VehicleData.Wheel wheel in data.wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
                WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                friction.stiffness = baseFriction.stiffness;
                wheel.wheelCollider.sidewaysFriction = friction;
            }
        }

        if (!isBraking)
        {
            EngineBreaking(forwardSpeed);
        }
    }
    private void EngineBreaking(float forwardSpeed)
    {
        if (Mathf.Abs(forwardSpeed) > 10f)
        {
            foreach (VehicleData.Wheel wheel in data.wheels)
            {
                if (wheel.axel == data.driveAxel || data.driveAxel == VehicleData.Axel.All)
                {
                    wheel.wheelCollider.brakeTorque = data.engineBrakeForce;
                }
            }
        }
        else
        {
            foreach (VehicleData.Wheel wheel in data.wheels)
            {
                if (wheel.axel == data.driveAxel || data.driveAxel == VehicleData.Axel.All)
                {
                    wheel.wheelCollider.brakeTorque = 0;
                }
            }
        }
    }
    private void UpdateWheel(WheelCollider col, Transform mesh, GameObject effects)
    {
        TrailRenderer trailRenderer = effects != null ? effects.GetComponentInChildren<TrailRenderer>() : null;
        
        if (col.GetGroundHit(out WheelHit hit))
        {
            float slip = Mathf.Abs(hit.sidewaysSlip);

            if (slip > 0.5f)   // tweak this value
            {
                trailRenderer.emitting = true;
                Vector3 skidPosition = hit.point + hit.normal * 0.02f;
                trailRenderer.transform.position = skidPosition;
            }
            else
            {
                trailRenderer.emitting = false;
            }
        }
        else
        {
            trailRenderer.emitting = false;
        }
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}
