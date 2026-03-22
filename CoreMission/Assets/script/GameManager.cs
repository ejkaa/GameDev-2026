using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject finishPanel;
    public GameObject failPanel;

    public CrystalManager crystalManager;

    [Header("Canvas Groups")]
    public CanvasGroup startCanvasGroup;
    public CanvasGroup gameCanvasGroup;
    public CanvasGroup finishCanvasGroup;
    public CanvasGroup failCanvasGroup;

    [Header("UI Text")]
    public TMP_Text timerText;
    public TMP_Text finalTimeText;
    public TMP_Text countdownText;
    public TMP_Text recordTextStart;
    public TMP_Text recordTextFinish;
    public TMP_Text finishText;

    [Header("Player Control")]
    public MonoBehaviour playerControlScript;

    [Header("Settings")]
    public float fadeDuration = 0.35f;

    private float timer = 0f;
    private bool gameRunning = false;
    private bool gameFinished = false;
    private bool isNewRecord = false;
    private int collected;
    private const string RecordKey = "BestTime";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ShowStartScreenImmediate();
        RefreshStartRecordUI();
    }

    void Update()
    {
        if (gameRunning && !gameFinished)
        {
            timer += Time.deltaTime;

            if (timerText != null)
                timerText.text = "TIME " + FormatTime(timer);
        }
    }

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        timer = 0f;
        gameRunning = false;
        gameFinished = false;
        isNewRecord = false;

        if (playerControlScript != null)
            playerControlScript.enabled = false;

        yield return StartCoroutine(FadeOutPanel(startPanel, startCanvasGroup));

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            yield return StartCoroutine(PlayCountdown());
            countdownText.gameObject.SetActive(false);
        }

        gamePanel.SetActive(true);
        gameCanvasGroup.alpha = 0f;
        yield return StartCoroutine(FadeInPanel(gamePanel, gameCanvasGroup));

        if (playerControlScript != null)
            playerControlScript.enabled = true;

        timer = 0f;

        if (timerText != null)
            timerText.text = "TIME " + FormatTime(timer);

        gameRunning = true;
    }

    public void FinishGame()
    {
        if (gameFinished) return;
        StartCoroutine(EndGameRoutine(true));
    }

    public void LoseGame()
    {
        if (gameFinished) return;
        StartCoroutine(EndGameRoutine(false));
    }

    IEnumerator EndGameRoutine(bool won)
    {
        gameRunning = false;
        gameFinished = true;

        if (playerControlScript != null)
            playerControlScript.enabled = false;

        
        if (crystalManager != null)
        {
            collected = crystalManager.GetCrystal();
            Debug.Log("Collected crystals: " + collected);
        }

        if (won)
        {
            isNewRecord = SaveRecord(timer);

            if (finishText != null)
                finishText.text = "FINISH";

            if (finalTimeText != null)
                finalTimeText.text = "YOUR TIME\n" + FormatTime(timer);

            RefreshFinishRecordUI();
        }
        else
        {
            yield return StartCoroutine(FadeOutPanel(gamePanel, gameCanvasGroup));

            failPanel.SetActive(true);
            failCanvasGroup.alpha = 0f;
            yield return StartCoroutine(FadeInPanel(failPanel, failCanvasGroup));

            yield break;
        }

        yield return StartCoroutine(FadeOutPanel(gamePanel, gameCanvasGroup));

        finishPanel.SetActive(true);
        finishCanvasGroup.alpha = 0f;
        yield return StartCoroutine(FadeInPanel(finishPanel, finishCanvasGroup));
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    void ShowStartScreenImmediate()
    {
        timer = 0f;
        gameRunning = false;
        gameFinished = false;
        isNewRecord = false;

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        finishPanel.SetActive(false);
        failPanel.SetActive(false);

        startCanvasGroup.alpha = 1f;
        gameCanvasGroup.alpha = 0f;
        finishCanvasGroup.alpha = 0f;
        failCanvasGroup.alpha = 0f;

        if (playerControlScript != null)
            playerControlScript.enabled = false;

        if (timerText != null)
            timerText.text = "TIME 00:00:00";

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    IEnumerator PlayCountdown()
    {
        yield return StartCoroutine(ShowCountdownStep("3", 0.8f));
        yield return StartCoroutine(ShowCountdownStep("2", 0.8f));
        yield return StartCoroutine(ShowCountdownStep("1", 0.8f));
        yield return StartCoroutine(ShowCountdownStep("GO!", 0.7f));
    }

    IEnumerator ShowCountdownStep(string value, float duration)
    {
        countdownText.text = value;
        countdownText.transform.localScale = Vector3.one * 1.4f;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float scale = Mathf.Lerp(1.4f, 1f, t / duration);
            countdownText.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        countdownText.transform.localScale = Vector3.one;
    }

    IEnumerator FadeInPanel(GameObject panel, CanvasGroup group)
    {
        panel.SetActive(true);
        group.alpha = 0f;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;
    }

    IEnumerator FadeOutPanel(GameObject panel, CanvasGroup group)
    {
        group.alpha = 1f;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        group.alpha = 0f;
        panel.SetActive(false);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int hundredths = Mathf.FloorToInt((time * 100f) % 100f);

        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }

    bool SaveRecord(float newTime)
    {
        if (!PlayerPrefs.HasKey(RecordKey))
        {
            PlayerPrefs.SetFloat(RecordKey, newTime);
            PlayerPrefs.Save();
            return true;
        }

        float currentRecord = PlayerPrefs.GetFloat(RecordKey);

        if (newTime < currentRecord)
        {
            PlayerPrefs.SetFloat(RecordKey, newTime);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }

    void RefreshStartRecordUI()
    {
        if (recordTextStart == null) return;

        if (!PlayerPrefs.HasKey(RecordKey))
        {
            recordTextStart.gameObject.SetActive(false);
            return;
        }

        float record = PlayerPrefs.GetFloat(RecordKey);
        recordTextStart.gameObject.SetActive(true);
        recordTextStart.text = "Record: " + FormatTime(record);
    }

    void RefreshFinishRecordUI()
    {
        if (recordTextFinish == null) return;

        if (!PlayerPrefs.HasKey(RecordKey))
        {
            recordTextFinish.gameObject.SetActive(false);
            return;
        }

        float record = PlayerPrefs.GetFloat(RecordKey);
        recordTextFinish.gameObject.SetActive(true);

        if (isNewRecord)
            recordTextFinish.text = "New Record: " + FormatTime(record);
        else
            recordTextFinish.text = "Record: " + FormatTime(record);
    }

    public bool IsGameRunning()
    {
        return gameRunning && !gameFinished;
    }
}