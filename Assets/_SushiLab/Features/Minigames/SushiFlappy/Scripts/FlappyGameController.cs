
using UnityEngine;
using UnityEngine.SceneManagement;

// Estados posibles de Flappy Sushi.
public enum FlappyGameState
{
    Ready,
    Playing,
    Paused,
    GameOver
}

public class FlappyGameController : MonoBehaviour
{
    public static FlappyGameController Instance
    {
        get;
        private set;
    }

    [Header("Referencias")]
    [SerializeField]
    private FlappyController player;

    [SerializeField]
    private FlappyAudio audioManager;

    [Header("Interfaz temporal")]
    [SerializeField]
    private bool showTestUI = false;

    private const string BestScoreKey =
        "SushiLab_FlappySushi_BestScore";

    // Permite reiniciar sin regresar al menu.
    private static bool nextRestartWithoutMenu = false;

    public FlappyGameState State
    {
        get;
        private set;
    } = FlappyGameState.Ready;

    public bool ShowIntroOnReady
    {
        get;
        private set;
    } = true;

    public int Score
    {
        get;
        private set;
    }

    public int BestScore
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ShowIntroOnReady = !nextRestartWithoutMenu;
        nextRestartWithoutMenu = false;

        BestScore = PlayerPrefs.GetInt(
            BestScoreKey,
            0
        );
    }

    private void Start()
    {
        Time.timeScale = 1f;

        Score = 0;
        State = FlappyGameState.Ready;

        if (player != null)
        {
            player.PrepareForStart();
        }
        else
        {
            Debug.LogError(
                "Flappy Sushi: asigna Player al GameController."
            );
        }
    }

    // =========================================
    // INICIAR PARTIDA
    // =========================================

    public void StartRun()
    {
        if (State != FlappyGameState.Ready)
        {
            return;
        }

        Time.timeScale = 1f;
        State = FlappyGameState.Playing;

        if (player != null)
        {
            player.ActivatePhysics();
        }
    }

    // =========================================
    // PAUSAR
    // =========================================

    public void PauseRun()
    {
        if (State != FlappyGameState.Playing)
        {
            return;
        }

        State = FlappyGameState.Paused;
        Time.timeScale = 0f;
    }

    // =========================================
    // CONTINUAR
    // =========================================

    public void ResumeRun()
    {
        if (State != FlappyGameState.Paused)
        {
            return;
        }

        Time.timeScale = 1f;
        State = FlappyGameState.Playing;
    }

    // =========================================
    // SUMAR PUNTOS
    // =========================================

    public void AddScore()
    {
        if (State != FlappyGameState.Playing)
        {
            return;
        }

        Score++;

        if (Score > BestScore)
        {
            BestScore = Score;

            PlayerPrefs.SetInt(
                BestScoreKey,
                BestScore
            );

            PlayerPrefs.Save();
        }

        if (audioManager != null)
        {
            audioManager.PlayPoint();
        }

        Debug.Log(
            "Flappy Sushi - Puntos: " + Score
        );
    }

    // =========================================
    // GAME OVER
    // =========================================

    public void EndRun()
    {
        if (State != FlappyGameState.Playing)
        {
            return;
        }

        State = FlappyGameState.GameOver;
        Time.timeScale = 1f;

        if (player != null)
        {
            player.StopPhysics();
        }

        if (audioManager != null)
        {
            audioManager.PlayHit();
        }

        Debug.Log(
            "Flappy Sushi - Game Over. Puntos: " +
            Score
        );
    }

    // =========================================
    // SONIDO DEL SALTO
    // =========================================

    public void PlayFlapSound()
    {
        if (audioManager != null)
        {
            audioManager.PlayFlap();
        }
    }

    // =========================================
    // REINTENTAR
    // =========================================

    public void Restart()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        if (currentScene.buildIndex < 0)
        {
            Debug.LogWarning(
                "Flappy Sushi: agrega MG_SushiFlappy " +
                "a Build Profiles > Scene List."
            );

            return;
        }

        // La siguiente partida no mostrara
        // el panel FLAPPY SUSHI.
        nextRestartWithoutMenu = true;

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================================
    // INTERFAZ DE PRUEBA
    // =========================================

    private void OnGUI()
    {
        if (!showTestUI)
        {
            return;
        }

        GUI.Label(
            new Rect(10, 10, 380, 40),
            "Flappy Sushi | " +
            State +
            " | Puntos: " +
            Score
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
