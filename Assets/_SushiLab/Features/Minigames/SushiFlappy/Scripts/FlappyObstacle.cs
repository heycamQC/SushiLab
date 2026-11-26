using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlappyObstacle : MonoBehaviour
{
    [Header("Movimiento de las vigas")]
    [SerializeField] private float speed = 3.25f;
    [SerializeField] private float destroyX = -11f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (FlappyGameController.Instance == null ||
            FlappyGameController.Instance.State != FlappyGameState.Playing)
            return;

        rb.MovePosition(rb.position + Vector2.left * speed * Time.fixedDeltaTime);

        if (rb.position.x < destroyX)
            Destroy(gameObject);
    }
}
