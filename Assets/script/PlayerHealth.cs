using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;           // hits you can take before dying
    public int lives = 3;               // deaths before game over
    public float invincibleTime = 3f;   // immunity after getting hit
    public float respawnDelay = 1f;     // seconds before coming back
    public float hurtBounce = 6f;       // small hop up when hit

    public int Health => health;        // the HUD reads these
    public int Lives => lives;

    private int health;
    private float invincibleTimer;
    private Vector3 respawnPoint;
    private bool isDead;
    private PlayerMovement movement;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        respawnPoint = transform.position;
        health = maxHealth;
    }

    private void Update()
    {
        // Immunity: count down and blink the sprite on and off
        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
            sr.enabled = Mathf.FloorToInt(invincibleTimer * 10f) % 2 == 0;  // switches 10 times a second
            if (invincibleTimer <= 0f)
                sr.enabled = true;   // make sure we end visible
        }
    }

    public void SetCheckpoint(Vector3 point)
    {
        respawnPoint = point;
    }

    // Enemies call this
    public void TakeDamage()
    {
        if (isDead) return;
        if (invincibleTimer > 0f) return;   // still immune
        if (movement.IsDashing) return;     // dashing phases through enemies

        if (AudioManager.instance != null)
            AudioManager.instance.PlayHurt();   // SOUND: hurt

        health--;
        if (health <= 0)
        {
            Die();
            return;
        }

        invincibleTimer = invincibleTime;                                   // immunity after a hit
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, hurtBounce);   // little hop
        movement.PlayHurt(0.3f);                                            // hurt pose
    }

    // The pit calls this directly, and TakeDamage calls it at 0 health
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        lives--;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayHurt();   // SOUND: death (same hurt sound)

        invincibleTimer = 0f;
        sr.enabled = true;
        movement.StopDash();
        movement.canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;              // pause physics while dead
        movement.PlayAnim("death");

        if (lives > 0)
            Invoke(nameof(Respawn), respawnDelay);       // come back at the last checkpoint
        else
            Invoke(nameof(ShowGameOver), respawnDelay);  // let the death animation play, then Game Over
    }

    private void Respawn()
    {
        transform.position = respawnPoint;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        health = maxHealth;                // full health again
        invincibleTimer = invincibleTime;  // safe for a moment after coming back
        movement.canMove = true;
        movement.PlayAnim("idle");
        isDead = false;
    }

    // Out of lives: tell the GameManager to show the Game Over screen
    private void ShowGameOver()
    {
        GameManager.instance.GameOver();
    }
}