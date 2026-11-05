using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class TicTacToeController : MonoBehaviour
{
    [Header("Casillas UI (orden: izquierda a derecha, fila por fila)")]
    [SerializeField] private Button[] casillas = new Button[9];

    [Header("Sprites")]
    [SerializeField] private Sprite spriteJugador;
    [SerializeField] private Sprite spriteComputadora;

    [Header("Opcional")]
    [SerializeField] private TMP_Text textoEstado;

    private readonly int[] tablero = new int[9];
    private readonly Image[] imagenes = new Image[9];
    private bool partidaTerminada;
    private bool turnoJugador = true;

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
            Debug.LogError("Asigna exactamente 9 botones UI al arreglo Casillas.");
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

            // Evita que Unity oscurezca o desvanezca el sprite al pulsar/desactivar.
            casillas[i].transition = Selectable.Transition.None;

            int indice = i;
            casillas[i].onClick.RemoveAllListeners();
            casillas[i].onClick.AddListener(() => JugarEnCasilla(indice));

            LimpiarImagen(i);
        }

        ActualizarEstado("Tu turno");
    }

    private void JugarEnCasilla(int indice)
    {
        if (partidaTerminada || !turnoJugador || tablero[indice] != 0)
            return;

        ColocarFicha(indice, 1);
        if (ComprobarFin())
            return;

        turnoJugador = false;
        ActualizarEstado("La computadora está pensando...");
        int mejorMovimiento = ObtenerMejorMovimiento();

        if (mejorMovimiento >= 0)
            ColocarFicha(mejorMovimiento, 2);

        if (ComprobarFin())
            return;

        turnoJugador = true;
        ActualizarEstado("Tu turno");
    }

    private void ColocarFicha(int indice, int jugador)
    {
        tablero[indice] = jugador;

        if (imagenes[indice] == null)
        {
            Debug.LogError("La casilla " + indice + " necesita un componente Image para mostrar el sprite.");
            return;
        }

        imagenes[indice].sprite = jugador == 1 ? spriteJugador : spriteComputadora;
        imagenes[indice].color = Color.white; // Mantiene la opacidad original del sprite.
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

    // Minimax: la computadora maximiza el puntaje y el jugador minimiza.
    private int ObtenerMejorMovimiento()
    {
        int mejorPuntaje = int.MinValue;
        int mejorIndice = -1;

        for (int i = 0; i < tablero.Length; i++)
        {
            if (tablero[i] != 0)
                continue;

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
            if (tablero[i] == 0)
                return false;

        return true;
    }

    private bool ComprobarFin()
    {
        int ganador = ObtenerGanador();

        if (ganador == 1)
        {
            partidaTerminada = true;
            ActualizarEstado("¡Ganaste!");
            return true;
        }

        if (ganador == 2)
        {
            partidaTerminada = true;
            ActualizarEstado("¡Ganó la computadora!");
            return true;
        }

        if (TableroLleno())
        {
            partidaTerminada = true;
            ActualizarEstado("¡Empate!");
            return true;
        }

        return false;
    }

    private void ActualizarEstado(string mensaje)
    {
        if (textoEstado != null)
            textoEstado.text = mensaje;
    }

    public void ReiniciarPartida()
    {
        for (int i = 0; i < tablero.Length; i++)
        {
            tablero[i] = 0;
            LimpiarImagen(i);
            if (casillas[i] != null)
                casillas[i].interactable = true;
        }

        partidaTerminada = false;
        turnoJugador = true;
        ActualizarEstado("Tu turno");
    }
}

