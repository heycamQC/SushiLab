
using UnityEngine;
using System.Collections;

public class SushiPiece : MonoBehaviour
{
    public TowerGameController gameController;
    public TowerSpawner spawner;

    private Rigidbody2D rb;
    private Collider2D pieceCollider;

    private bool hasDropped = false;
    private bool hasScored = false;
    private bool checking = false;

    private float supportHeight;
    private float settledHeight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pieceCollider = GetComponent<Collider2D>();
    }

    public void Initialize(
        TowerGameController controller,
        TowerSpawner towerSpawner,
        float currentSupportHeight)
    {
        gameController = controller;
        spawner = towerSpawner;
        supportHeight = currentSupportHeight;
    }

    public void SetDropped()
    {
        hasDropped = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!hasDropped || hasScored || checking)
            return;

        if (collision.gameObject.CompareTag("Platform") ||
            collision.gameObject.CompareTag("Sushi"))
        {
            StartCoroutine(CheckStability());
        }
    }

    IEnumerator CheckStability()
    {
        checking = true;

        yield return new WaitForSeconds(0.4f);

        if (gameController.gameOver)
        {
            checking = false;
            yield break;
        }

        if (rb.linearVelocity.magnitude < 0.65f &&
            Mathf.Abs(rb.angularVelocity) < 35f)
        {
            hasScored = true;
            settledHeight = transform.position.y;

            gameController.AddPoint(
                pieceCollider.bounds.max.y
            );

            spawner.PieceSettled();
        }

        checking = false;
    }

    void Update()
    {
        if (!hasDropped || gameController == null ||
            gameController.gameOver)
            return;

        if (Mathf.Abs(transform.position.x) > 4f)
        {
            gameController.GameOver();
            return;
        }

        if (!hasScored)
        {
            if (transform.position.y < supportHeight - 2.2f)
                gameController.GameOver();
        }
        else
        {
            if (transform.position.y < settledHeight - 2f)
                gameController.GameOver();
        }
    }
}
