
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class FlappyController : MonoBehaviour
{
    [Header("Salto y fisica")]
    [SerializeField] private float flapSpeed = 5.5f;
    [SerializeField] private float gravityDuringGame = 2f;

    [Header("Rotacion del gatito")]
    [SerializeField] private float neutralAngle = -25f;
    [SerializeField] private float jumpingAngle = -12f;
    [SerializeField] private float fallingAngle = -38f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Animacion al perder")]
    [SerializeField] private float deathAngle = -100f;
    [SerializeField] private float deathGravity = 14f;
    [SerializeField] private float maxDeathSpeed = 12f;
    [SerializeField] private float deathRotationSpeed = 210f;

    [Header("Limites de pantalla")]
    [SerializeField] private float screenMargin = 0.3f;

    private Rigidbody2D rb;
    private Camera gameCamera;
    private SpriteRenderer playerRenderer;

    private bool isFallingAfterDeath;
    private float deathFallSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main;
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // =====================================
    // PREPARAR AL GATITO
    // =====================================

    public void PrepareForStart()
    {
        isFallingAfterDeath = false;
        deathFallSpeed = 0f;

        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        SetRotation(neutralAngle);
    }

    // =====================================
    // EMPEZAR A JUGAR
    // =====================================

    public void ActivatePhysics()
    {
        rb.simulated = true;
        rb.gravityScale = gravityDuringGame;
    }

    // =====================================
    // CUANDO PIERDE
    // =====================================

    public void StopPhysics()
    {
        if (isFallingAfterDeath)
        {
            return;
        }

        isFallingAfterDeath = true;
        deathFallSpeed = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Permite animar la caida sin bloquearse
        // contra las vigas.
        rb.simulated = false;
    }

    // =====================================
    // ACTUALIZACION
    // =====================================

    private void Update()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        if (game.State == FlappyGameState.Ready)
        {
            if (PressedThisFrame())
            {
                game.StartRun();
                Flap();
            }

            return;
        }

        if (game.State == FlappyGameState.Playing)
        {
            if (PressedThisFrame())
            {
                Flap();
            }

            UpdateRotation();
        }
        else if (game.State == FlappyGameState.GameOver)
        {
            UpdateDeathAnimation();
        }

        // Cuando esta en pausa no modifica
        // la velocidad ni la rotacion.
    }

    // =====================================
    // LIMITES SIN TERMINAR LA PARTIDA
    // =====================================

    private void FixedUpdate()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null ||
            game.State != FlappyGameState.Playing ||
            gameCamera == null ||
            isFallingAfterDeath)
        {
            return;
        }

        KeepInsideScreen();
    }

    private void KeepInsideScreen()
    {
        float cameraY = gameCamera.transform.position.y;
        float halfHeight = gameCamera.orthographicSize;

        float playerHalfHeight =
            playerRenderer != null
                ? playerRenderer.bounds.extents.y
                : 0.45f;

        float minY =
            cameraY - halfHeight +
            playerHalfHeight + screenMargin;

        float maxY =
            cameraY + halfHeight -
            playerHalfHeight - screenMargin;

        if (minY >= maxY)
        {
            return;
        }

        float currentY = rb.position.y;

        float newY = Mathf.Clamp(
            currentY,
            minY,
            maxY
        );

        if (!Mathf.Approximately(currentY, newY))
        {
            rb.position = new Vector2(
                rb.position.x,
                newY
            );
        }

        float verticalSpeed = rb.linearVelocity.y;

        if ((newY <= minY && verticalSpeed < 0f) ||
            (newY >= maxY && verticalSpeed > 0f))
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );
        }
    }

    // =====================================
    // SALTO
    // =====================================

    private void Flap()
    {
        rb.linearVelocity = new Vector2(
            0f,
            flapSpeed
        );

        FlappyGameController.Instance?.PlayFlapSound();
    }

    // =====================================
    // ROTACION DURANTE EL JUEGO
    // =====================================

    private void UpdateRotation()
    {
        float speedY = rb.linearVelocity.y;

        float targetAngle = neutralAngle;

        if (speedY > 0.4f)
        {
            targetAngle = jumpingAngle;
        }
        else if (speedY < -0.4f)
        {
            targetAngle = fallingAngle;
        }

        float nextAngle = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            targetAngle,
            rotationSpeed * Time.deltaTime
        );

        SetRotation(nextAngle);
    }

    // =====================================
    // CAIDA AL PERDER
    // =====================================

    private void UpdateDeathAnimation()
    {
        if (!isFallingAfterDeath)
        {
            return;
        }

        deathFallSpeed += deathGravity * Time.deltaTime;

        deathFallSpeed = Mathf.Min(
            deathFallSpeed,
            maxDeathSpeed
        );

        transform.position +=
            Vector3.down *
            deathFallSpeed *
            Time.deltaTime;

        float nextAngle = Mathf.MoveTowardsAngle(
            transform.eulerAngles.z,
            deathAngle,
            deathRotationSpeed * Time.deltaTime
        );

        SetRotation(nextAngle);
    }

    private void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }

    // =====================================
    // ENTRADAS: ESPACIO, CLICK Y TOQUE
    // =====================================

    private static bool PressedThisFrame()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM

        pressed |= Keyboard.current != null &&
                   Keyboard.current.spaceKey.wasPressedThisFrame;

        pressed |= Mouse.current != null &&
                   Mouse.current.leftButton.wasPressedThisFrame;

        pressed |= Touchscreen.current != null &&
                   Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

#endif

#if ENABLE_LEGACY_INPUT_MANAGER

        pressed |= Input.GetKeyDown(KeyCode.Space);

        pressed |= Input.GetMouseButtonDown(0);

        pressed |= Input.touchCount > 0 &&
                   Input.GetTouch(0).phase == TouchPhase.Began;

#endif

        return pressed;
    }

    // =====================================
    // COLISION CON LAS VIGAS
    // =====================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null ||
            game.State != FlappyGameState.Playing)
        {
            return;
        }

        FlappyObstacle obstacle =
            collision.gameObject.GetComponentInParent<FlappyObstacle>();

        if (obstacle != null)
        {
            game.EndRun();
        }
    }
}
