using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

// One manager for every screen: title, pause, game over, level complete, and the HUD.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;      // lets other scripts call GameManager.instance.Win() etc.

    [Header("Screens (panels inside the Canvas)")]
    public GameObject titlePanel;

    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject controlsPanel;         // "How to Play" screen, shown after pressing Play
    public GameObject hud;                   // the in-game UI (health bar, lives, souls, objective)

    [Header("HUD")]
    public TMP_Text livesText;               // shows "Lives: 3"

    public PlayerHealth player;              // drag the Player here

    // static = survives a scene reload. True only the first time, so Restart skips the title.
    private static bool showTitle = true;

    private bool playing;                    // true while the player is actually playing
    private bool paused;
    private PlayerMovement movement;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        movement = player.GetComponent<PlayerMovement>();

        HideAll();
        if (showTitle)
        {
            titlePanel.SetActive(true);
            Freeze(true);                    // game waits behind the title
        }
        else
        {
            BeginPlaying();                  // came from Restart: skip the title and the controls
        }
    }

    private void Update()
    {
        // HUD: keep lives up to date (health is shown by the HealthBar at the top)
        if (livesText != null)
            livesText.text = "Lives: " + SoulCounter.Number(player.Lives);   // number in the normal font (see SoulCounter)

        // Esc opens/closes the pause menu, only while playing
        if (Keyboard.current.escapeKey.wasPressedThisFrame && playing)
        {
            if (paused) Resume();
            else Pause();
        }

        // On the controls screen, Enter works like the Start button
        if (controlsPanel != null && controlsPanel.activeSelf
            && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            BeginPlaying();
    }

    // ---------- Buttons call these (Button > On Click) ----------

    // Title screen > Play: show the controls first (the game stays frozen behind them)
    public void StartGame()
    {
        if (controlsPanel == null)
        {
            BeginPlaying();                  // no controls screen made yet: just play
            return;
        }
        HideAll();
        controlsPanel.SetActive(true);
    }

    // Controls screen > Start (or Enter): the game really starts
    public void BeginPlaying()
    {
        HideAll();
        hud.SetActive(true);                 // health bar, lives, souls and the objective
        playing = true;
        Freeze(false);
    }

    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);
        Freeze(true);
    }

    public void Resume()
    {
        paused = false;
        pausePanel.SetActive(false);
        Freeze(false);
    }

    public void Restart()
    {
        showTitle = false;                   // skip the title after reloading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitToTitle()
    {
        showTitle = true;                    // show the title after reloading
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // in the editor: stop Play mode
#else
        Application.Quit();                                 // in the built game: close the window
#endif
    }

    // ---------- Other scripts call these ----------

    public void GameOver()                   // PlayerHealth calls this when lives hit 0
    {
        playing = false;
        hud.SetActive(false);
        gameOverPanel.SetActive(true);
        Freeze(true);
    }

    public void Win()                        // LevelGoal calls this when the player reaches the end
    {
        playing = false;
        hud.SetActive(false);
        winPanel.SetActive(true);
        Freeze(true);
    }

    // ---------- Helpers ----------

    private void HideAll()
    {
        titlePanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        hud.SetActive(false);
    }

    // Stops time and turns off player input, so you can't jump or dash behind a menu
    private void Freeze(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;
        movement.enabled = !frozen;
    }
}