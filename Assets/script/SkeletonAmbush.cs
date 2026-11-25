using UnityEngine;

public class SkeletonAmbush : MonoBehaviour
{
    public float wakeRange = 5f;      // rises when the player is this close
    public float riseTime = 0.6f;     // length of the rise animation (seconds)
    public float speed = 2f;          // walking speed
    public LayerMask groundLayer;     // what counts as ground
    public Transform edgeCheck;       // empty child in front of the feet
    public string riseAnim = "skelo rise no cloth";   // clip names, must match exactly
    public string walkAnim = "skelo no cloth walk";

    private Transform player;
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator anim;
    private bool awake;               // has it risen yet?
    private bool walking;             // finished rising, now walking?

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

        float diff = player.position.x - transform.position.x;
        if (Mathf.Abs(diff) < 0.1f) return;          // right under the player: don't jitter
        float dir = Mathf.Sign(diff);                // 1 = player is right, -1 = left

        // 3. Face the player FIRST, so the edge check is on the correct side
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -dir;         // minus because the art faces left
        transform.localScale = scale;

        // 4. No ground in front? Stop here, don't walk off the edge
        if (!Physics2D.OverlapCircle(edgeCheck.position, 0.1f, groundLayer)) return;

        // 5. Walk toward the player
        transform.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
    }

    private void WakeUp()
    {
        awake = true;
        sr.enabled = true;                       // appear
        anim.Play(riseAnim, 0, 0f);              // play the rise from the first frame

        if (AudioManager.instance != null)
            AudioManager.instance.PlayRise();    // SOUND: skeleton rises from the ground

        Invoke(nameof(StartWalking), riseTime);  // start walking when the rise ends
    }

    private void StartWalking()
    {
        walking = true;
        col.enabled = true;                      // now it can hurt you
        anim.Play(walkAnim);
    }
}