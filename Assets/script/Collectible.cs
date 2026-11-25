using UnityEngine;

public class Collectible : MonoBehaviour
{
    public GameObject pickupEffect;     // particle burst prefab (optional)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() == null) return;   // only the player collects

        if (SoulCounter.instance != null)
            SoulCounter.instance.AddSoul();                        // +1 on the counter

        if (AudioManager.instance != null)
            AudioManager.instance.PlayCollect();                   // SOUND: collect

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);  // burst

        Destroy(gameObject);                                       // remove the soul
    }
}