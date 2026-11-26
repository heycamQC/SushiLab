using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ScoreZone : MonoBehaviour
{
    private bool counted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (counted || !other.CompareTag("Player")) return;

        FlappyGameController game = FlappyGameController.Instance;
        if (game == null || game.State != FlappyGameState.Playing) return;

        counted = true;
        game.AddScore();
    }
}
