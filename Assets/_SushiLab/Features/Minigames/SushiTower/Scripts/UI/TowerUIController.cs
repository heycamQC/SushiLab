
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TowerUIController : MonoBehaviour
{
    public GameObject instructionsPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    public TMP_Text finalScoreText;
    public TowerSpawner towerSpawner;

    public bool isPaused = false;
    public bool gameStarted = false;

    void Start()
    {
        Time.timeScale = 0f;

        gameStarted = false;
        isPaused = false;

        instructionsPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void StartGame()
    {
        instructionsPanel.SetActive(false);

        gameStarted = true;
        isPaused = false;

        Time.timeScale = 1f;
        towerSpawner.BeginGame();
    }

    public void ShowGameOver(int score)
    {
        finalScoreText.text = score.ToString();
        gameOverPanel.SetActive(true);
    }

    public void PauseGame()
    {
        if (!gameStarted)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void BackToInstructions()
    {
        RestartGame();
    }

    public void BackToArcade()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SC_Arcade");
    }
}