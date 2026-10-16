using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using StarterAssets;

// PC-PLA-P2 + PC-PLA-P3 – countdown, timer and end screen.
// Put it on an empty GameObject called "RaceManager" in the scene.
public class RaceManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject player;              // drag PlayerArmature here

    [Header("UI")]
    public TextMeshProUGUI countdownText;  // big text in the middle: 3, 2, 1, GO!
    public TextMeshProUGUI timerText;      // small text in a corner: 00:12.3
    public GameObject endPanel;            // panel shown at the end
    public TextMeshProUGUI resultText;     // text inside the end panel
    public TextMeshProUGUI bestTimeText;   // "BEST 00:18.2" in a corner (optional)
    public GameObject getReadyBanner;      // "GET READY!" shown during the countdown (optional)

    bool raceRunning = false;
    float startTime;

    void Start()
    {
        if (player == null) Debug.LogWarning("RaceManager: drag PlayerArmature into the Player field.");
        if (endPanel != null) endPanel.SetActive(false);
        SetText(timerText, "00:00.0");

        // show the best time saved on this computer
        float savedBest = PlayerPrefs.GetFloat("BestTime", 0);
        if (savedBest > 0) SetText(bestTimeText, "BEST " + FormatTime(savedBest));
        else SetText(bestTimeText, "BEST --:--");

        // the finish line gets its script by code (no need to change the prefab)
        GameObject finishLine = GameObject.Find("FinishLine");
        if (finishLine != null) finishLine.AddComponent<FinishLine>();
        else Debug.LogWarning("RaceManager: no FinishLine in the scene (add a Finish platform).");

        // nothing on screen before the countdown
        SetText(countdownText, "");
        if (getReadyBanner != null) getReadyBanner.SetActive(false);

        StartCoroutine(StartRace());
    }

    IEnumerator StartRace()
    {
        // the player cannot move during the intro and the countdown
        SetPlayerCanMove(false);

        // 1. camera flight around the level (if there is a LevelIntro in the scene)
        LevelIntro intro = FindObjectOfType<LevelIntro>();
        if (intro != null)
        {
            yield return StartCoroutine(intro.Play());
        }

        // 2. then the countdown
        yield return StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        StartLightsFX lights = FindObjectOfType<StartLightsFX>();
        if (getReadyBanner != null) getReadyBanner.SetActive(true);

        for (int number = 3; number > 0; number--)
        {
            SetText(countdownText, number.ToString());
            if (lights != null) lights.SetCountdown(number);
            yield return new WaitForSeconds(1);
        }

        // GO!
        SetText(countdownText, "GO!");
        if (getReadyBanner != null) getReadyBanner.SetActive(false);
        if (lights != null) lights.SetCountdown(0);
        SetPlayerCanMove(true);
        raceRunning = true;
        startTime = Time.time;

        yield return new WaitForSeconds(1);
        SetText(countdownText, "");
    }

    void Update()
    {
        if (raceRunning)
        {
            SetText(timerText, FormatTime(Time.time - startTime));
        }
    }

    // Called by FinishLine when the player crosses the line
    public void PlayerFinished()
    {
        if (!raceRunning) return; // only once
        raceRunning = false;

        float time = Time.time - startTime;

        // best time is saved on the computer
        float bestTime = PlayerPrefs.GetFloat("BestTime", 9999);
        bool newRecord = time < bestTime;
        if (newRecord)
        {
            bestTime = time;
            PlayerPrefs.SetFloat("BestTime", bestTime);
        }

        // stop the player and celebrate
        SetPlayerCanMove(false);
        if (player != null)
        {
            BeanAnimator bean = player.GetComponentInChildren<BeanAnimator>();
            if (bean != null) bean.Celebrate();
        }
        FinishFX finish = FindObjectOfType<FinishFX>();
        if (finish != null) finish.Celebrate();

        // end screen
        // big time, then the best time smaller (<size> and <color> are TextMeshPro tags)
        string result = FormatTime(time);
        result += "\n<size=40%>Best " + FormatTime(bestTime) + "</size>";
        if (newRecord) result += "\n<size=40%><color=#FFC93C>NEW RECORD!</color></size>";
        SetText(resultText, result);
        SetText(bestTimeText, "BEST " + FormatTime(bestTime));
        StartCoroutine(ShowEndPanel());
    }

    IEnumerator ShowEndPanel()
    {
        yield return new WaitForSeconds(2); // let the confetti play first
        if (endPanel != null) endPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // so we can click the buttons
        Cursor.visible = true;
    }

    // Button "Restart" on the end panel
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // freezes or unfreezes the player (turns the Starter Assets controller off / on)
    void SetPlayerCanMove(bool canMove)
    {
        if (player == null) return;

        // works if "player" is PlayerArmature or its parent
        ThirdPersonController controller = player.GetComponentInChildren<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogWarning("RaceManager: no ThirdPersonController found in " + player.name);
            return;
        }

        controller.enabled = canMove;
        Debug.Log("RaceManager: player can move = " + canMove);
    }

    // writes in a text only if it exists
    void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null) text.text = value;
    }

    // Button "Menu" on the end panel (the "Menu" scene will be made later)
    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // 12.345 seconds -> "00:12.3"
    string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        float rest = seconds % 60;
        return minutes.ToString("00") + ":" + rest.ToString("00.0");
    }
}