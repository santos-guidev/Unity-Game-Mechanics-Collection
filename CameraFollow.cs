using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 5f;

    private Vector3 _currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate the desired position based on target's position and offset
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera to the desired position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothSpeed);

        // Look at the target, potentially with some rotation smoothing
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sets a new target for the camera to follow.
    /// </summary>
    /// <param name="newTarget">The transform of the new target.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Adjusts the camera's offset from the target.
    /// </summary>
    /// <param name="newOffset">The new offset vector.</param>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
