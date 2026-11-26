using UnityEngine;

// Put this on the Player. While dashing, it leaves see-through purple copies of the player behind
// (afterimages, like a ghost trail). Each copy fades out and deletes itself.
public class DashTrail : MonoBehaviour
{
    public Color trailColor = new Color(0.6f, 0.3f, 1f, 0.7f);   // ghostly purple, a bit see-through
    public float spawnEvery = 0.025f;   // seconds between copies (smaller = more copies, smoother trail)
    public float fadeTime = 0.3f;       // how long each copy takes to disappear

    private PlayerMovement movement;
    private SpriteRenderer sr;
    private float timer;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!movement.DashActive)
        {
            timer = 0f;                 // so the first copy appears right when the next dash starts
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnCopy();
            timer = spawnEvery;
        }
    }

    // Makes one frozen copy of how the player looks right now
    private void SpawnCopy()
    {
        GameObject copy = new GameObject("Dash Afterimage");
        copy.transform.position = transform.position;
        copy.transform.rotation = transform.rotation;
        copy.transform.localScale = transform.localScale;   // same size and same facing direction

        SpriteRenderer copySr = copy.AddComponent<SpriteRenderer>();
        copySr.sprite = sr.sprite;                // the animation frame showing right now
        copySr.flipX = sr.flipX;
        copySr.sharedMaterial = sr.sharedMaterial;
        copySr.sortingLayerID = sr.sortingLayerID;
        copySr.sortingOrder = sr.sortingOrder - 1; // behind the player
        copySr.color = trailColor;

        Afterimage fade = copy.AddComponent<Afterimage>();
        fade.fadeTime = fadeTime;
    }
}

// Lives on each copy: fades it from its color to invisible, then deletes it
public class Afterimage : MonoBehaviour
{
    public float fadeTime = 0.3f;

    private SpriteRenderer sr;
    private Color startColor;
    private float age;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = age / fadeTime;               // 0 = just made, 1 = fully faded

        Color c = startColor;
        c.a = Mathf.Lerp(startColor.a, 0f, t); // less and less visible
        sr.color = c;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
