using UnityEngine;

// Put this on every enemy. The hero's sword calls TakeHit().
public class EnemyHealth : MonoBehaviour
{
    public int hits = 1;                // how many sword hits to kill it
    public float flashTime = 0.1f;      // how long it flashes red when hit

    private SpriteRenderer sr;

    // A hidden skeleton (invisible underground) can't be hit
    public bool CanBeHit => sr == null || sr.enabled;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeHit()
    {
        hits--;

        // EFFECT: hit flash
        sr.color = Color.red;
        Invoke(nameof(ResetColor), flashTime);

        if (hits > 0) return;           // still alive

        // Dead: stop it from moving or hurting, play the kill sound, then remove it
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
            if (script != this) script.enabled = false;     // stops GhostChase / SkeletonAmbush / Hazard
        foreach (Collider2D c in GetComponents<Collider2D>())
            c.enabled = false;

        if (AudioManager.instance != null)
            AudioManager.instance.PlayKill();

        Destroy(gameObject, flashTime); // disappear after the red flash
    }

    private void ResetColor()
    {
        sr.color = Color.white;
    }
}