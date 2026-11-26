using UnityEngine;
using UnityEngine.UI;

// Put on the health bar at the top of the screen (Tools > Style UI does it for you).
// Every frame it makes the red fill match the player's health: full = 3/3, empty = 0/3.
public class HealthBar : MonoBehaviour
{
    public PlayerHealth player;     // the Player
    public Image fill;              // the red bar (Image Type = Filled)
    public float speed = 3f;        // how fast the bar slides down/up (1 = whole bar in 1 second)

    private void Update()
    {
        if (player == null || fill == null) return;

        float target = (float)player.Health / player.maxHealth;   // (float) so 2/3 = 0.66, not 0

        // slide toward the new value instead of jumping, so you can see the hit
        // unscaled time, so it still moves while the game is frozen behind a menu
        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, target, speed * Time.unscaledDeltaTime);
    }
}
