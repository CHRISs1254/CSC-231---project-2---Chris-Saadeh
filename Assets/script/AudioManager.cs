using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Plays all sound effects and the music. Other scripts call AudioManager.instance.PlayJump() etc.
// Press M (or click the Mute button) to turn all sound on/off.
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Player sounds (drag the files here)")]
    public AudioClip jumpClip;         // jump.ogg (also used for landing, lower pitch)

    public AudioClip collectClip;      // collect.wav
    public AudioClip hurtClip;         // hurt.ogg
    public AudioClip winClip;          // win.wav
    public AudioClip heroAttackClip;   // attack.ogg - hero sword swing

    [Header("Enemy sounds")]
    public AudioClip riseClip;         // rise.ogg         - skeleton comes out of the ground

    public AudioClip ghostAttackClip;  // ghost_attack.wav - ghost lunges
    public AudioClip killClip;         // kill.ogg         - an enemy dies

    [Header("Music")]
    public AudioClip music;            // sci_fi_platformer04_main_loop.ogg

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.4f;

    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Mute button text (optional)")]
    public TMP_Text muteText;          // shows "Sound: ON" / "Sound: OFF"

    // static = stays the same after Restart reloads the scene
    private static bool muted = false;

    private AudioSource musicSource;   // plays the music on loop
    private AudioSource sfxSource;     // plays normal sound effects
    private AudioSource landSource;    // plays the landing sound (lower pitch)

    private void Awake()
    {
        instance = this;

        // Make 3 speakers on this object
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        landSource = gameObject.AddComponent<AudioSource>();

        landSource.pitch = 0.6f;       // the jump sound, deeper = a soft "thud" for landing
    }

    private void Start()
    {
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();

        ApplyMute();
    }

    private void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
            ToggleMute();
    }

    // ---------- Other scripts call these ----------

    public void PlayJump()
    {
        Play(jumpClip);
    }

    public void PlayLand()
    {
        if (jumpClip != null) landSource.PlayOneShot(jumpClip, sfxVolume * 0.5f);
    }

    public void PlayCollect()
    {
        Play(collectClip);
    }

    public void PlayHurt()
    {
        Play(hurtClip);
    }

    public void PlayWin()
    {
        Play(winClip);
    }

    public void PlayHeroAttack()
    {
        Play(heroAttackClip);
    }

    public void PlayRise()
    {
        Play(riseClip);
    }

    public void PlayGhostAttack()
    {
        Play(ghostAttackClip);
    }

    public void PlayKill()
    {
        Play(killClip);
    }

    // Plays one sound, but only if a clip was dragged in (no error if a slot is empty)
    private void Play(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ---------- Mute ----------

    public void ToggleMute()            // the Mute button calls this
    {
        muted = !muted;
        ApplyMute();
    }

    private void ApplyMute()
    {
        AudioListener.volume = muted ? 0f : 1f;   // 0 = silence everything
        if (muteText != null)
            muteText.text = muted ? "Sound: OFF" : "Sound: ON";
    }
}