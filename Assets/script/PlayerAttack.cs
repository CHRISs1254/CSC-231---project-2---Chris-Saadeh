using UnityEngine;
using UnityEngine.InputSystem;

// Put this on the Player. J, X or left mouse = sword attack.
// The sword hits anything inside a box in front of the player (shown in red in the Scene view).
public class PlayerAttack : MonoBehaviour
{
    public float range = 2.5f;          // how far in front the sword reaches
    public float extraHeight = 1f;      // makes the box taller than the player (for floating ghosts)
    public float cooldown = 0.4f;       // wait between attacks
    public float animTime = 0.5f;       // length of the attack animation
    public float hitDelay = 0.1f;       // the hit lands in the middle of the swing

    private PlayerMovement movement;
    private Collider2D body;            // the player's own collider, used to place the sword box
    private float cooldownTimer;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        body = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // No attacking in menus (movement disabled) or while dead (canMove false)
        if (!movement.enabled || !movement.canMove) return;

        cooldownTimer -= Time.deltaTime;

        bool pressed = Keyboard.current.jKey.wasPressedThisFrame
                    || Keyboard.current.xKey.wasPressedThisFrame
                    || Mouse.current.leftButton.wasPressedThisFrame;

        if (pressed && cooldownTimer <= 0f)
        {
            cooldownTimer = cooldown;
            movement.PlayLockedAnim("attack", animTime);    // show the swing

            if (AudioManager.instance != null)
                AudioManager.instance.PlayHeroAttack();      // SOUND: sword

            Invoke(nameof(DoHit), hitDelay);
        }
    }

    // The sword box: starts at the player's middle and goes 'range' forward
    private Bounds SwordBox()
    {
        Collider2D col = body != null ? body : GetComponent<Collider2D>();
        Bounds player = col.bounds;
        float facing = Mathf.Sign(transform.localScale.x);  // 1 = facing right, -1 = left

        Vector3 center = new Vector3(player.center.x + facing * range * 0.5f, player.center.y, 0f);
        Vector3 size = new Vector3(range, player.size.y + extraHeight, 100f);  // deep in Z so it always overlaps
        return new Bounds(center, size);
    }

    // Hit every enemy whose sprite touches the sword box
    private void DoHit()
    {
        Bounds sword = SwordBox();

        foreach (EnemyHealth enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            if (!enemy.CanBeHit) continue;

            SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
            if (enemySprite == null) continue;

            if (sword.Intersects(enemySprite.bounds))
                enemy.TakeHit();
        }
    }

    // Draws the sword box in red in the Scene view when the player is selected (helps you tune range)
    private void OnDrawGizmosSelected()
    {
        if (GetComponent<Collider2D>() == null) return;
        Bounds b = SwordBox();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}