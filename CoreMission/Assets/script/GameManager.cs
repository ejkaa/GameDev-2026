using UnityEngine;
using UnityEngine.UI;
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
    public GameObject starsPanel;

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

    [Header("Star UI")]
    public Image star1Image;
    public Image star2Image;
    public Image star3Image;
    public Sprite starEmptySprite;
    public Sprite starFilledSprite;

    [Header("Game Stars UI")]
    public Image gameStar1;
    public Image gameStar2;
    public Image gameStar3;

    [Header("Star Animation")]
    public float starPopDuration = 0.35f;
    public float starDelay = 0.25f;

    [Header("Player Control")]
    public MonoBehaviour playerControlScript;

    [Header("Settings")]
    public float fadeDuration = 0.35f;

    private float timer = 0f;
    private bool gameRunning = false;
    private bool gameFinished = false;
    private bool isNewRecord = false;

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
        SetStarDisplay(0);
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
        star1Image.transform.localScale = Vector3.zero;
        star2Image.transform.localScale = Vector3.zero;
        star3Image.transform.localScale = Vector3.zero;

        SetStarDisplay(0);
        starsPanel.SetActive(false);
        UpdateGameStars(0);

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

        int collected = 0;

        if (crystalManager != null)
            collected = crystalManager.GetCrystal();

        if (won)
            collected += 1;

        collected = Mathf.Clamp(collected, 0, 3);

        SetStarDisplay(collected);
        starsPanel.SetActive(true);

        if (won)
        {
            isNewRecord = SaveRecord(timer);

            if (finishText != null)
                finishText.text = "CONGRATS!";

            if (finalTimeText != null)
                finalTimeText.text = FormatTime(timer);

            RefreshFinishRecordUI();
        }

        yield return StartCoroutine(FadeOutPanel(gamePanel, gameCanvasGroup));

        if (won)
        {
            finishPanel.SetActive(true);
            finishCanvasGroup.alpha = 0f;
            yield return StartCoroutine(FadeInPanel(finishPanel, finishCanvasGroup));
        }
        else
        {
            failPanel.SetActive(true);
            failCanvasGroup.alpha = 0f;
            yield return StartCoroutine(FadeInPanel(failPanel, failCanvasGroup));
        }
    }

    void SetStarDisplay(int collected)
    {
        StartCoroutine(AnimateStars(collected));
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
        starsPanel.SetActive(false);

        startCanvasGroup.alpha = 1f;
        gameCanvasGroup.alpha = 0f;
        finishCanvasGroup.alpha = 0f;
        failCanvasGroup.alpha = 0f;

        if (playerControlScript != null)
            playerControlScript.enabled = false;

        if (timerText != null)
            timerText.text = "00:00:00";

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    IEnumerator AnimateStars(int collected)
    {
        collected = Mathf.Clamp(collected, 0, 3);

        Image[] stars = new Image[] { star1Image, star2Image, star3Image };

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = starEmptySprite;
            stars[i].transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < collected; i++)
        {
            stars[i].sprite = starFilledSprite;

            yield return StartCoroutine(PopStar(stars[i]));

            yield return new WaitForSeconds(starDelay);
        }
    }

    IEnumerator PopStar(Image star)
    {
        float t = 0f;

        while (t < starPopDuration)
        {
            t += Time.deltaTime;

            float progress = t / starPopDuration;

            float scale = Mathf.Lerp(0f, 1.2f, progress);
            star.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        // bounce back
        float bounceTime = 0.15f;
        t = 0f;

        while (t < bounceTime)
        {
            t += Time.deltaTime;

            float progress = t / bounceTime;
            float scale = Mathf.Lerp(1.2f, 1f, progress);

            star.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        star.transform.localScale = Vector3.one;
    }

    public void UpdateGameStars(int collected)
    {
        collected = Mathf.Clamp(collected, 0, 3);

        if (gameStar1 != null)
            gameStar1.sprite = collected >= 1 ? starFilledSprite : starEmptySprite;

        if (gameStar2 != null)
            gameStar2.sprite = collected >= 2 ? starFilledSprite : starEmptySprite;

        if (gameStar3 != null)
            gameStar3.sprite = collected >= 3 ? starFilledSprite : starEmptySprite;
    }

    IEnumerator PlayCountdown()
    {
        yield return StartCoroutine(ShowCountdownStep("READY?", 1.0f));
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
        recordTextStart.text = "Record\n" + FormatTime(record);
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
        {
            recordTextFinish.text = "New Record!";
            recordTextFinish.color = Color.red;
        }
        else
            recordTextFinish.text = "Record: " + FormatTime(record);
    }

    public void ResetRecord()
    {
        PlayerPrefs.DeleteKey(RecordKey);
        PlayerPrefs.Save();

        RefreshStartRecordUI();
    }

    public bool IsGameRunning()
    {
        return gameRunning && !gameFinished;
    }
}