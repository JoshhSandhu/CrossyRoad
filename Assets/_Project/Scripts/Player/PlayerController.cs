using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static event Action OnPlayerMovedForward;  //player moved forward event
    public static event Action<int> OnScoreChanged;   //event for score change

    [Header("Dependancies")]
    [SerializeField]
    private CamFollow cameraFollow;

    // public feilds
    [Tooltip("hight of the player hops")]
    [SerializeField]
    private float hopHeight = 0.5f;

    [Tooltip("duration of the player hops")]
    [SerializeField]
    private float hopDuration = 0.2f;
    [SerializeField]
    private float xBoundary = 5f;
    [SerializeField]
    private LayerMask blockingMask; //solid obstacles and Boundary
    [SerializeField]
    private LayerMask groundMask; //for the ground masks (ground, water, platform)
    [SerializeField]
    private Vector3 blockCastHalfExtents = new Vector3(0.3f, 0.5f, 0.3f);

    [Header("Death Scatter Settings")]
    [SerializeField]
    private ParticleSystem scatterParticleSystem;

    [Header("Rotation")]
    //[SerializeField]
    //private bool smoothTurn = true;
    //[SerializeField]
    //private float turnSpeed = 20f;

    //private feilds
    private Rigidbody playerRb;
    private bool isMoving = false;  //to check if the player is moving
    private int forwardPosZ = 0;    //to track the forward position of the player
    private bool isDead = false;    //to keep track if the player is alive or dead
    private bool hasScattered = false;
    private Quaternion targetRotation; //to store the rotation of the player
    private Vector2 touchStartPos;      //for the new input system var
    private float minSwipeDist = 50f;   //minimum distance for a swipe to be registered
    private PlayerInputActions playerInputActions;

    // SOLID Principle Components - Dependency Injection
    private IGameStateManager gameStateManager;
    private IAuthenticationManager authenticationManager;
    private IMovementValidator movementValidator;
    private IMovementExecutor movementExecutor;
    private ICollisionHandler collisionHandler;
    private HybridTransactionService hybridTransactionService;
    private ICameraController cameraController;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        playerRb.interpolation = RigidbodyInterpolation.Interpolate;
        playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        playerRb.freezeRotation = true;
        TouchSimulation.Enable();

        // Initialize SOLID Principle Components
        InitializeComponents();
    }

    /// <summary>
    /// Initialize all SOLID principle components with dependency injection
    /// </summary>
    private void InitializeComponents()
    {
        // Initialize adapters for existing singleton managers
        gameStateManager = new GameStateManagerAdapter();
        authenticationManager = new AuthenticationManagerAdapter();
        cameraController = new CameraControllerAdapter(cameraFollow);

        // Initialize components with dependencies
        movementValidator = new PlayerMovementValidator(transform, xBoundary, blockingMask, groundMask, blockCastHalfExtents);
        movementExecutor = new PlayerMovementExecutor(transform, playerRb, hopHeight, hopDuration, groundMask);

        // Initialize collision handler with callbacks
        collisionHandler = new PlayerCollisionHandler(transform, xBoundary, GameOver, () => gameStateManager.AddCoins());

        // Initialize Hybrid transaction service
        hybridTransactionService = HybridTransactionService.Instance;
        if (hybridTransactionService == null)
        {
            Debug.LogError("HybridTransactionService not found! Creating new instance...");
            var serviceObject = new GameObject("HybridTransactionService");
            hybridTransactionService = serviceObject.AddComponent<HybridTransactionService>();
        }
    }

    //subscribe the events
    private void OnEnable()
    {
        if (playerInputActions == null)
        {
            playerInputActions = new PlayerInputActions();
        }

        // Subscribe to keyboard/gamepad movement
        playerInputActions.Player.Move.performed += OnMove;

        // Subscribe to touch input
        playerInputActions.Player.TouchTap.performed += OnTouchTap;
        playerInputActions.Player.TouchSwipe.performed += OnTouchSwipe;
        playerInputActions.Player.TouchPosition.performed += OnTouchPosition;

        playerInputActions.Enable();
    }

    //unsubscribe the events
    private void OnDisable()
    {
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.TouchTap.performed -= OnTouchTap;
        playerInputActions.Player.TouchSwipe.performed -= OnTouchSwipe;
        playerInputActions.Player.TouchPosition.performed -= OnTouchPosition;
        playerInputActions.Player.Disable();
    }

    private void Update()
    {
        collisionHandler?.CheckForOutOfBounds();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = context.ReadValue<Vector2>();
        float deadZone = 0.1f;
        if (Mathf.Abs(inputDirection.x) < deadZone) inputDirection.x = 0;
        if (Mathf.Abs(inputDirection.y) < deadZone) inputDirection.y = 0;

        //vertical movement takes priority over horizontal movement
        if (Mathf.Abs(inputDirection.y) > Mathf.Abs(inputDirection.x))
        {
            if (inputDirection.y > 0)
            {
                movePlayer(Vector3.forward);
            }
            else if (inputDirection.y < 0)
            {
                movePlayer(Vector3.back);
            }
        }
        else if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))  //remove this condition to allow diagonal movement
        {
            if (inputDirection.x > 0)
            {
                movePlayer(Vector3.right);
            }
            else if (inputDirection.x < 0)
            {
                movePlayer(Vector3.left);
            }
        }
    }

    /// <summary>
    /// touch input for movement
    /// </summary>
    private Vector2 touchStartPosition;
    private bool isTouching = false;
    private float minSwipeDistance = 50f; //min distance for a swipe to be registered

    private void OnTouchTap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Touch tap detected - moving forward");
            movePlayer(Vector3.forward);
        }
    }

    private void OnTouchSwipe(InputAction.CallbackContext context)
    {
        Vector2 swipeDelta = context.ReadValue<Vector2>();

        if (isTouching && swipeDelta.magnitude > 0.1f)
        {
            // Determine swipe direction
            Vector3 moveDirection = GetSwipeDirection(swipeDelta);
            if (moveDirection != Vector3.zero)
            {
                Debug.Log($"Touch swipe detected: {swipeDelta} -> {moveDirection}");
                movePlayer(moveDirection);
            }
        }
    }

    private void OnTouchPosition(InputAction.CallbackContext context)
    {
        Vector2 touchPosition = context.ReadValue<Vector2>();

        if (context.performed)
        {
            // Touch started
            touchStartPosition = touchPosition;
            isTouching = true;
        }
        else if (context.canceled)
        {
            // Touch ended
            Vector2 touchEndPosition = touchPosition;
            Vector2 swipeVector = touchEndPosition - touchStartPosition;

            if (swipeVector.magnitude >= minSwipeDistance)
            {
                // It was a swipe
                Vector3 moveDirection = GetSwipeDirection(swipeVector);
                if (moveDirection != Vector3.zero)
                {
                    Debug.Log($"Touch swipe completed: {swipeVector} -> {moveDirection}");
                    movePlayer(moveDirection);
                }
            }
            else
            {
                // It was a tap
                Debug.Log("Touch tap completed - moving forward");
                movePlayer(Vector3.forward);
            }

            isTouching = false;
        }
    }

    /// <summary>
    /// Convert swipe vector to movement direction for Crossy Road style movement
    /// </summary>
    private Vector3 GetSwipeDirection(Vector2 swipeVector)
    {
        Vector2 normalizedSwipe = swipeVector.normalized;

        if (Mathf.Abs(normalizedSwipe.y) > Mathf.Abs(normalizedSwipe.x))
        {
            if (normalizedSwipe.y > 0)
            {
                return Vector3.forward;     // Swipe up = move forward
            }
            else
            {
                return Vector3.back;        // Swipe down = move backward
            }
        }
        else
        {
            if (normalizedSwipe.x > 0)
            {
                return Vector3.right;       // Swipe right = move right
            }
            else
            {
                return Vector3.left;        // Swipe left = move left
            }
        }
    }

    private void movePlayer(Vector3 direction)
    {
        if (isMoving || isDead)
        {
            return;
        }

        //check if authentication flow is active and game is not ready
        if (authenticationManager != null && !authenticationManager.IsGameReady())
        {
            return;
        }
        //if authentication is complete but game hasn't started yet, start the game
        if (authenticationManager != null && authenticationManager.IsGameReady() && !gameStateManager.IsGameActive())
        {
            Debug.Log("Authentication complete, starting game...");
            authenticationManager.StartGame();
            return;
        }
        //if no authentication flow but game is not active, start the game
        if (authenticationManager == null && !gameStateManager.IsGameActive())
        {
            if (StartScreenManager.Instance != null)
            {
                Debug.Log("No authentication flow, starting game from start screen...");
                StartScreenManager.Instance.StartGame();
            }
            return;
        }

        // Use movement validator to check if movement is valid
        if (!movementValidator.CanMove(direction))
        {
            return;
        }

        Vector3 snappedPos = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y + 0.5f,
            Mathf.Round(transform.position.z)
        );

        Vector3 destination = snappedPos + direction;

        //function to turn the player the ideration we are moving
        transform.SetParent(null);
        movementExecutor.FaceDirection(direction);
        StartCoroutine(ExecuteMovementWithCallback(destination));

        if (direction == Vector3.forward && (int)destination.z > forwardPosZ)
        {
            forwardPosZ = (int)destination.z;
            OnPlayerMovedForward?.Invoke();
            OnScoreChanged?.Invoke(forwardPosZ);
        }

        // Send Solana transaction for player movement
        hybridTransactionService?.SendMovementTransaction(direction);
    }

    /// <summary>
    /// Execute movement with callback handling for water collision
    /// </summary>
    private IEnumerator ExecuteMovementWithCallback(Vector3 destination)
    {
        isMoving = true;
        yield return StartCoroutine(movementExecutor.ExecuteMovement(destination));

        // Check if player landed in water (handled by movement executor)
        if (Physics.Raycast(destination + Vector3.up * 0.5f, Vector3.down, out var landinghit, 1.5f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (landinghit.collider.CompareTag("Water"))
            {
                GameOver(true);
            }
        }

        cameraController?.Shake(0.15f); //camera shake at the end of an hop
        isMoving = false;
    }

    //game over logic
    private void GameOver(bool isDrowning)
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Game Over!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        if (isDrowning)
        {
            transform.SetParent(null); //detach from the log if we are attched to it
            cameraController?.SetEnabled(false);
            StartCoroutine(DrownCoroutine());
        }
        else
        {
            ScatterIntoBlocks();
        }
    }

    private IEnumerator DrownCoroutine()
    {
        float Duration = 1f;
        float elapsedTime = 0f;
        Vector3 startpos = transform.position;
        Vector3 endpos = startpos - new Vector3(0, 4, 0); //to sink the player

        while (elapsedTime < Duration)
        {
            Vector3 newpos = Vector3.Lerp(startpos, endpos, elapsedTime / Duration);
            playerRb.MovePosition(newpos);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    //collision detection
    private void OnTriggerEnter(Collider other)
    {
        collisionHandler?.HandleTriggerEnter(other);
    }

    //this is for physical Collisions (like trees and rocks) that only block you
    private void OnCollisionEnter(Collision collision)
    {
        collisionHandler?.HandleCollisionEnter(collision);
    }

    private void ScatterIntoBlocks()
    {
        if (hasScattered) return;
        hasScattered = true;

        // disable visuals and interaction of the player
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        cameraController?.Shake(0.6f);

        ParticleSystem ps = Instantiate(scatterParticleSystem, transform.position, transform.rotation);
        var main = ps.main;
        ps.Play();
        Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax + 0.5f);

    }

    public void ResetPlayer()
    {
        // Reset death state
        isDead = false;
        hasScattered = false;
        isMoving = false;
        forwardPosZ = 0;

        // Reset position
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.SetParent(null);

        // Re-enable camera follow
        cameraController?.SetEnabled(true);
        cameraController?.ResetCamera();

        // Re-enable renderers and colliders
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = true;
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = true;

        // Reset rigidbody
        if (playerRb != null)
        {
            if (!playerRb.isKinematic)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
    }

}