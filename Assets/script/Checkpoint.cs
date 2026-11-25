using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public ParticleSystem sparkles;     // drag CheckpointSparkles in here
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;           // only trigger once

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SetCheckpoint(other.transform.position);
            activated = true;
            sparkles.Play();             // start the golden sparkles
        }
    }
}