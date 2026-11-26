
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FlappyGround : MonoBehaviour
{
    private void Awake()
    {
        // El suelo funciona como una zona
        // de deteccion invisible.
        BoxCollider2D collider =
            GetComponent<BoxCollider2D>();

        collider.isTrigger = true;
    }

    // Cuando el gatito entra al suelo.
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckGroundCollision(other);
    }

    // Tambien comprueba si el gatito ya
    // estaba tocando el suelo.
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckGroundCollision(other);
    }

    private void CheckGroundCollision(Collider2D other)
    {
        // Comprobar que sea nuestro gatito.
        FlappyController player =
            other.GetComponentInParent<FlappyController>();

        if (player == null)
        {
            return;
        }

        // Buscar el controlador principal.
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        // Solo se puede perder
        // si la partida esta activa.
        if (game.State != FlappyGameState.Playing)
        {
            return;
        }

        Debug.Log(
            "FLAPPY SUSHI: el gatito toco el suelo. GAME OVER."
        );

        // Usar el mismo Game Over que
        // cuando chocamos con una viga.
        game.EndRun();
    }
}
