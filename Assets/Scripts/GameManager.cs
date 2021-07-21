using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MPack;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager ins;

    public Sprite webSprite;

    public TextMeshProUGUI timeText;
    public int gameTime;
    private int gameTimer;
    // private Timer oneSecondTimer = new Timer(0.1f);
    private Timer oneSecondTimer = new Timer(1);

    public Canvas pauseMenuCanvas;

    public SpriteRenderer[] eggs;
    public EggSpriteTimePair[] eggSpriteTimePairs;

    public CanvasGroup loseCanvasGroup;
    public CanvasGroup winCanvasGroup;
    private Timer fadeTimer = new Timer(1f);

    private void Awake()
    {
        ins = this;
        gameTimer = gameTime;
        ChangeTiemrText();
        pauseMenuCanvas.enabled = false;
        fadeTimer.Running = false;
    }

    private void ChangeTiemrText()
    {
        int minute = gameTimer / 60;
        int second = gameTimer %  60;
        timeText.text = string.Format("{0}:{1}", minute, second.ToString("D2"));
    }

    private void Update()
    {
        if (fadeTimer.Running)
        {
            if (fadeTimer.UpdateEnd)
            {
                loseCanvasGroup.alpha = 1;
                winCanvasGroup.alpha = 1;
                enabled = false;
                return;
            }

            loseCanvasGroup.alpha = fadeTimer.Progress;
            winCanvasGroup.alpha = fadeTimer.Progress;

            return;
        }
        if (oneSecondTimer.UpdateEnd)
        {
            oneSecondTimer.Reset();
            gameTimer -= 1;
            ChangeTiemrText();

            for (int i = 0; i < eggSpriteTimePairs.Length; i++)
            {
                if (gameTimer > eggSpriteTimePairs[i].time)
                {
                    for (int e = 0; e < eggs.Length; e++)
                        eggs[e].sprite = eggSpriteTimePairs[i].sprite;
                    break;
                }
            }

            if (gameTimer <= 0)
            {
                Win();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuCanvas.enabled) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
        pauseMenuCanvas.enabled = true;
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        pauseMenuCanvas.enabled = false;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Win()
    {
        if (fadeTimer.Running) return;
        winCanvasGroup.gameObject.SetActive(true);
        fadeTimer.Reset();
    }

    public void Lose()
    {
        if (fadeTimer.Running) return;

        PlayerController.ins.Failed();
        loseCanvasGroup.gameObject.SetActive(true);
        fadeTimer.Reset();
    }

    [System.Serializable]
    public struct EggSpriteTimePair
    {
        public Sprite sprite;
        public int time;

    }
}
