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
    public GameObject hud;                   // the in-game text (souls, lives)

    [Header("HUD")]
    public TMP_Text livesText;               // shows "Lives: 3   HP: 3"

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
            StartGame();                     // came from Restart: jump straight in
        }
    }

    private void Update()
    {
        // HUD: keep lives up to date (health is shown by the HealthBar at the top)
        if (livesText != null)
            livesText.text = "Lives: " + player.Lives;

        // Esc opens/closes the pause menu, only while playing
        if (Keyboard.current.escapeKey.wasPressedThisFrame && playing)
        {
            if (paused) Resume();
            else Pause();
        }
    }

    // ---------- Buttons call these (Button > On Click) ----------

    public void StartGame()
    {
        HideAll();
        hud.SetActive(true);
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
        hud.SetActive(false);
    }

    // Stops time and turns off player input, so you can't jump or dash behind a menu
    private void Freeze(bool frozen)
    {
        Time.timeScale = frozen ? 0f : 1f;
        movement.enabled = !frozen;
    }
}