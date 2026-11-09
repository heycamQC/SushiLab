using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MemoryGameController : MonoBehaviour
{
    public enum MemoryDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    private enum GameState
    {
        SelectingDifficulty,
        Ready,
        Playing,
        Paused,
        Won,
        Lost
    }

    [Header("Referencias del tablero")]
    [SerializeField] private MemoryCard cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GridLayoutGroup boardGrid;

    [Header("Cartas - Facil (6 ilustraciones)")]
    [SerializeField] private List<Sprite> easyCardSprites = new List<Sprite>();

    [Header("Cartas - Normal (8 ilustraciones)")]
    [SerializeField] private List<Sprite> normalCardSprites = new List<Sprite>();

    [Header("Cartas - Dificil (12 ilustraciones)")]
    [SerializeField] private List<Sprite> hardCardSprites = new List<Sprite>();

    [Header("Tiempo por dificultad (segundos)")]
    [SerializeField, Min(1f)] private float easyTimeLimitSeconds = 90f;
    [SerializeField, Min(1f)] private float normalTimeLimitSeconds = 100f;
    [SerializeField, Min(1f)] private float hardTimeLimitSeconds = 120f;

    [Header("Seleccion de dificultad")]
    [SerializeField] private GameObject difficultySelectionPanel;
    [SerializeField] private MemoryDifficulty defaultDifficulty = MemoryDifficulty.Easy;

    // Simulacion local del dia de Sushi Lab. El Arcade real se habilita el dia 3.
    // Core/Progression proporcionara este dato cuando exista la integracion.
    [Header("Progreso local de prueba (no conectado a Core)")]
    [SerializeField, Min(1)] private int testRestaurantDay = 3;

    [Header("Indicadores superiores (HUD)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text hudAttemptsText;
    [SerializeField] private TMP_Text hudPairsText;

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

    private MemoryCard firstCard;
    private MemoryCard secondCard;
    private bool isCheckingPair;
    private int attempts;
    private int matchedPairs;
    private int totalPairs;
    private float remainingTime;
    private List<Sprite> activeCardSprites;
    private MemoryDifficulty currentDifficulty = MemoryDifficulty.Easy;
    private GameState currentState = GameState.SelectingDifficulty;
    private GameState stateBeforePause = GameState.Ready;

    private void Start()
    {
        if (boardGrid == null && cardContainer != null)
        {
            boardGrid = cardContainer.GetComponent<GridLayoutGroup>();
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Mientras no exista el menu en la escena, Facil sigue funcionando
        // como antes. Cuando lo conectemos, el juego comenzara en el menu.
        if (difficultySelectionPanel != null)
        {
            currentState = GameState.SelectingDifficulty;
            difficultySelectionPanel.SetActive(true);
            difficultySelectionPanel.transform.SetAsLastSibling();
        }
        else
        {
            StartDifficulty(defaultDifficulty);
        }
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

    // Conectar estos metodos a los botones del menu mediante On Click().
    public void SelectEasy()
    {
        StartDifficulty(MemoryDifficulty.Easy);
    }

    public void SelectNormal()
    {
        StartDifficulty(MemoryDifficulty.Normal);
    }

    public void SelectHard()
    {
        StartDifficulty(MemoryDifficulty.Hard);
    }

    public void StartDifficulty(MemoryDifficulty difficulty)
    {
        if (!IsDifficultyUnlocked(difficulty))
        {
            Debug.Log($"Sushi Memory: dificultad {difficulty} bloqueada en el dia {testRestaurantDay}.");
            return;
        }

        List<Sprite> sprites;
        int requiredPairs;
        int columns;
        float limitSeconds;

        switch (difficulty)
        {
            case MemoryDifficulty.Normal:
                sprites = normalCardSprites;
                requiredPairs = 8;
                columns = 4;
                limitSeconds = normalTimeLimitSeconds;
                break;

            case MemoryDifficulty.Hard:
                sprites = hardCardSprites;
                requiredPairs = 12;
                columns = 6;
                limitSeconds = hardTimeLimitSeconds;
                break;

            default:
                sprites = easyCardSprites;
                requiredPairs = 6;
                columns = 4;
                limitSeconds = easyTimeLimitSeconds;
                break;
        }

        if (cardPrefab == null || cardContainer == null)
        {
            Debug.LogError("Sushi Memory: faltan Card Prefab o Card Container.");
            return;
        }

        if (sprites == null || sprites.Count != requiredPairs)
        {
            Debug.LogError(
                $"Sushi Memory: {difficulty} necesita {requiredPairs} ilustraciones distintas. " +
                "Completa su lista en el Inspector."
            );
            return;
        }

        HashSet<Sprite> uniqueSprites = new HashSet<Sprite>();
        foreach (Sprite sprite in sprites)
        {
            if (sprite == null || !uniqueSprites.Add(sprite))
            {
                Debug.LogError(
                    $"Sushi Memory: la lista {difficulty} contiene una ilustracion vacia o repetida."
                );
                return;
            }
        }

        StopAllCoroutines();
        ClearBoard();

        currentDifficulty = difficulty;
        activeCardSprites = sprites;
        totalPairs = requiredPairs;
        remainingTime = limitSeconds;
        attempts = 0;
        matchedPairs = 0;
        firstCard = null;
        secondCard = null;
        isCheckingPair = false;
        currentState = GameState.Ready;
        stateBeforePause = GameState.Ready;

        // Conservar el tamano de las cartas; cambiar solo las columnas.
        if (boardGrid != null)
        {
            boardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            boardGrid.constraintCount = columns;
            boardGrid.childAlignment = TextAnchor.MiddleCenter;
        }

        if (difficultySelectionPanel != null)
        {
            difficultySelectionPanel.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        UpdateTimerDisplay();
        UpdateHUDStats();
        GenerateBoard();

        Debug.Log($"Sushi Memory: inicio {currentDifficulty} ({totalPairs} parejas).");
    }

    private void GenerateBoard()
    {
        if (activeCardSprites == null)
        {
            return;
        }

        List<int> cardIds = new List<int>();
        for (int i = 0; i < activeCardSprites.Count; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        for (int i = cardIds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = cardIds[i];
            cardIds[i] = cardIds[randomIndex];
            cardIds[randomIndex] = temp;
        }

        foreach (int pairId in cardIds)
        {
            MemoryCard newCard = Instantiate(cardPrefab, cardContainer);
            newCard.Initialize(pairId, activeCardSprites[pairId], OnCardSelected);
        }
    }

    private void ClearBoard()
    {
        if (cardContainer == null)
        {
            return;
        }

        foreach (Transform child in cardContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    private void OnCardSelected(MemoryCard selectedCard)
    {
        if (isCheckingPair ||
            currentState == GameState.SelectingDifficulty ||
            currentState == GameState.Paused ||
            currentState == GameState.Won ||
            currentState == GameState.Lost)
        {
            return;
        }

        if (currentState == GameState.Ready)
        {
            currentState = GameState.Playing;
        }

        if (firstCard == null)
        {
            firstCard = selectedCard;
            firstCard.Reveal();
            return;
        }

        if (selectedCard == firstCard)
        {
            return;
        }

        secondCard = selectedCard;
        secondCard.Reveal();
        isCheckingPair = true;
        attempts++;
        UpdateHUDStats();
        StartCoroutine(CheckPair());
    }

    private IEnumerator CheckPair()
    {
        if (firstCard.PairId == secondCard.PairId)
        {
            firstCard.MarkMatched();
            secondCard.MarkMatched();
            matchedPairs++;
            UpdateHUDStats();

            if (matchedPairs == totalPairs)
            {
                currentState = GameState.Won;
                Debug.Log("Sushi Memory: ¡VICTORIA! Todas las parejas encontradas.");
                ShowResults(true);
            }
        }
        else
        {
            float remainingFlipDelay = 1f;

            while (remainingFlipDelay > 0f || currentState == GameState.Paused)
            {
                if (currentState == GameState.Lost ||
                    currentState == GameState.Won ||
                    currentState == GameState.SelectingDifficulty)
                {
                    yield break;
                }

                if (currentState == GameState.Playing)
                {
                    remainingFlipDelay -= Time.deltaTime;
                }

                yield return null;
            }

            if (currentState != GameState.Playing)
            {
                yield break;
            }

            firstCard.Hide();
            secondCard.Hide();
        }

        firstCard = null;
        secondCard = null;
        isCheckingPair = false;
    }

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

    private void UpdateHUDStats()
    {
        if (hudAttemptsText != null)
        {
            hudAttemptsText.text = attempts.ToString();
        }

        if (hudPairsText != null)
        {
            hudPairsText.text = $"{matchedPairs}/{totalPairs}";
        }
    }

    private void ShowResults(bool isVictory)
    {
        if (resultTitleText != null)
        {
            resultTitleText.text = isVictory ? "¡EXCELENTE!" : "¡SE ACABÓ EL TIEMPO!";
        }

        if (resultSubtitleText != null)
        {
            resultSubtitleText.text = isVictory
                ? "¡Todas las parejas encontradas!"
                : "¡Sigue practicando para encontrar todas las parejas!";
        }

        int totalSeconds = Mathf.CeilToInt(remainingTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (resultTimeText != null)
        {
            resultTimeText.text = $"Tiempo restante: {minutes:00}:{seconds:00}";
        }

        if (resultAttemptsText != null)
        {
            resultAttemptsText.text = $"Intentos realizados: {attempts}";
        }

        if (resultPairsText != null)
        {
            resultPairsText.text = $"Parejas encontradas: {matchedPairs}/{totalPairs}";
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    public void RetryGame()
    {
        StartDifficulty(currentDifficulty);
    }

    public void ShowDifficultySelection()
    {
        if (difficultySelectionPanel == null)
        {
            Debug.LogWarning("Sushi Memory: aun no se asigno el panel de dificultades.");
            return;
        }

        StopAllCoroutines();
        ClearBoard();
        firstCard = null;
        secondCard = null;
        isCheckingPair = false;
        currentState = GameState.SelectingDifficulty;

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        difficultySelectionPanel.SetActive(true);
        difficultySelectionPanel.transform.SetAsLastSibling();
    }

    // Se utiliza para activar/desactivar las opciones del menu cuando lo construyamos.
    // El dia se simula localmente hasta que Core/Progression este conectado.
    public bool IsDifficultyUnlocked(MemoryDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MemoryDifficulty.Normal:
                return testRestaurantDay >= 10;
            case MemoryDifficulty.Hard:
                return testRestaurantDay >= 20;
            default:
                return testRestaurantDay >= 3;
        }
    }

    // Punto de entrada provisional para recibir el dia desde Core en el futuro.
    // No gestiona ni guarda por su cuenta el progreso global.
    public void SetRestaurantDay(int day)
    {
        testRestaurantDay = Mathf.Max(1, day);
    }

    public void ReturnToArcade()
    {
        Debug.Log("Sushi Memory: regresando al Arcade.");
        SceneManager.LoadScene("SC_Arcade");
    }

    public void PauseGame()
    {
        if (currentState == GameState.SelectingDifficulty ||
            currentState == GameState.Paused ||
            currentState == GameState.Won ||
            currentState == GameState.Lost)
        {
            return;
        }

        stateBeforePause = currentState;
        currentState = GameState.Paused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
        {
            return;
        }

        currentState = stateBeforePause;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }
}
