using UnityEngine;
using System.Collections;
using System;


public class VehicleController : MonoBehaviour
{
    private Quaternion steeringWheelBaseRotation;
    private VehicleData _data;
    private WheelCollider brCol, blCol, frCol, flCol;
    private Transform brMesh, blMesh, frMesh, flMesh;

    void Start()
    {
        _data = GetComponent<VehicleData>();
        steeringWheelBaseRotation = _data.steeringWheel.transform.localRotation;
        brCol = _data.brCol;
        blCol = _data.blCol;
        frCol = _data.frCol;
        flCol = _data.flCol;

        brMesh = _data.br.transform;
        blMesh = _data.bl.transform;
        frMesh = _data.fr.transform;
        flMesh = _data.fl.transform;

        if (_data == null) Debug.LogError("DATA IS NULL");
        if (_data.fl == null) Debug.LogError("FRONT LEFT TRANSFORM IS NULL");
        if (flCol == null) Debug.LogError("FRONT LEFT WHEEL COLLIDER IS NULL");
    }
    void FixedUpdate() {
        float throttle = Input.GetAxis("Vertical");
        // Drive (rear-wheel)
        float torque = throttle * _data.acceleration;
        torque = Mathf.Clamp(torque, -_data.speed, _data.speed);

        brCol.motorTorque = torque;
        blCol.motorTorque = torque;

    }
    void Update()
    {
        

        float steer = Input.GetAxis("Horizontal");

        // Steering (front wheels)
        float steerAngle = Mathf.Lerp(flCol.steerAngle, steer * _data.turnSpeed, 0.1f);
        flCol.steerAngle = steerAngle;
        frCol.steerAngle = steerAngle;
        // steering wheel
        float steeringAngle = steer * 180f; 
        _data.steeringWheel.transform.localRotation =
            steeringWheelBaseRotation * Quaternion.Euler(0, -steeringAngle, 0);

        // Braking
        if (Input.GetKey(KeyCode.Space))
        {
            flCol.brakeTorque = 1500f;
            frCol.brakeTorque = 1500f;
        }
        else
        {
            flCol.brakeTorque = 0;
            frCol.brakeTorque = 0;
        }

        // // Fuel
        // if (throttle != 0) _data.ConsumeFuel(1);

        // if (!_data.IsAlive() || _data.currentFuel <= 0)
        // {
        //     brCol.motorTorque = 0;
        //     blCol.motorTorque = 0;
        // }

        UpdateWheel(brCol, brMesh);
        UpdateWheel(blCol, blMesh);
        UpdateWheel(frCol, frMesh);
        UpdateWheel(flCol, flMesh);
    }

    private void UpdateWheel(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}
