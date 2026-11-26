using UnityEngine;
using UnityEngine.UI;

// Pink ashes drifting across the title screen, drawn ON TOP of the menu.
// Put it on an empty UI object that covers the whole screen (Tools > Style UI does it for you).
// Each ash is a tiny pink square that floats up and to the right, sways, fades in and out, then starts again somewhere else.
public class AshesUI : MonoBehaviour
{
    public int count = 70;                                       // how many ashes on screen
    public Color color = new Color(1f, 0.45f, 0.75f, 0.85f);     // pink, a bit see-through
    public Vector2 sizeRange = new Vector2(4f, 10f);             // smallest and biggest ash (pixels)
    public Vector2 wind = new Vector2(45f, 35f);                 // drift per second: right and up
    public float sway = 25f;                                     // how far they wobble side to side
    public Vector2 lifeRange = new Vector2(4f, 9f);              // each ash lives this many seconds

    // Everything one ash needs to remember
    private class Ash
    {
        public RectTransform rect;
        public Image image;
        public Vector2 pos;         // where it is, without the sway
        public float speed;         // some ashes are faster than others
        public float age, life;     // seconds alive, seconds it will live
        public float phase;         // random start of the sway, so they don't wobble together
    }

    private Ash[] ashes;
    private RectTransform area;     // the full-screen object this script is on

    private void Start()
    {
        area = (RectTransform)transform;
        ashes = new Ash[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Ash", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            Ash a = new Ash();
            a.rect = (RectTransform)go.transform;
            a.image = go.GetComponent<Image>();
            a.image.raycastTarget = false;          // clicks go through to the buttons
            ashes[i] = a;

            Respawn(a);
            a.age = Random.Range(0f, a.life);       // start at different ages, so they don't all appear at once
        }
    }

    private void Update()
    {
        // unscaled time: the title screen freezes the game (timeScale = 0), but the ashes should still move
        float dt = Time.unscaledDeltaTime;

        foreach (Ash a in ashes)
        {
            a.age += dt;
            if (a.age >= a.life)
            {
                Respawn(a);
                continue;
            }

            a.pos += wind * a.speed * dt;
            float wobble = Mathf.Sin(a.age * 1.5f + a.phase) * sway;
            a.rect.anchoredPosition = a.pos + new Vector2(wobble, 0f);

            // fade in at the start of its life and out at the end (sin goes 0 -> 1 -> 0)
            Color c = color;
            c.a = color.a * Mathf.Sin(Mathf.PI * a.age / a.life);
            a.image.color = c;
        }
    }

    // Puts an ash at a random spot on the screen with a new size, speed and life
    private void Respawn(Ash a)
    {
        Vector2 half = area.rect.size / 2f;
        a.pos = new Vector2(Random.Range(-half.x, half.x), Random.Range(-half.y, half.y));
        a.speed = Random.Range(0.5f, 1.5f);
        a.age = 0f;
        a.life = Random.Range(lifeRange.x, lifeRange.y);
        a.phase = Random.Range(0f, 10f);

        float size = Mathf.Round(Random.Range(sizeRange.x, sizeRange.y));
        a.rect.sizeDelta = new Vector2(size, size);
        a.rect.anchoredPosition = a.pos;
        a.image.color = new Color(color.r, color.g, color.b, 0f);   // invisible until it fades in
    }
}
