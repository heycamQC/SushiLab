
using UnityEngine;

public class TowerCamera : MonoBehaviour
{
    public float highestPoint = 0f;
    public float cameraOffset = 0f;
    public float smoothSpeed = 5f;

    private float initialY;

    void Start()
    {
        initialY = transform.position.y;
    }

    void LateUpdate()
    {
        float targetY = Mathf.Max(
            initialY,
            highestPoint + cameraOffset
        );

        Vector3 targetPosition = new Vector3(
            transform.position.x,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    public void UpdateHeight(float newHeight)
    {
        if (newHeight > highestPoint)
        {
            highestPoint = newHeight;
        }
    }

    public float GetDeathLine()
    {
        Camera cam = GetComponent<Camera>();

        return transform.position.y - cam.orthographicSize - 1f;
    }
}
