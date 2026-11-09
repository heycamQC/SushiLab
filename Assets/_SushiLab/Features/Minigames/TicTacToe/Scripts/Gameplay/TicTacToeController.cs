using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TicTacToeController : MonoBehaviour
{
    private enum MatchResult { PlayerWins, ComputerWins, Draw }

    [Header("Casillas UI (orden: izquierda a derecha, fila por fila)")]
    [SerializeField] private Button[] casillas = new Button[9];

    [Header("Sprites")]
    [SerializeField] private Sprite spriteJugador;
    [SerializeField] private Sprite spriteComputadora;

    [Header("Textos HUD")]
    [SerializeField] private TMP_Text textoEstado;
    [SerializeField] private TMP_Text textoProgreso; 

    [Header("Tiempos")]
    [SerializeField, Min(0.1f)] private float esperaComputadoraSegundos = 1f;
    [SerializeField, Min(0.1f)] private float pausaEntrePartidasSegundos = 2f;

    [Header("Serie de partidas")]
    [SerializeField, Min(1)] private int totalPartidas = 6;

    [Header("Panel de pausa")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button botonPausa;
    [SerializeField] private Button botonVolverArcade;
    [SerializeField] private Button botonVolverArcade2;
    [SerializeField] private Button botonResume;
    [SerializeField] private Button botonReintentar;

    [Header("Panel de resultados finales")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultTitulo;
    [SerializeField] private TMP_Text resultSubtitulo;
    [SerializeField] private TMP_Text resultEstadisticas;
    [SerializeField] private TMP_Text resultWin;
     [SerializeField] private TMP_Text resultLose;
      [SerializeField] private TMP_Text resultTie;

    // Estado del tablero
    private readonly int[] tablero = new int[9];
    private readonly Image[] imagenes = new Image[9];
    private bool partidaTerminada;
    private bool turnoJugador = true;
    private bool serieTerminada;

    // Corrutinas activas
    private Coroutine turnoComputadoraCoroutine;
    private Coroutine transicionCoroutine;

    // Contadores de la serie
    private int partidaActual = 1;
    private int victoriasJugador;
    private int victoriasComputadora;
    private int empates;

    private static readonly int[,] LineasGanadoras =
    {
        { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 },
        { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 },
        { 0, 4, 8 }, { 2, 4, 6 }
    };

    private void Awake()
    {
        if (casillas == null || casillas.Length != 9)
        {
            Debug.LogError("Asigna exactamente 9 botones UI al arreglo casillas.");
            enabled = false;
            return;
        }

        for (int i = 0; i < casillas.Length; i++)
        {
            if (casillas[i] == null)
            {
                Debug.LogError("Falta asignar el botón de la casilla " + i + ".");
                enabled = false;
                return;
            }

            imagenes[i] = casillas[i].GetComponent<Image>();
            if (imagenes[i] == null)
                imagenes[i] = casillas[i].GetComponentInChildren<Image>(true);

            casillas[i].transition = Selectable.Transition.None;

            int indice = i;
            casillas[i].onClick.RemoveAllListeners();
            casillas[i].onClick.AddListener(() => JugarEnCasilla(indice));

            LimpiarImagen(i);
        }

        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveAllListeners();
            botonPausa.onClick.AddListener(PauseGame);
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        if(botonReintentar != null)
        {
            botonReintentar.onClick.RemoveAllListeners();
            botonReintentar.onClick.AddListener(ReiniciarSerie);
        }
        if(botonVolverArcade != null)
        {
            botonVolverArcade.onClick.RemoveAllListeners();
            botonVolverArcade.onClick.AddListener(() => SceneManager.LoadScene("SC_Arcade"));

        
        }
        if(botonVolverArcade2 != null)
        {
            botonVolverArcade2.onClick.RemoveAllListeners();
            botonVolverArcade2.onClick.AddListener(() => SceneManager.LoadScene("SC_Arcade"));
        }
        if(botonResume != null)
        {
            botonResume.onClick.RemoveAllListeners();
            botonResume.onClick.AddListener(ResumeGame);
        }
    }

    private void Start()
    {
        IniciarSerie();
    }

    // ------------------------------------------------------------------
    // Flujo principal
    // ------------------------------------------------------------------

    private void IniciarSerie()
    {
        DetenerCorrutinas();
        victoriasJugador = 0;
        victoriasComputadora = 0;
        empates = 0;
        partidaActual = 1;
        serieTerminada = false;

        if (resultPanel != null) resultPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        IniciarPartida();
    }

    private void IniciarPartida()
    {
        DetenerCorrutinas();
        LimpiarTablero();
        partidaTerminada = false;
        turnoJugador = true;
        ActualizarProgreso();
        ActualizarEstado("Tu turno");
    }

    private void TerminarPartida(MatchResult result)
    {
        partidaTerminada = true;

        switch (result)
        {
            case MatchResult.PlayerWins:  victoriasJugador++;     ActualizarEstado("¡Ganaste");    break;
            case MatchResult.ComputerWins: victoriasComputadora++; ActualizarEstado("Ganó la IA");  break;
            case MatchResult.Draw:        empates++;              ActualizarEstado("Empate");      break;
        }

        if (partidaActual >= totalPartidas)
        {
            serieTerminada = true;
            transicionCoroutine = StartCoroutine(FinalizarSerie());
        }
        else
        {
            transicionCoroutine = StartCoroutine(SiguientePartida());
        }
    }

    private IEnumerator SiguientePartida()
    {
        yield return new WaitForSeconds(pausaEntrePartidasSegundos);
        partidaActual++;
        IniciarPartida();
        transicionCoroutine = null;
    }

    private IEnumerator FinalizarSerie()
    {
        yield return new WaitForSeconds(pausaEntrePartidasSegundos);
        MostrarResumenFinal();
        transicionCoroutine = null;
    }

    private void MostrarResumenFinal()
    {
        if (resultTitulo != null) resultTitulo.text = "Serie terminada";
        if (resultSubtitulo != null) resultSubtitulo.text = "Buen trabajo";
        if (resultTie != null && resultWin != null && resultLose != null)
        {
            resultWin.text = $"Juegos Ganados: {victoriasJugador}";
            resultLose.text = $"Juegos Perdidos: {victoriasComputadora}";
            resultTie.text = $"Juegos Empatados: {empates}";
        }
        if (resultPanel != null) resultPanel.SetActive(true);
        ActualizarEstado("Fin de la serie");
    }

    // ------------------------------------------------------------------
    // Turnos
    // ------------------------------------------------------------------

    private void JugarEnCasilla(int indice)
    {
        if (partidaTerminada || serieTerminada || !turnoJugador || tablero[indice] != 0)
            return;

        ColocarFicha(indice, 1);
        if (ComprobarFin())
            return;

        turnoJugador = false;
        ActualizarEstado("Pensando…");
        turnoComputadoraCoroutine = StartCoroutine(TurnoComputadora());
    }

    private IEnumerator TurnoComputadora()
    {
        yield return new WaitForSeconds(esperaComputadoraSegundos);

        if (partidaTerminada)
        {
            turnoComputadoraCoroutine = null;
            yield break;
        }

        int mejorMovimiento = ObtenerMejorMovimiento();

        if (mejorMovimiento >= 0)
            ColocarFicha(mejorMovimiento, 2);

        if (ComprobarFin())
        {
            turnoComputadoraCoroutine = null;
            yield break;
        }

        turnoJugador = true;
        ActualizarEstado("Tu turno");
        turnoComputadoraCoroutine = null;
    }

    private void ColocarFicha(int indice, int jugador)
    {
        tablero[indice] = jugador;

        if (imagenes[indice] == null)
        {
            Debug.LogError("La casilla " + indice + " necesita un componente Image.");
            return;
        }

        imagenes[indice].sprite = jugador == 1 ? spriteJugador : spriteComputadora;
        imagenes[indice].color = Color.white;
        imagenes[indice].preserveAspect = true;
        imagenes[indice].enabled = true;
    }

    private void LimpiarImagen(int indice)
    {
        if (imagenes[indice] == null)
            return;

        imagenes[indice].sprite = null;
        imagenes[indice].color = new Color(1f, 1f, 1f, 0f);
    }

    private void LimpiarTablero()
    {
        for (int i = 0; i < tablero.Length; i++)
        {
            tablero[i] = 0;
            LimpiarImagen(i);
            if (casillas[i] != null)
                casillas[i].interactable = true;
        }
    }

    // ------------------------------------------------------------------
    // IA Minimax
    // ------------------------------------------------------------------

    private int ObtenerMejorMovimiento()
    {
        int mejorPuntaje = int.MinValue;
        int mejorIndice = -1;

        for (int i = 0; i < tablero.Length; i++)
        {
            if (tablero[i] != 0) continue;

            tablero[i] = 2;
            int puntaje = Minimax(0, false);
            tablero[i] = 0;

            if (puntaje > mejorPuntaje)
            {
                mejorPuntaje = puntaje;
                mejorIndice = i;
            }
        }

        return mejorIndice;
    }

    private int Minimax(int profundidad, bool maximizaComputadora)
    {
        int ganador = ObtenerGanador();

        if (ganador == 2) return 10 - profundidad;
        if (ganador == 1) return profundidad - 10;
        if (TableroLleno()) return 0;

        if (maximizaComputadora)
        {
            int mejor = int.MinValue;
            for (int i = 0; i < tablero.Length; i++)
            {
                if (tablero[i] != 0) continue;
                tablero[i] = 2;
                int puntaje = Minimax(profundidad + 1, false);
                tablero[i] = 0;
                mejor = Mathf.Max(mejor, puntaje);
            }
            return mejor;
        }
        else
        {
            int mejor = int.MaxValue;
            for (int i = 0; i < tablero.Length; i++)
            {
                if (tablero[i] != 0) continue;
                tablero[i] = 1;
                int puntaje = Minimax(profundidad + 1, true);
                tablero[i] = 0;
                mejor = Mathf.Min(mejor, puntaje);
            }
            return mejor;
        }
    }

    private int ObtenerGanador()
    {
        for (int i = 0; i < LineasGanadoras.GetLength(0); i++)
        {
            int a = LineasGanadoras[i, 0];
            int b = LineasGanadoras[i, 1];
            int c = LineasGanadoras[i, 2];

            if (tablero[a] != 0 && tablero[a] == tablero[b] && tablero[b] == tablero[c])
                return tablero[a];
        }
        return 0;
    }

    private bool TableroLleno()
    {
        for (int i = 0; i < tablero.Length; i++)
            if (tablero[i] == 0) return false;
        return true;
    }

    private bool ComprobarFin()
    {
        int ganador = ObtenerGanador();

        if (ganador == 1) { TerminarPartida(MatchResult.PlayerWins);   return true; }
        if (ganador == 2) { TerminarPartida(MatchResult.ComputerWins); return true; }
        if (TableroLleno()) { TerminarPartida(MatchResult.Draw);       return true; }

        return false;
    }

    // ------------------------------------------------------------------
    // UI
    // ------------------------------------------------------------------

    private void ActualizarEstado(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
    }

    private void ActualizarProgreso()
    {
        if (textoProgreso != null)
            textoProgreso.text = $"Partida {partidaActual}/{totalPartidas}";
    }

    // ------------------------------------------------------------------
    // Pausa
    // ------------------------------------------------------------------

    public void PauseGame()
    {
        if (serieTerminada || partidaTerminada) return;

        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    
    public void ReiniciarSerie()
    {
        Time.timeScale = 1f;
        IniciarSerie();
    }

    private void DetenerCorrutinas()
    {
        if (turnoComputadoraCoroutine != null)
        {
            StopCoroutine(turnoComputadoraCoroutine);
            turnoComputadoraCoroutine = null;
        }
        if (transicionCoroutine != null)
        {
            StopCoroutine(transicionCoroutine);
            transicionCoroutine = null;
        }
    }
}