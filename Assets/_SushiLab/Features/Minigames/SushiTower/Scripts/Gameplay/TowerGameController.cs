using UnityEngine;
using TMPro;

public class TowerGameController : MonoBehaviour
{
    public int score = 0;
    public bool gameOver = false;

    public TowerSpawner towerSpawner;
    public TowerCamera towerCamera;
    public Collider2D platformCollider;
    public TMP_Text scoreText;
    public TowerUIController towerUI;

    public float highestSupportY;

    void Start()
    {
        highestSupportY = platformCollider.bounds.max.y;
        scoreText.text = "Puntos: 0";
    }

    public void AddPoint(float pieceTop)
    {
        if (gameOver)
            return;

        score++;
        scoreText.text = "Puntos: " + score;

        highestSupportY = Mathf.Max(
            highestSupportY, pieceTop
        );

        towerCamera.UpdateHeight(highestSupportY);

        Debug.Log("Puntuacion: " + score);
    }

    public void GameOver()
    {
        if (gameOver)
            return;

        gameOver = true;
        towerSpawner.StopSpawning();

        towerUI.ShowGameOver(score);

        Debug.Log("GAME OVER! Puntuacion final: " + score);
    }
}
