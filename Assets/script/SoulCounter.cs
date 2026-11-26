using UnityEngine;
using TMPro;

public class SoulCounter : MonoBehaviour
{
    public static SoulCounter instance;   // lets any script reach the counter
    public TMP_Text text;                 // the "Souls: x / y" text on screen

    private int souls;
    private int total;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // count how many souls are placed in the level
        total = FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;
        UpdateText();
    }

    public void AddSoul()
    {
        souls++;
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = "Souls: " + Number(souls) + " / " + Number(total);
    }

    // The gothic font has no numbers (its "0" is even blank), so numbers are written in the normal font.
    // <font="..."> is a TextMeshPro tag that switches font for the text inside it.
    public static string Number(int n)
    {
        return "<font=\"LiberationSans SDF\">" + n + "</font>";
    }
}