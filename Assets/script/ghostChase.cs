using UnityEngine;

public class GhostChase : MonoBehaviour
{
    public float speed = 2f;          // how fast it flies
    public float chaseRange = 6f;     // starts chasing when the player is this close

    private Transform player;
    private Vector3 homePos;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;  // find the player
        homePos = transform.position;                                // remember where it started
    }

    private void Update()
    {
        // 1. Player close? Fly to the player. Otherwise fly back home.
        Vector3 target;
        if (Vector2.Distance(transform.position, player.position) < chaseRange)
            target = player.position;
        else
            target = homePos;

        // 2. Move a little toward the target every frame
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // 3. Face the direction it's flying
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (target.x > transform.position.x ? 1 : -1);
        transform.localScale = scale;
    }
}