using UnityEngine;

public class Hazard : MonoBehaviour
{
    public bool instantKill = false;   // tick for the death pit. Enemies leave it off.

    // "Stay" runs every physics step while touching, so you can be hit again
    // if you're still touching the enemy when your immunity ends
    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        if (instantKill)
            health.Die();          // pit: dead right away
        else
            health.TakeDamage();   // enemy: lose 1 health
    }
}