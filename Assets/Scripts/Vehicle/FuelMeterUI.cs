using System.Reflection;
using UnityEngine;

public class FuelMeterUI : MonoBehaviour
{
    [SerializeField] private Transform fuelMeterNeedle;
    [SerializeField] private float maxNeedleRotation = 180f;
    private Quaternion _initialNeedleRotation;
    public VehicleData vehicleData;

    private void Start()
    {
        _initialNeedleRotation = fuelMeterNeedle.localRotation;
    }

    private void Update()
    {
        if (vehicleData != null)
        {
            float fuelPercentage = vehicleData.GetFuelPercentage();
            float needleRotation = Mathf.Lerp(-maxNeedleRotation, maxNeedleRotation, fuelPercentage);
            fuelMeterNeedle.localRotation = _initialNeedleRotation * Quaternion.Euler(0f, 0f, -needleRotation);
        }
    }
}
