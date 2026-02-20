using Unity.VisualScripting;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [SerializeField] private VehicleData vehicleData;
    [SerializeField] private TMPro.TextMeshProUGUI speedText;
    [SerializeField] private float speedMultiplier = 2f; // Convert m/s to km/h

    private void Update()
    {
        if (vehicleData != null && speedText != null)
        {
            float speed = vehicleData.currentSpeed * speedMultiplier; // Apply multiplier to convert m/s to km/h
            speedText.text = Mathf.Abs(speed).ToString("0");
        }
    }
}
