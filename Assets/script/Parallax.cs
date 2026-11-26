using UnityEngine;

// Put this on each background layer. Far layers follow the camera more (move slower on screen),
// close layers follow it less (move faster on screen). That difference makes the depth.
[DefaultExecutionOrder(100)]   // runs after CameraFollow, so the layers don't shake
public class Parallax : MonoBehaviour
{
    [Range(0f, 1f)] public float followX = 0.5f;   // 1 = stuck to camera (very far), 0 = part of the world (close)
    [Range(0f, 1f)] public float followY = 0.5f;   // same, up and down

    // Tick this on wide background images (sky, graveyard). It puts a copy on the left and right,
    // and keeps the layer around the camera, so the image never runs out and the blue never shows.
    // Leave it off for single objects like grass or bushes, or they get copied too.
    public bool repeat = false;

    private Transform cam;
    private Vector3 startPos;      // where this layer started
    private Vector3 camStart;      // where the camera started
    private float width;           // width of one copy of the image, in world units

    private void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
        camStart = cam.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (repeat && sr != null)
        {
            width = sr.bounds.size.x;
            MakeCopy(sr, -1);   // copy on the left
            MakeCopy(sr, 1);    // copy on the right
        }
    }

    private void LateUpdate()
    {
        Vector3 moved = cam.position - camStart;   // how far the camera has moved since the start

        float x = startPos.x + moved.x * followX;

        // REPEAT: if the camera got more than half an image away from the layer,
        // jump the layer a whole image width toward it. The copies look the same, so you can't see the jump.
        if (repeat && width > 0f)
            x += Mathf.Round((cam.position.x - x) / width) * width;

        transform.position = new Vector3(
            x,
            startPos.y + moved.y * followY,
            startPos.z);
    }

    // Makes a child with the same picture, one image width to the left (side = -1) or right (side = 1)
    private void MakeCopy(SpriteRenderer original, int side)
    {
        GameObject copy = new GameObject(name + " copy");
        copy.transform.SetParent(transform, false);   // a child, so it moves and scales with the layer

        // local width = the width before the layer's scale (the child gets the scale from the parent)
        float localWidth = original.drawMode == SpriteDrawMode.Simple
            ? original.sprite.bounds.size.x
            : original.size.x;
        copy.transform.localPosition = new Vector3(side * localWidth, 0f, 0f);

        SpriteRenderer sr = copy.AddComponent<SpriteRenderer>();
        sr.sprite = original.sprite;
        sr.drawMode = original.drawMode;
        if (original.drawMode != SpriteDrawMode.Simple)
            sr.size = original.size;                  // Tiled/Sliced images keep their stretched size
        sr.color = original.color;
        sr.flipX = original.flipX;
        sr.sharedMaterial = original.sharedMaterial;
        sr.sortingLayerID = original.sortingLayerID;
        sr.sortingOrder = original.sortingOrder;
    }
}
