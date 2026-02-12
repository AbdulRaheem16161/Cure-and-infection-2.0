using UnityEngine;

public class HoverCube : MonoBehaviour
{
    #region Settings
    [SerializeField] private float hoverHeight = 0.5f;  // How far it moves up and down
    [SerializeField] private float hoverSpeed = 1f;     // How fast it moves
    #endregion

    #region Runtime
    private Vector3 _startPosition;
    #endregion

    private void Awake()
    {
        #region Initialize
        _startPosition = transform.position;
        #endregion
    }

    private void Update()
    {
        #region Hover Motion
        float newY = _startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);
        #endregion
    }
}
