using UnityEngine;

// Put this on the goal at the end of the level (needs a Collider 2D with Is Trigger ticked).
public class LevelGoal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() == null) return;  // only the player can finish
        GameManager.instance.Win();
    }
}