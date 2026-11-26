using UnityEngine;

public class GhostChase : MonoBehaviour
{
    public float speed = 2f;            // normal chase speed
    public float chaseRange = 6f;       // notices the player this close
    public float attackRange = 2.5f;    // starts the halo attack this close
    public float windUpTime = 1f;       // glows in place first (the warning)
    public float attackTime = 0.5f;     // how long the glowing lunge lasts
    public float attackSpeed = 4f;      // lunge speed
    public float restTime = 1.5f;       // pause after an attack
    [Header("Floating around when the player is far")]
    public float wanderWidth = 2f;      // drifts this far left and right of where you placed it
    public float wanderSpeed = 0.6f;    // how fast it drifts left and right
    public float bobHeight = 0.3f;      // bobs this much up and down
    public float bobSpeed = 2f;         // how fast it bobs

    public string floatAnim = "ghost anima";        // your clip names, must match exactly
    public string haloAnim = "ghost anima helo";

    // The 4 things the ghost can be doing
    private enum State
    { Chase, WindUp, Attack, Rest }

    private State state = State.Chase;

    private Transform player;
    private Vector3 homePos;
    private Animator anim;
    private Collider2D col;
    private float timer;
    private float wanderOffset;         // random start point, so two ghosts don't float in sync

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
        homePos = transform.position;
        wanderOffset = Random.value * 10f;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        col.enabled = false;            // harmless until it attacks
        anim.Play(floatAnim);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chase:
                // Follow the player if close, otherwise float around its home spot
                if (dist < chaseRange)
                    MoveTo(player.position, speed);
                else
                    MoveTo(WanderPoint(), speed);

                // Close enough? Start glowing (warning)
                if (dist < attackRange)
                {
                    state = State.WindUp;
                    timer = windUpTime;
                    anim.Play(haloAnim);
                }
                break;

            case State.WindUp:
                // Stay still while glowing, then attack
                if (timer <= 0f)
                {
                    state = State.Attack;
                    timer = attackTime;
                    col.enabled = true;     // now it can kill

                    if (AudioManager.instance != null)
                        AudioManager.instance.PlayGhostAttack();   // SOUND: ghostly swoop
                }
                break;

            case State.Attack:
                // Fast lunge toward the player, still glowing
                MoveTo(player.position, attackSpeed);
                if (timer <= 0f)
                {
                    state = State.Rest;
                    timer = restTime;
                    col.enabled = false;    // harmless again
                    anim.Play(floatAnim);
                }
                break;

            case State.Rest:
                // Pause, then chase again
                if (timer <= 0f)
                    state = State.Chase;
                break;
        }
    }

    // A point that slowly swings left and right around home and bobs up and down.
    // Sin goes smoothly from -1 to 1 and back, so the ghost drifts instead of jumping.
    private Vector3 WanderPoint()
    {
        float t = Time.time + wanderOffset;
        return homePos + new Vector3(
            Mathf.Sin(t * wanderSpeed) * wanderWidth,
            Mathf.Sin(t * bobSpeed) * bobHeight,
            0f);
    }

    // Move toward a point and face it
    private void MoveTo(Vector3 target, float moveSpeed)
    {
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Only turn around if the target is clearly to one side (stops flickering)
        float diff = target.x - transform.position.x;
        if (Mathf.Abs(diff) > 0.2f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (diff > 0 ? 1 : -1);
            transform.localScale = scale;
        }
    }
}