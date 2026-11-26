using UnityEngine;

public class SkeletonAmbush : MonoBehaviour
{
    public float wakeRange = 5f;      // rises when the player is this close
    public float riseTime = 0.6f;     // length of the rise animation (seconds)
    public float speed = 2f;          // walking speed
    public float chaseHeight = 1.5f;  // only chases if the player is about on the same floor (up/down distance)
    public float chaseRange = 8f;     // only chases if the player is this close (left/right distance)
    public LayerMask groundLayer;     // what counts as ground
    public Transform edgeCheck;       // empty child in front of the feet, a little below the ground line
    public string riseAnim = "skelo rise no cloth";   // clip names, must match exactly
    public string walkAnim = "skelo no cloth walk";

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    private bool awake;               // has it risen yet?
    private bool walking;             // finished rising, now walking?
    private float patrolDir = -1f;    // walking direction when not chasing: 1 = right, -1 = left
    private float turnTimer;          // short wait after turning around, so it doesn't flip every frame

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sr.enabled = false;           // invisible: hidden underground
        col.enabled = false;          // can't hurt you while hidden
    }

    private void Update()
    {
        // 1. Sleeping: wake up when the player comes close
        if (!awake)
        {
            if (Vector2.Distance(transform.position, player.position) < wakeRange)
                WakeUp();
            return;
        }

        // 2. Still rising: wait
        if (!walking) return;

        turnTimer -= Time.deltaTime;

        // 3. CHASE or PATROL?
        // Chase only if the player is on about the same floor and not too far.
        // Otherwise walk back and forth on its own platform.
        Vector2 toPlayer = player.position - transform.position;
        bool chasing = Mathf.Abs(toPlayer.y) < chaseHeight && Mathf.Abs(toPlayer.x) < chaseRange;

        float dir;
        if (chasing)
        {
            if (Mathf.Abs(toPlayer.x) < 0.1f) { Stand(); return; }   // right under the player: don't jitter
            dir = Mathf.Sign(toPlayer.x);                            // 1 = player is right, -1 = left
        }
        else
        {
            dir = patrolDir;
        }

        // 4. Face the way it's going FIRST, so the edge check is on the correct side
        Face(dir);

        // 5. No ground in front?
        if (!Physics2D.OverlapCircle(edgeCheck.position, 0.1f, groundLayer))
        {
            if (!chasing && turnTimer <= 0f)
            {
                patrolDir = -patrolDir;   // patrolling: turn around at the edge
                turnTimer = 0.3f;
            }
            Stand();                      // chasing: wait at the edge, don't fall off
            return;
        }

        // 6. Walk
        anim.speed = 1f;                  // legs move
        transform.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
    }

    // Standing still: freeze the walk animation so it doesn't walk in place
    private void Stand()
    {
        anim.speed = 0f;
    }

    private void Face(float dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;     // minus because the art faces left
        transform.localScale = scale;
    }

    private void WakeUp()
    {
        awake = true;
        sr.enabled = true;                       // appear
        anim.Play(riseAnim, 0, 0f);              // play the rise from the first frame

        if (AudioManager.instance != null)
            AudioManager.instance.PlayRise();    // SOUND: skeleton rises from the ground

        // walk toward the player first
        patrolDir = Mathf.Sign(player.position.x - transform.position.x);

        Invoke(nameof(StartWalking), riseTime);  // start walking when the rise ends
    }

    private void StartWalking()
    {
        walking = true;
        col.enabled = true;                      // now it can hurt you
        anim.Play(walkAnim);
    }
}
