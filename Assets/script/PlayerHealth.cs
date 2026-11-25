using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int lives = 3;
    public float respawnDelay = 1f;

    private Vector3 respawnPoint;
    private bool isDead;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        respawnPoint = transform.position;   // the start is the first respawn point
    }

    public void SetCheckpoint(Vector3 point)
    {
        respawnPoint = point;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        lives--;

        movement.StopDash();
        movement.canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;              // pause physics while dead
        movement.PlayAnim("death");

        if (lives > 0)
            Invoke(nameof(Respawn), respawnDelay);
        else
            Debug.Log("GAME OVER");        // game over screen comes later
    }

    private void Respawn()
    {
        transform.position = respawnPoint;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        movement.canMove = true;
        movement.PlayAnim("idle");
        isDead = false;
    }
}