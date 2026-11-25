using UnityEngine;

// Put this on the Main Camera. It smoothly follows the player.
public class CameraFollow : MonoBehaviour
{
    public Transform target;                        // drag the Player here
    public Vector2 offset = new Vector2(0f, 1f);    // camera sits a bit above the player
    public float smoothTime = 0.2f;                 // lower = snappier, higher = floatier
    public float lookAhead = 2f;                    // shows more of the level in front of the player

    private Vector3 velocity;                       // used by SmoothDamp to remember the camera's speed

    // LateUpdate runs after the player has moved this frame, so the camera never lags a frame behind
    private void LateUpdate()
    {
        if (target == null) return;

        // Which way is the player facing? (our Flip() makes scale.x negative when facing left)
        float facing = Mathf.Sign(target.localScale.x);

        // Where the camera wants to be: on the player, plus the offset, plus a little in front
        Vector3 wanted = new Vector3(
            target.position.x + offset.x + facing * lookAhead,
            target.position.y + offset.y,
            transform.position.z);                  // keep the camera's Z (usually -10) or it shows nothing

        // Glide toward that point instead of snapping to it
        transform.position = Vector3.SmoothDamp(transform.position, wanted, ref velocity, smoothTime);
    }
}