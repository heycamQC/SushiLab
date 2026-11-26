
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class FlappyUIController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField]
    private GameObject panelInicio;

    [SerializeField]
    private GameObject panelGameplay;

    [SerializeField]
    private GameObject panelGameOver;

    [SerializeField]
    private GameObject panelPausa;

    [Header("Botones")]
    [SerializeField]
    private Button btnJugar;

    [SerializeField]
    private Button btnReintentar;

    [SerializeField]
    private Button btnVolver;

    [SerializeField]
    private Button btnPausa;

    [SerializeField]
    private Button btnContinuar;

    [Header("Textos")]
    [SerializeField]
    private TMP_Text txtPuntos;

    [SerializeField]
    private TMP_Text txtResultado;

    [SerializeField]
    private TMP_Text txtRecord;

    [SerializeField]
    private TMP_Text txtTocaParaEmpezar;

    [Header("Primer salto al pulsar Jugar")]
    [SerializeField]
    private Rigidbody2D playerBody;

    [SerializeField]
    private float initialFlapSpeed = 5.5f;

    private FlappyGameState lastState;
    private bool lastShowIntro;
    private bool initialized;

    // =========================================
    // CONECTAR BOTONES
    // =========================================

    private void Awake()
    {
        if (btnJugar != null)
        {
            btnJugar.onClick.AddListener(Jugar);
        }

        if (btnReintentar != null)
        {
            btnReintentar.onClick.AddListener(Reintentar);
        }

        if (btnVolver != null)
        {
            btnVolver.onClick.AddListener(Volver);
        }

        if (btnPausa != null)
        {
            btnPausa.onClick.AddListener(Pausar);
        }

        if (btnContinuar != null)
        {
            btnContinuar.onClick.AddListener(Continuar);
        }
    }

    private void Start()
    {
        ActualizarInterfaz(true);
    }

    private void Update()
    {
        ActualizarInterfaz();
    }

    // =========================================
    // ACTUALIZAR PANELES
    // =========================================

    private void ActualizarInterfaz(bool force = false)
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        FlappyGameState state = game.State;

        bool showIntro = game.ShowIntroOnReady;

        if (
            force ||
            !initialized ||
            state != lastState ||
            showIntro != lastShowIntro
        )
        {
            bool isIntro =
                state == FlappyGameState.Ready &&
                showIntro;

            bool isGameplay =
                state == FlappyGameState.Playing ||
                state == FlappyGameState.Paused ||
                (
                    state == FlappyGameState.Ready &&
                    !showIntro
                );

            bool isGameOver =
                state == FlappyGameState.GameOver;

            bool isPaused =
                state == FlappyGameState.Paused;

            if (panelInicio != null)
            {
                panelInicio.SetActive(isIntro);
            }

            if (panelGameplay != null)
            {
                panelGameplay.SetActive(isGameplay);
            }

            if (panelGameOver != null)
            {
                panelGameOver.SetActive(isGameOver);
            }

            if (panelPausa != null)
            {
                panelPausa.SetActive(isPaused);
            }

            if (btnPausa != null)
            {
                btnPausa.interactable =
                    state == FlappyGameState.Playing;
            }

            if (txtTocaParaEmpezar != null)
            {
                txtTocaParaEmpezar.gameObject.SetActive(
                    state == FlappyGameState.Ready &&
                    !showIntro
                );
            }

            lastState = state;
            lastShowIntro = showIntro;
            initialized = true;
        }

        // =====================================
        // PUNTOS EN PANTALLA
        // =====================================

        if (txtPuntos != null)
        {
            txtPuntos.text =
                "Puntos: " + game.Score;
        }

        // =====================================
        // RESULTADOS DEL GAME OVER
        // =====================================

        if (state == FlappyGameState.GameOver)
        {
            if (txtResultado != null)
            {
                txtResultado.text =
                    "Puntos: " + game.Score;
            }

            if (txtRecord != null)
            {
                txtRecord.text =
                    "Récord: " + game.BestScore;
            }
        }
    }

    // =========================================
    // BOTON JUGAR
    // =========================================

    public void Jugar()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (
            game == null ||
            game.State != FlappyGameState.Ready
        )
        {
            return;
        }

        game.StartRun();

        if (playerBody != null)
        {
            playerBody.linearVelocity =
                new Vector2(
                    0f,
                    initialFlapSpeed
                );

            game.PlayFlapSound();
        }

        ActualizarInterfaz(true);
    }

    // =========================================
    // BOTON PAUSA
    // =========================================

    public void Pausar()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        game.PauseRun();

        ActualizarInterfaz(true);
    }

    // =========================================
    // BOTON CONTINUAR
    // =========================================

    public void Continuar()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        game.ResumeRun();

        ActualizarInterfaz(true);
    }

    // =========================================
    // BOTON REINTENTAR
    // =========================================

    public void Reintentar()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game != null)
        {
            game.Restart();
        }
    }

    // =========================================
    // BOTON VOLVER
    // =========================================

    public void Volver()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("SC_Arcade");
    }

    // =========================================
    // LIMPIAR EVENTOS
    // =========================================

    private void OnDestroy()
    {
        if (btnJugar != null)
        {
            btnJugar.onClick.RemoveListener(Jugar);
        }

        if (btnReintentar != null)
        {
            btnReintentar.onClick.RemoveListener(Reintentar);
        }

        if (btnVolver != null)
        {
            btnVolver.onClick.RemoveListener(Volver);
        }

        if (btnPausa != null)
        {
            btnPausa.onClick.RemoveListener(Pausar);
        }

        if (btnContinuar != null)
        {
            btnContinuar.onClick.RemoveListener(Continuar);
        }
    }
}
