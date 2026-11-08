
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MemoryGameController : MonoBehaviour
{
    [Header("Referencias del tablero")]
    [SerializeField] private MemoryCard cardPrefab;
    [SerializeField] private Transform cardContainer;

    [Header("Cartas - Dificultad Facil")]
    [SerializeField] private List<Sprite> easyCardSprites = new List<Sprite>();

    [Header("Estado de la partida")]
    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool isCheckingPair = false;
    private int attempts = 0;
    private int matchedPairs = 0;

    [Header("Configuracion del tiempo")]
    [SerializeField, Min(1f)] private float timeLimitSeconds = 90f;

    private float remainingTime;
    [SerializeField] private TMP_Text timerText;

    [Header("Pantalla de resultados")]
    [SerializeField] private GameObject resultPanel;
    [Header("Pantalla de pausa")]
    [SerializeField] private GameObject pausePanel;

    [Header("Textos de resultados")]
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text resultSubtitleText;

    [Header("Estadisticas finales")]
    [SerializeField] private TMP_Text resultTimeText;
    [SerializeField] private TMP_Text resultAttemptsText;
    [SerializeField] private TMP_Text resultPairsText;



    private void Start()
    {
        remainingTime = timeLimitSeconds;

        UpdateTimerDisplay();
        GenerateBoard();
    }


    private void Update()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        remainingTime -= Time.deltaTime;


        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            currentState = GameState.Lost;

            Debug.Log("Sushi Memory: ¡DERROTA! Se termino el tiempo.");

            ShowResults(false);
        }


        UpdateTimerDisplay();
    }




    private void GenerateBoard()
    {
        if (cardPrefab == null || cardContainer == null)
        {
            Debug.LogError("Memory: faltan referencias del tablero.");
            return;
        }

        if (easyCardSprites.Count != 6)
        {
            Debug.LogError("Memory: el nivel Facil necesita 6 ilustraciones.");
            return;
        }

        // Crear dos cartas por cada ilustracion.
        List<int> cardIds = new List<int>();

        for (int i = 0; i < easyCardSprites.Count; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        // Mezclar las posiciones de las cartas.
        for (int i = cardIds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            int temp = cardIds[i];
            cardIds[i] = cardIds[randomIndex];
            cardIds[randomIndex] = temp;
        }

        // Generar las 12 cartas dentro del tablero.
        foreach (int pairId in cardIds)
        {
            MemoryCard newCard = Instantiate(cardPrefab, cardContainer);

            newCard.Initialize(
                pairId,
                easyCardSprites[pairId],
                OnCardSelected
            );
        }

        Debug.Log("Sushi Memory: tablero de 12 cartas generado.");
    }


    private void OnCardSelected(MemoryCard selectedCard)
    {
        // No permitir nuevas selecciones mientras se comprueba una pareja.
        if (isCheckingPair ||
            currentState == GameState.Paused ||
            currentState == GameState.Won ||
            currentState == GameState.Lost)
        {
            return;
        }

        // La partida comienza al descubrir la primera carta.
        if (currentState == GameState.Ready)
        {
            currentState = GameState.Playing;
        }

        // Registrar la primera carta.
        if (firstCard == null)
        {
            firstCard = selectedCard;
            firstCard.Reveal();
            return;
        }

        // Evitar seleccionar dos veces la misma carta.
        if (selectedCard == firstCard)
        {
            return;
        }

        // Registrar la segunda carta.
        secondCard = selectedCard;
        secondCard.Reveal();

        // Bloquear nuevas selecciones.
        isCheckingPair = true;

        // Contar el intento.
        attempts++;

        Debug.Log("Sushi Memory: intento " + attempts);

        StartCoroutine(CheckPair());
    }


    private IEnumerator CheckPair()
    {
        // Comprobar si ambas cartas pertenecen a la misma pareja.

        if (firstCard.PairId == secondCard.PairId)
        {
            Debug.Log("Sushi Memory: ¡pareja encontrada!");

            firstCard.MarkMatched();
            secondCard.MarkMatched();

            // Registrar una nueva pareja encontrada.
            matchedPairs++;

            Debug.Log("Parejas encontradas: " + matchedPairs + "/6");

            // Comprobar si todas las parejas fueron encontradas.

            if (matchedPairs == easyCardSprites.Count)
            {
                currentState = GameState.Won;

                Debug.Log("Sushi Memory: ¡VICTORIA! Todas las parejas encontradas.");

                ShowResults(true);
            }

        }


        else
        {
            Debug.Log("Sushi Memory: pareja incorrecta.");

            // Tiempo que falta para volver a ocultar las cartas.
            float remainingFlipDelay = 1f;

            // Esperar un segundo de juego activo, respetando la pausa.
            while (remainingFlipDelay > 0f ||
                   currentState == GameState.Paused)
            {
                // Si la partida terminó, cancelar la comparación.
                if (currentState == GameState.Lost ||
                    currentState == GameState.Won)
                {
                    yield break;
                }

                // Descontar tiempo solamente mientras estamos jugando.
                if (currentState == GameState.Playing)
                {
                    remainingFlipDelay -= Time.deltaTime;
                }

                // Continuar comprobando en el siguiente fotograma.
                yield return null;
            }

            // No modificar las cartas si la partida dejó de estar activa.
            if (currentState != GameState.Playing)
            {
                yield break;
            }

            // Ocultar las cartas incorrectas.
            firstCard.Hide();
            secondCard.Hide();
        }

        // Limpiar las selecciones para permitir otro turno.
        firstCard = null;
        secondCard = null;

        isCheckingPair = false;
    }


    private enum GameState
    {
        Ready,
        Playing,
        Paused,
        Won,
        Lost
    }

    private GameState currentState = GameState.Ready;
    private GameState stateBeforePause = GameState.Ready;


    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }


    private void ShowResults(bool isVictory)
    {
        // Configurar los mensajes segun el resultado.
        if (isVictory)
        {
            resultTitleText.text = "¡EXCELENTE!";
            resultSubtitleText.text = "¡Todas las parejas encontradas!";
        }
        else
        {
            resultTitleText.text = "¡SE ACABÓ EL TIEMPO!";
            resultSubtitleText.text =
                "¡Sigue practicando para encontrar todas las parejas!";
        }

        // Convertir el tiempo restante a minutos y segundos.
        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // Mostrar las estadisticas reales de la partida.
        resultTimeText.text =
            $"Tiempo restante: {minutes:00}:{seconds:00}";

        resultAttemptsText.text =
            $"Intentos realizados: {attempts}";

        resultPairsText.text =
            $"Parejas encontradas: {matchedPairs}/{easyCardSprites.Count}";

        // Mostrar el panel al finalizar la partida.
        resultPanel.SetActive(true);
    }


    public void RetryGame()
    {
        // Detener las comparaciones que pudieran quedar pendientes.
        StopAllCoroutines();

        // Eliminar las cartas de la partida anterior.
        foreach (Transform child in cardContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        // Reiniciar las selecciones.
        firstCard = null;
        secondCard = null;
        isCheckingPair = false;

        // Reiniciar las estadísticas.
        attempts = 0;
        matchedPairs = 0;

        // Reiniciar el cronómetro y el estado de la partida.
        remainingTime = timeLimitSeconds;
        currentState = GameState.Ready;

        UpdateTimerDisplay();

        // Ocultar los resultados.
        resultPanel.SetActive(false);

        // Crear un nuevo tablero mezclado.
        GenerateBoard();

        Debug.Log("Sushi Memory: nueva partida iniciada.");
    }

    public void ReturnToArcade()
    {
        Debug.Log("Sushi Memory: regresando al Arcade.");

        SceneManager.LoadScene("SC_Arcade");
    }


    public void PauseGame()
    {
        // No permitir pausar si la partida ya termino o esta pausada.
        if (currentState == GameState.Paused ||
            currentState == GameState.Won ||
            currentState == GameState.Lost)
        {
            return;
        }

        // Guardar el estado anterior.
        stateBeforePause = currentState;

        // Cambiar al estado de pausa.
        currentState = GameState.Paused;

        // Mostrar la ventana de pausa.
        pausePanel.SetActive(true);

        Debug.Log("Sushi Memory: partida pausada.");
    }

    public void ResumeGame()
    {
        // Solo se puede reanudar una partida pausada.
        if (currentState != GameState.Paused)
        {
            return;
        }

        // Recuperar el estado anterior de la partida.
        currentState = stateBeforePause;

        // Ocultar la ventana de pausa.
        pausePanel.SetActive(false);

        Debug.Log("Sushi Memory: partida reanudada.");
    }



}
