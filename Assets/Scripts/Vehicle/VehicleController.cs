using UnityEngine;
using System.Collections;
using System;
using TMPro;


public class VehicleController : MonoBehaviour
{
    private Quaternion steeringWheelBaseRotation;
    private VehicleData _data;
    private Rigidbody _rigidbody;
    private WheelFrictionCurve baseFriction;
    private bool isReverse = false;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _data = GetComponent<VehicleData>();
        steeringWheelBaseRotation = _data.steeringWheel.transform.localRotation;

        baseFriction = _data.wheels[0].wheelCollider.sidewaysFriction;
        baseFriction.stiffness = _data.wheels[0].wheelCollider.sidewaysFriction.stiffness;


        if (_data == null) Debug.LogError("DATA IS NULL");
    }
    void Update()
    {
        // Input
        float forwardSpeed = transform.InverseTransformDirection(_rigidbody.linearVelocity).z;
        float throttle = Input.GetAxis("Vertical");
        float steer = Input.GetAxis("Horizontal");
        // Drive

        Drive(throttle);
        
        // steering

        WheelTurning(steer);

        steeringWheel(steer);

        // braking
        Braking(forwardSpeed);

        // Update wheel meshes
        foreach (VehicleData.Wheel wheel in _data.wheels)
        {            
            UpdateWheel(wheel.wheelCollider, wheel.wheelObject.transform, wheel.wheelEffects);
        }
    }
    private void Drive(float throttle)
    {
        float torque = throttle * _data.acceleration;
        torque = Mathf.Clamp(torque, -_data.maxSpeed, _data.maxSpeed);

        foreach (VehicleData.Wheel wheel in _data.wheels)
        {
            if (wheel.axel == _data.driveAxel || _data.driveAxel == VehicleData.Axel.All)
            {
                wheel.wheelCollider.motorTorque = torque;
            }
        }
    }
    private void WheelTurning(float steerInput)
    {
        float steerAngle = Mathf.Lerp
            (_data.wheels[0].wheelCollider.steerAngle, steerInput * _data.MaxTurnAngle, 
            _data.handlingSpeed * Time.deltaTime);
        
        foreach (VehicleData.Wheel wheel in _data.wheels)
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
        _data.steeringWheel.transform.localRotation =
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
        foreach (Light light in _data.reverseLights)
        {
            light.intensity = isReverse ? 2f : 0f;
        }

        //turning brakelights on/off
        foreach (Light light in _data.brakeLights)
        {
            light.intensity = isBraking ? 0.8f : 0.1f;
        }

        if (isBraking)
        {
            foreach (VehicleData.Wheel wheel in _data.wheels)
            {
                if (wheel.axel == VehicleData.Axel.Rear)
                {
                    wheel.wheelCollider.brakeTorque = _data.brakeForce;
                    wheel.wheelCollider.motorTorque = 0;
                    WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                    friction.stiffness = 0.5f;
                    wheel.wheelCollider.sidewaysFriction = friction;
                }
            }
           
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            foreach (VehicleData.Wheel wheel in _data.wheels)
            {
                if (wheel.axel == VehicleData.Axel.Rear)
                {
                    wheel.wheelCollider.brakeTorque = _data.handBrakeForce;
                    wheel.wheelCollider.motorTorque = 0;
                    WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                    friction.stiffness = 0.5f;
                    wheel.wheelCollider.sidewaysFriction = friction;
                }
            }
        }
        else
        {
            foreach (VehicleData.Wheel wheel in _data.wheels)
            {
                if (wheel.axel == VehicleData.Axel.Rear)
                {
                    wheel.wheelCollider.brakeTorque = 0;
                    WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                    friction.stiffness = baseFriction.stiffness;
                    wheel.wheelCollider.sidewaysFriction = friction;
                }
                if (wheel.axel == VehicleData.Axel.Front)
                {
                    WheelFrictionCurve friction = wheel.wheelCollider.sidewaysFriction;
                    friction.stiffness = baseFriction.stiffness;
                    wheel.wheelCollider.sidewaysFriction = friction;
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
