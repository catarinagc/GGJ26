using UnityEngine;

public class Parallax_Background : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.2f; // 0 = far away, 1 = moves with camera

    private Vector3 previousCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        previousCameraPosition = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 targetPos = new Vector3(
            transform.position.x + (cameraTransform.position.x - previousCameraPosition.x) * parallaxFactor,
            transform.position.y + (cameraTransform.position.y - previousCameraPosition.y) * parallaxFactor,
            transform.position.z
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, 0.1f); // smooth
        previousCameraPosition = cameraTransform.position;
    }


}
