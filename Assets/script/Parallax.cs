using UnityEngine;

// Put this on each background layer. Far layers follow the camera more (move slower on screen),
// close layers follow it less (move faster on screen). That difference makes the depth.
[DefaultExecutionOrder(100)]   // runs after CameraFollow, so the layers don't shake
public class Parallax : MonoBehaviour
{
    [Range(0f, 1f)] public float followX = 0.5f;   // 1 = stuck to camera (very far), 0 = part of the world (close)
    [Range(0f, 1f)] public float followY = 0.5f;   // same, up and down

    private Transform cam;
    private Vector3 startPos;      // where this layer started
    private Vector3 camStart;      // where the camera started

    private void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        camStart = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 moved = cam.position - camStart;   // how far the camera has moved since the start

        transform.position = new Vector3(
            startPos.x + moved.x * followX,
            startPos.y + moved.y * followY,
            startPos.z);
    }
}