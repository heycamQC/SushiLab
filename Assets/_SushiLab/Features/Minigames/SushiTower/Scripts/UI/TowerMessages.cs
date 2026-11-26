using UnityEngine;
using TMPro;

public class TowerMessages : MonoBehaviour
{
    public TMP_Text messageText;
    public TowerGameController gameController;

    public float messageInterval = 6f;

    private float timer;
    private int lastScore = -1;
    private int lastRandom = -1;

    private string[] randomMessages =
    {
        "¡Con cuidado, que se tambalea!",
        "¡Esa torre se ve deliciosa!",
        "¡Un sushi a la vez!",
        "¡Sigue así, chef!",
        "¡Qué buena puntería!",
        "¡No pierdas el equilibrio!"
    };

    void Start()
    {
        messageText.text = "¡Vamos, chef! Que no se caiga ningún sushi.";
        timer = 0f;
    }

    void Update()
    {
        if (gameController.gameOver)
            return;

        if (gameController.score != lastScore)
        {
            lastScore = gameController.score;

            if (lastScore > 0 && lastScore % 8 == 0)
            {
                messageText.text =
                    "¡Cuidado! Los sushis ahora van más rápido.";

                timer = 0f;
                return;
            }

            if (lastScore == 5)
            {
                messageText.text = "¡Cinco sushis! Vas muy bien.";
                timer = 0f;
                return;
            }

            if (lastScore == 24)
            {
                messageText.text = "¡Imparable! ¿Hasta dónde llegarás?";
                timer = 0f;
                return;
            }
        }

        timer += Time.deltaTime;

        if (timer >= messageInterval)
        {
            ShowRandomMessage();
            timer = 0f;
        }
    }

    void ShowRandomMessage()
    {
        int index;

        do
        {
            index = Random.Range(0, randomMessages.Length);
        }
        while (index == lastRandom);

        lastRandom = index;
        messageText.text = randomMessages[index];
    }
}
