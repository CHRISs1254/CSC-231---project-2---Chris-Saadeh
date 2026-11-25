using UnityEngine;

public class HellGatoCharge : MonoBehaviour
{
    public float sightRange = 7f;     // sees the player within this distance
    public float chargeSpeed = 9f;    // charge speed (fast)
    public float chargeTime = 0.8f;   // how long one charge lasts
    public float restTime = 1.2f;     // rest between charges
    public float distance = 6f;       // never runs further than this from its start

    private Transform player;
    private Animator anim;
    private Vector3 startPos;
    private float timer;
    private bool charging;
    private float dir;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().transform;
        anim = GetComponent<Animator>();
        startPos = transform.position;
        anim.speed = 0f;              // frozen pose while waiting
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (charging)
        {
            // 1. Run fast in one direction
            Vector3 pos = transform.position;
            pos.x += dir * chargeSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, startPos.x - distance, startPos.x + distance);
            transform.position = pos;

            // 2. Charge over: stop and rest
            if (timer <= 0f)
            {
                charging = false;
                timer = restTime;
                anim.speed = 0f;
            }
        }
        else if (timer <= 0f && PlayerInSight())
        {
            // 3. Rested and sees the player: charge toward them
            charging = true;
            timer = chargeTime;
            anim.speed = 1f;          // run animation plays
            dir = Mathf.Sign(player.position.x - transform.position.x);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            transform.localScale = scale;
        }
    }

    // Player close sideways and at about the same height?
    private bool PlayerInSight()
    {
        return Mathf.Abs(player.position.x - transform.position.x) < sightRange
            && Mathf.Abs(player.position.y - transform.position.y) < 3f;
    }
}