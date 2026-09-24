using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Ready, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed / difficulty (bonus)")]
    [Tooltip("Column + ground scroll speed at score 0 (world units / second).")]
    [SerializeField] private float baseSpeed = 3f;
    [Tooltip("Speed added for each point scored.")]
    [SerializeField] private float speedPerPoint = 0.05f;
    [SerializeField] private float maxSpeed = 6f;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject getReadyPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [Tooltip("Small pause before the Game Over screen, so the player sees the crash.")]
    [SerializeField] private float gameOverPanelDelay = 0.6f;

    [Header("Audio (bonus)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip scoreClip;
    [SerializeField] private AudioClip hitClip;

    [Header("FX (bonus)")]
    [SerializeField] private CameraShake cameraShake;

    private const string BestScoreKey = "BestScore";
    private bool canRestart;
    private Coroutine punchRoutine;

    public GameState State { get; private set; } = GameState.Ready;
    public int Score { get; private set; }

    public float CurrentSpeed =>
        State == GameState.GameOver ? 0f : Mathf.Min(baseSpeed + Score * speedPerPoint, maxSpeed);

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        Score = 0;
        UpdateScoreText();
        if (getReadyPanel) getReadyPanel.SetActive(true);
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (canRestart && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Restart();
    }

    public void StartGame()
    {
        if (State != GameState.Ready) return;
        State = GameState.Playing;
        if (getReadyPanel) getReadyPanel.SetActive(false);
    }

    public void AddScore()
    {
        if (State != GameState.Playing) return;
        Score++;
        UpdateScoreText();
        if (sfxSource && scoreClip) sfxSource.PlayOneShot(scoreClip);

        if (scoreText)
        {
            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(Punch(scoreText.transform));
        }
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;

        if (sfxSource && hitClip) sfxSource.PlayOneShot(hitClip);
        if (cameraShake) cameraShake.Shake();

        int best = Mathf.Max(Score, PlayerPrefs.GetInt(BestScoreKey, 0));
        PlayerPrefs.SetInt(BestScoreKey, best);
        PlayerPrefs.Save();

        Invoke(nameof(ShowGameOverPanel), gameOverPanelDelay);
    }

    private void ShowGameOverPanel()
    {
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (finalScoreText) finalScoreText.text = $"Score : {Score}";
        if (bestScoreText) bestScoreText.text = $"Best : {PlayerPrefs.GetInt(BestScoreKey, 0)}";
        if (gameOverPanel) gameOverPanel.SetActive(true);
        canRestart = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void UpdateScoreText()
    {
        if (scoreText) scoreText.text = Score.ToString();
    }

    private static IEnumerator Punch(Transform t)
    {
        const float duration = 0.15f;
        for (float e = 0f; e < duration; e += Time.deltaTime)
        {
            float s = 1f + 0.35f * Mathf.Sin(e / duration * Mathf.PI);
            t.localScale = Vector3.one * s;
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
